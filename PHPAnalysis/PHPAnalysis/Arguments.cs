using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using PHPAnalysis.Annotations;

namespace PHPAnalysis
{
    public sealed class Arguments
    {
        private const string TargetHelpText = "Directory or file to be analyzed.";
        private const string ScanAllSubroutinesHelpText = 
            "If set, analysis will scan all subroutines (methods + functions), " + 
            "no matter if they are called or not.";

        private const string SummaryHelpText = "If set, analysis will generate summaries of " +
            "subroutines and use those when seing a call to a subroutine. (Warning: This will " + 
            "decrease analysis precision)";

        private const string ConfigLocHelpText = "If set, the analysis will read the given config file " +
            ", instead of defaulting to the one in the running folder. Use this if you encounter " +
            "problems with finding the Config.yml file. Usage: Full path and filename to Config.yml file";

        [Option('t', "target", Required = true, HelpText = TargetHelpText)]
        public string Target { get; set; }

        [Option('a', "all", DefaultValue = false, HelpText = ScanAllSubroutinesHelpText)]
        public bool ScanAllSubroutines { get; set; }

        [Option('s', "summaries", DefaultValue = false, HelpText = SummaryHelpText)]
        public bool UseFunctionSummaries { get; set; }

        [Option('c', "configlocation", HelpText = ConfigLocHelpText)]
        public string ConfigLocation { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption, UsedImplicitly]
        public string GetUsage()
        {
            var helpText = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            helpText.Heading = new HeadingInfo("Eir", "0.0.1");
            helpText.Copyright = new CopyrightInfo(" ", 2015);
            helpText.AdditionalNewLineAfterOption = true;
            helpText.AddPreOptionsLine("---------------------");
            helpText.AddPreOptionsLine("Usage: PhpAnalyzer -t [Directory/File] [Options]");
            helpText.AddPreOptionsLine("     : PhpAnalyzer [Options] -t [Directory/File]");
            return helpText;
        }
    }
}
