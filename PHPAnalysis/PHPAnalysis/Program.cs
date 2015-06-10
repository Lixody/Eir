using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CommandLine;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Components;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.IO.Cmd;
using PHPAnalysis.Parsing;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.Exceptions;
using File = PHPAnalysis.Data.File;

[assembly: InternalsVisibleTo("PHPAnalysis.Tests")]
namespace PHPAnalysis
{
    static class Program
    {
        //HACK: there must be a better way to handle this ! :O 
        public static Config Configuration;
        private static ComponentContainer _components;

        static void Main(string[] args)
        {
            Arguments arguments = ParseArguments(args);
            Configuration = GetConfiguration(arguments.ConfigLocation);
            Config configuration = Configuration;
            FunctionsHandler.Instance.FunctionSpecification = configuration.FuncSpecSettings;
            FunctionsHandler.Instance.LoadJsonSpecifications();

            _components = ImportExternalComponents(configuration.ComponentSettings);
            Analyze(arguments, configuration);
        }

        private static void WPGotoAnalysis(Arguments arguments, Config configuration)
        {
            var v = Stopwatch.StartNew();
            var folders = Directory.GetDirectories(@"G:\WP");
            int counter = 0;

            var progress = new BikeGuyRidingAnimation(folders.Count());

            var locker = new object();

            Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = 7 },
                folder =>
                {
                    int myNumber = Interlocked.Increment(ref counter);
                    arguments.Target = Path.Combine(arguments.Target, folder);
                    if (!Directory.Exists(arguments.Target))
                        return;

                    var projectParser = new ProjectParser(arguments.Target, configuration.PHPSettings);
                    ParseResult parseResult = projectParser.ParseProjectFiles();

                    foreach (var parsedFile in parseResult.ParsedFiles)
                    {
                        //Console.WriteLine("File: " + parsedFile.Key);
                        var traverser = new XmlTraverser();
                        var metricVisitor = new MetricVisitor();
                        traverser.AddVisitor(metricVisitor);
                        traverser.Traverse(parsedFile.Value);

                        if (metricVisitor.Gotos > 0)
                        {
                            lock (locker)
                            {
                                System.IO.File.AppendAllLines(@"C:/pluginDLMessages.txt", new [] { "Goto found in " + parsedFile.Key});
                            }
                        }
                    }

                    //Console.WriteLine(folder);

                    if ((myNumber % 250) == 0)
                    {
                        Console.WriteLine(myNumber + " plugins scanned..");
                    }
                });
            Console.WriteLine(v.Elapsed);
            Environment.Exit(1);
        }

        private static void Analyze(Arguments arguments, Config configuration)
        {
            Console.WriteLine("Parsing project at: " + arguments.Target);
            Console.WriteLine();

            foreach (var analysisStartingListener in _components.AnalysisStartingListeners)
            {
                // TODO - This should probably be a proper event - same goes for EndingEvent (this will also remove the loop(s)).
                analysisStartingListener.AnalysisStarting(null, new AnalysisStartingEventArgs(configuration, arguments));
            }

            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Building ASTs..");
            ParseResult parseResult = ParseTarget(arguments, configuration);

            Console.WriteLine(" - AST build for {0} files ({1} failed)..", parseResult.ParsedFiles.Count, parseResult.FilesThatFailedToParse.Count);
            Console.WriteLine("Traversing ASTs..");

            var filesCollection = new List<File>();
            var runningVulnReporter = new CompositeVulneribilityReporter(_components.VulnerabilityReporters);
            var vulnerabilityStorage = new ReportingVulnerabilityStorage(runningVulnReporter);

            var progrssIndicator = ProgressIndicatorFactory.CreateProgressIndicator(parseResult.ParsedFiles.Count());
            foreach (var parsedFile in parseResult.ParsedFiles)
            {
                progrssIndicator.Step();

                var file = BuildFileCFGAndExtractFileInformation(parsedFile);
                filesCollection.Add(file);
            }

            Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks,
                 ImmutableVariableStorage> fileTaintAnalyzer = null;
            fileTaintAnalyzer = (varStorage, inclResolver, scope, stacks) =>
            {
                Preconditions.NotNull(varStorage, "varStorage");
                Preconditions.NotNull(inclResolver, "inclResolver");

                var blockAnalyzer = new TaintBlockAnalyzer(vulnerabilityStorage, inclResolver, scope, fileTaintAnalyzer, stacks);
                blockAnalyzer.AnalysisExtensions.AddRange(_components.BlockAnalyzers);
                var condAnalyser = new ConditionTaintAnalyser(scope, inclResolver, stacks.IncludeStack);
                var cfgTaintAnalysis = new TaintAnalysis(blockAnalyzer, condAnalyser, varStorage);
                var fileToAnalyze = stacks.IncludeStack.Peek();
                var analyzer = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new ReversePostOrderWorkList(fileToAnalyze.CFG));
                //var analyzer = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new QueueWorklist());
                analyzer.Analyze(fileToAnalyze.CFG);
                return cfgTaintAnalysis.Taints[fileToAnalyze.CFG.Vertices.Single(block => block.IsLeaf)].Out[EdgeType.Normal];
            };

            foreach (var file in filesCollection)
            {
                Console.WriteLine(Environment.NewLine + "=============================");
                Console.WriteLine("Analyzing {0}..", file.FullPath);
                var initialTaint = GetDefaultTaint();
                var inclusionResolver = new IncludeResolver(filesCollection);

                var stacks = new AnalysisStacks(file);
                fileTaintAnalyzer(initialTaint, inclusionResolver, AnalysisScope.File, stacks);
            }

            Console.WriteLine("Scanned {0}/{1} subroutines. ", FunctionsHandler.Instance.ScannedFunctions.Count, FunctionsHandler.Instance.CustomFunctions.Count);

            if (arguments.ScanAllSubroutines)
            {
                Console.WriteLine("Scanning remaining subroutines..");
                ScanUnscannedSubroutines(filesCollection, fileTaintAnalyzer, vulnerabilityStorage);
            }

            vulnerabilityStorage.CheckForStoredVulnerabilities();
            //parseResult.ParsedFiles.Values.First().Save(@"C:\Users\Kenneth\Documents\Uni\TestScript\current\parsedFile");

            stopwatch.Stop();

            foreach (var analysisEndedListener in _components.AnalysisEndedListeners)
            {
                analysisEndedListener.AnalysisEnding(null, new AnalysisEndedEventArgs(stopwatch.Elapsed));
            }

            Console.WriteLine("Time spent: " + stopwatch.Elapsed);
            Console.WriteLine("Found {0} vulnerabilities.", runningVulnReporter.NumberOfReportedVulnerabilities);
        }

        private static ImmutableVariableStorage GetDefaultTaint()
        {
            var defaultTaint = new DefaultTaintProvider().GetTaint();
            if (_components.TaintProviders.Count == 1)
            {
                return _components.TaintProviders.Single().GetTaint();
            }
            if (_components.TaintProviders.Count > 1)
            {
                Console.WriteLine("Found multiple taint providers. Can't decide which one to use. Using builtin default taint.");
            }
            
            return defaultTaint;
        }

        private static void ScanUnscannedSubroutines(List<File> filesCollection, Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> fileTaintAnalyzer, ReportingVulnerabilityStorage vulnerabilityStorage)
        {
            var defaultTaint = new DefaultTaintProvider().GetTaint();

            foreach (var file in filesCollection)
            {
                var analysisStacks = new AnalysisStacks(file);
                var analyser = new FunctionAndMethodAnalyzer(defaultTaint,
                    new IncludeResolver(filesCollection), analysisStacks,
                    new CustomFunctionHandler(fileTaintAnalyzer), vulnerabilityStorage);

                foreach (var function in file.Functions.SelectMany(f => f.Value).Except(FunctionsHandler.Instance.ScannedFunctions))
                {
                    var functionCall = new FunctionCall(function.Name, function.AstNode, 0, 0);
                    analysisStacks.CallStack.Push(functionCall);

                    analyser.AnalyzeFunctionCall(functionCall, new List<ExpressionInfo>());
                }
                foreach (var @class in file.Classes.SelectMany(c => c.Value))
                {
                    foreach (var method in @class.Methods.Except(FunctionsHandler.Instance.ScannedFunctions))
                    {
                        var methodCall = new MethodCall(method.Name, new [] { @class.Name }, method.AstNode, 0, 0);
                        analysisStacks.CallStack.Push(methodCall);

                        analyser.AnalyzeMethodCall(methodCall, new List<ExpressionInfo>());
                    }
                }
            }
        }

        private static File BuildFileCFGAndExtractFileInformation(KeyValuePair<string, XmlDocument> parsedFile)
        {
            var traverser = new XmlTraverser();
            var metricAnalyzer = new MetricVisitor();
            var extractor = new ClassAndFunctionExtractor();
            var printer = new ASTPrinter(Console.Out);
            var cfgcreator = new CFGCreator();
            traverser.AddVisitor(extractor);
            traverser.AddVisitor(metricAnalyzer);
            traverser.AddVisitor(cfgcreator);
            //traverser.AddVisitor(printer);
            traverser.AddVisitors(_components.AstVisitors.ToArray());

            traverser.Traverse(parsedFile.Value.FirstChild.NextSibling);

            foreach (var function in extractor.Functions)
            {
                function.File = parsedFile.Key;
            }
            foreach (var closure in extractor.Closures)
            {
                closure.File = parsedFile.Key;
            }

            FunctionsHandler.Instance.CustomFunctions.AddRange(extractor.Functions);

            foreach (var @class in extractor.Classes)
            {
                @class.File = parsedFile.Key;
                foreach (var method in @class.Methods)
                {
                    //HACK: This is not a good way to handle this! Should we add a new derived function class called method that includes the class name
                    //-||-: and make a special list for them in the function handler, or is this okay?
                    method.Name = @class.Name + "->" + method.Name;
                    method.File = parsedFile.Key;
                    FunctionsHandler.Instance.CustomFunctions.Add(method);
                }
            }

            //cfgcreator.Graph.VisualizeGraph("graph", Program.Configuration.GraphSettings);
            var cfgPruner = new CFGPruner();
            cfgPruner.Prune(cfgcreator.Graph);
            //cfgcreator.Graph.VisualizeGraph("graph-pruned", Configuration.GraphSettings);

            File file = new File(parsedFile.Value) {
                                                       CFG = cfgcreator.Graph,
                                                       FullPath = parsedFile.Key,
                                                       Interfaces = extractor.Interfaces.GroupBy(i => i.Name, i => i).ToDictionary(i => i.Key, i => i.ToList()),
                                                       Classes = extractor.Classes.GroupBy(c => c.Name, c => c).ToDictionary(c => c.Key, c => c.ToList()),
                                                       Closures = extractor.Closures.ToArray(),
                                                       Functions = extractor.Functions.GroupBy(i => i.Name, i => i).ToDictionary(i => i.Key, i => i.ToList())
                                                   };
            return file;
        }

        private static ParseResult ParseTarget(Arguments arguments, Config configuration)
        {
            if (Directory.Exists(arguments.Target))
            {
                return ParseDirectoryFiles(configuration, arguments);
            }
            if (System.IO.File.Exists(arguments.Target))
            {
                return ParseFile(configuration, arguments);
            }
            Console.WriteLine("Target does not seem to be a valid directory or file.");
            Environment.Exit(1);
            return null;
        }

        private static ParseResult ParseFile(Config configuration, Arguments arguments)
        {
            var fileParser = new FileParser(configuration.PHPSettings.PHPParserPath);
            var result = fileParser.ParsePHPFile(arguments.Target);
            var parseResult = new ParseResult();
            parseResult.ParsedFiles.Add(arguments.Target, result);
            return parseResult;
        }

        private static ParseResult ParseDirectoryFiles(Config configuration, Arguments arguments)
        {
            var projectParser = new ProjectParser(arguments.Target, configuration.PHPSettings);
            ParseResult parseResult = projectParser.ParseProjectFiles();
            return parseResult;
        }

        private static ComponentContainer ImportExternalComponents(ComponentConfiguration configuration)
        {
            if (!configuration.IncludeComponents)
            {
                return new ComponentContainer();
            }
            try
            {
                var importer = new ComponentImporter();
                var components = importer.ImportComponents(configuration.ComponentPath);

                Console.WriteLine("Components:");
                Console.WriteLine("  Found {0} visitors.", components.AstVisitors.Count);
                Console.WriteLine("  Found {0} block analyzers.", components.BlockAnalyzers.Count);
                Console.WriteLine("  Found {0} reporters.", components.VulnerabilityReporters.Count);
                Console.WriteLine("  Found {0} taint providers.", components.TaintProviders.Count);

                return components;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("EXTERNAL COMPONENTS ERROR: ");
                Console.WriteLine("Could not find specified component folder ({0}). Please make sure to set the correct folder in the config file.", configuration.ComponentPath);
                Environment.Exit(1);
                return null;
            }
        }

        private static void PrintResults(File file, MetricVisitor metricAnalyzer, TextWriter writer)
        {
            writer.WriteLine("File: " + file.FullPath);
            writer.WriteLine(" - Total AST nodes: " + metricAnalyzer.TotalNodes);
            writer.WriteLine(" - Echo statements: " + metricAnalyzer.EchoStatements);
            writer.WriteLine(" - Sql query strings: " + metricAnalyzer.PotentialSQLQueries);
            writer.WriteLine(" - Functions: " + file.Functions.Count);
            foreach (var function in file.Functions.Values)
            {
                writer.WriteLine("   - " + function);
            }
            writer.WriteLine(" - Classes: " + file.Classes.Count);
            foreach (var classDef in file.Classes.Values.SelectMany(classDefinition => classDefinition))
            {
                writer.WriteLine("   - {0} {1} {2}", classDef.Name, classDef.StartLine, classDef.EndLine);
                writer.WriteLine("     - Methods: " + classDef.Methods.Count);
            }
            writer.WriteLine(" - Interfaces: " + file.Interfaces.Count);
            foreach (var interfaceDef in file.Interfaces.Values.SelectMany(interfaceDef => interfaceDef))
            {
                writer.WriteLine("  - {0} {1} {2}", interfaceDef.Name, interfaceDef.StartLine, interfaceDef.EndLine);
                writer.WriteLine("    - Methods: " + interfaceDef.Methods.Count);
            }
            writer.WriteLine(" - Closures: " + file.Closures.Length);
            foreach (var closure in file.Closures)
            {
                writer.WriteLine("   - " + closure);
            }

            writer.WriteLine();
        }

        private static Config GetConfiguration(string configLocation)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(configLocation))
                {
                    return Config.ReadConfiguration("config.yml");
                }
                return Config.ReadConfiguration(configLocation);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("CONFIGURATION ERROR: ");
                Console.WriteLine("Could not find configuration file. Please make sure you have created the required 'config.yml' file.");
            }
            catch (ConfigurationParseException e)
            {
                Console.WriteLine("CONFIGURATION ERROR: ");
                Console.WriteLine("Could not parse configuration file (config.yml). Please make sure the file is in correct Yaml format. ");
            }
            Environment.Exit(1);
            return null; 
        }

        private static Arguments ParseArguments(string[] args)
        {
            var arguments = new Arguments();
            if (Parser.Default.ParseArguments(args, arguments))
            {
                return arguments;
            }
            Environment.Exit(1);
            return null;
        }
    }
}