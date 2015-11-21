using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data.PHP;
using System;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Configuration;
using YamlDotNet.Core.Tokens;

namespace PHPAnalysis.Analysis.PHPDefinitions
{
    // TODO - This should not be a singleton! Currently our analysis is dependent on this singleton being filled
    //        with functions, _before_ the analysis start. This is easy to forget and therefore an obvious place for mistakes (already happened once).
    //        APIs should be easy to use, but hard to misuse.
    public sealed class FunctionsHandler
    {
        public List<Source> Sources { get; private set; }
        public List<SQLSanitizer> SQLSanitizers { get; private set; }
        public List<XSSSanitizer> XSSSanitizers { get; private set; }
        public List<CondSanitizer> CondSanitizers { get; private set; } 
        public List<SQLSink> SQLSinks { get; private set; }
        public List<XSSSink> XSSSinks { get; private set; }
        public List<Function> CustomFunctions { get; private set; }
        public List<Function> StoredProviders { get; set; }
        public HashSet<Function> ScannedFunctions { get; private set; }

        public Dictionary<Function, List<FunctionSummary>> FunctionSummaries { get; private set; } 

        /// <summary>
        /// This will magically be set before first usage. If not, everything will crash and burn..
        /// </summary>
        /// <returns></returns>
        public FuncSpecConfiguration FunctionSpecification { get; set; }

        //Crazy big ctor loading JSON specifications
        public FunctionsHandler()
        {
            Sources = new List<Source>();
            SQLSanitizers = new List<SQLSanitizer>();
            XSSSanitizers = new List<XSSSanitizer>();
            CondSanitizers = new List<CondSanitizer>();
            SQLSinks = new List<SQLSink>();
            XSSSinks = new List<XSSSink>();
            CustomFunctions = new List<Function>();
            StoredProviders = new List<Function>();
            ScannedFunctions = new HashSet<Function>();
            FunctionSummaries = new Dictionary<Function, List<FunctionSummary>>();
        }

        public void LoadJsonSpecifications()
        {
            var jsonFiles = new List<string>();

            foreach (var filePath in FunctionSpecification.PHPSpecs)
            {
                try
                {
                    jsonFiles.AddRange(Directory.GetFiles(filePath, "*.json").ToList());
                }
                catch (DirectoryNotFoundException)
                {
                    //TODO: Should we stop analysis here, or should we let the exception be thrown or somethign else?
                    Console.WriteLine("Could not find the given specification path! Stopping analysis");
                    Environment.Exit(100);
                }
            }

            foreach (var filePath in FunctionSpecification.ExtensionSpecs)
            {
                jsonFiles.AddRange(Directory.GetFiles(filePath, "*.json").ToList());
            }

            foreach (var file in jsonFiles)
            {
                JToken parsedJson;
                try
                {
                    var sr = new StreamReader(file);
                    parsedJson = JToken.Parse(sr.ReadToEnd());
                }
                catch (JsonReaderException e)
                {
                    Console.WriteLine("Specification found in {0} was not formatted correctly.. Skipping! {1}", file, Environment.NewLine);
                    continue;
                }

                var condSanitizers = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.CondSinks);
                var sources = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.Sources);
                var sqlSantizers = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.SqlSanitizer);
                var sqlSinks = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.SqlSinks);
                var xssSanitizers = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.XssSanitizer);
                var xssSinks = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.XssSinks);
                var storedProviders = parsedJson.SelectToken(Keys.PHPDefinitionJSONKeys.FunctionSpecificationArrays.StoredVulnProviders);

                if (condSanitizers != null)
                {
                    foreach (JToken spec in condSanitizers)
                    {
                        CondSanitizers.Add(new CondSanitizer(spec));
                    }
                }

                if (sources != null)
                {
                    foreach (JToken spec in sources)
                    {
                        Sources.Add(new Source(spec));
                    }
                }

                if (sqlSantizers != null)
                {
                    foreach (JToken spec in sqlSantizers)
                    {
                        SQLSanitizers.Add(new SQLSanitizer(spec));
                    }
                }

                if (sqlSinks != null)
                {
                    foreach (JToken spec in sqlSinks)
                    {
                        SQLSinks.Add(new SQLSink(spec));
                    }
                }

                if (xssSanitizers != null)
                {
                    foreach (JToken spec in xssSanitizers)
                    {
                        XSSSanitizers.Add(new XSSSanitizer(spec));
                    }
                }

                if (xssSinks != null)
                {
                    foreach (JToken spec in xssSinks)
                    {
                        XSSSinks.Add(new XSSSink(spec));
                    }
                }

                if (storedProviders != null)
                {
                    foreach (JToken spec in storedProviders)
                    {
                        StoredProviders.Add(new Function(spec));
                    }
                }
            }

            Console.WriteLine("Loaded {0} Conditional sanitizers", CondSanitizers.Count());
            Console.WriteLine("Loaded {0} sources", Sources.Count());
            Console.WriteLine("Loaded {0} SQL Sanitizers", SQLSanitizers.Count());
            Console.WriteLine("Loaded {0} SQL sinks", SQLSinks.Count());
            Console.WriteLine("Loaded {0} XSS Sanitizers", XSSSanitizers.Count());
            Console.WriteLine("Loaded {0} XSS Sinks", XSSSinks.Count());
            Console.WriteLine("Loaded {0} Stored Vulnerability Providers", StoredProviders.Count());
        }

        /// <summary>
        /// Helper function to load ressources and return them as a JToken.
        /// Used to load files with the JSON specification.
        /// </summary>
        /// <returns>The JToken (Which is a JSON array)</returns>
        /// <param name="id">The ressource identifier</param>
        private JToken LoadSpecification(string id)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(id);
            var streamReader = new StreamReader(stream);
            JToken specificationArray = JArray.Parse(streamReader.ReadToEnd());
            return specificationArray;
        }

        /// <summary>
        /// Function to find Source by name.
        /// </summary>
        /// <param name="name">The name of the source to find</param>
        /// <returns>The found source or null if none was found</returns>
        public Source FindSourceByName(string name)
        {
            return Sources.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Function to find SQLSanitizer by it's name, including any known alias.
        /// </summary>
        /// <param name="name">The name of the SQLsanitizer to find</param>
        /// <returns>The SQL sanitizer or null if none was found</returns>
        public SQLSanitizer FindSQLSanitizerByName(string name)
        {
            return SQLSanitizers.FirstOrDefault(x => x.Name == name) ??
                   SQLSanitizers.FirstOrDefault(x => x.Aliases.Any(y => y == name));
        }

        /// <summary>
        /// Function to find an XSS sanitizer function from the name.
        /// </summary>
        /// <returns>The found XSS sanitizer or if none null.</returns>
        /// <param name="name">The name of the expected function</param>
        public XSSSanitizer FindXSSSanitizerByName(string name)
        {
            return XSSSanitizers.FirstOrDefault(x => x.Name == name) ??
                   XSSSanitizers.FirstOrDefault(x => x.Aliases.Any(y => y == name));
        }

        /// <summary>
        /// Function to find an XSS sanitizer function from the name.
        /// </summary>
        /// <returns>The found XSS sanitizer or if none null.</returns>
        /// <param name="name">The name of the expected function</param>
        public CondSanitizer FindCondSanitizerByName(string name)
        {
            return CondSanitizers.FirstOrDefault(x => x.Name == name) ??
                   CondSanitizers.FirstOrDefault(x => x.Aliases.Any(y => y == name));
        }

        /// <summary>
        /// Function to find SQLSink by name
        /// </summary>
        /// <param name="name">The name of the SQL sink to find</param>
        /// <returns>The found SQLSink or the null if not found</returns>
        public SQLSink FindSQLSinkByName(string name)
        {
            return SQLSinks.FirstOrDefault(x => x.Name == name) ??
                   SQLSinks.FirstOrDefault(x => x.Aliases.Any(y => y == name));
        }

        /// <summary>
        /// Function to find XSSSink by name
        /// </summary>
        /// <param name="name">The name of the XSS sink to find</param>
        /// <returns>The found XSS sink or null if not found</returns>
        public XSSSink FindXSSSinkByName(string name)
        {
            return XSSSinks.FirstOrDefault(x => x.Name == name) ??
                   XSSSinks.FirstOrDefault(x => x.Aliases.Any(y => y == name));
        }

        /// <summary>
        /// Finds a custom function (a user defined function found in the parsed PHP code) by it's name
        /// </summary>
        /// <returns>The found custom function or null</returns>
        /// <param name="name">The name of the function to find.</param>
        public Function FindCustomFunctionByName(string name)
        {
            return CustomFunctions.FirstOrDefault(x => x.Name == name) ??
                   CustomFunctions.FirstOrDefault(x => x.Aliases.Any(y=>y == name));
        }

        public List<Function> FindStoredProviderMethods(string name)
        {
            var tmp = (from provider in StoredProviders
                    where provider.Name == name || provider.Aliases.Any(y => y == name)
                    select provider).ToList();
            return tmp;
        }

        /// <summary>
        /// Finds all functions by name and returns a list of them
        /// </summary>
        /// <returns>Returns a list of functions with name or alias as specified</returns>
        /// <param name="name">The name of the function to find</param>
        public List<Function> LookupFunction(string name)
        {
            List<Function> functionList = new List<Function>();
            List<Function> funcs;
            funcs = this.XSSSinks.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name)).ToList<Function>();
            functionList.AddRange(funcs);

            funcs = this.XSSSanitizers.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name)).ToList<Function>();
            functionList.AddRange(funcs);

            funcs = this.SQLSinks.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name)).ToList<Function>();
            functionList.AddRange(funcs);

            funcs = this.SQLSanitizers.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name)).ToList<Function>();
            functionList.AddRange(funcs);

            funcs = this.CustomFunctions.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name));
            functionList.AddRange(funcs);

            funcs = this.CondSanitizers.FindAll(x => x.Name == name || x.Aliases.Any(y => y == name)).ToList<Function>();
            functionList.AddRange(funcs);

            return functionList;
        }
    }
}