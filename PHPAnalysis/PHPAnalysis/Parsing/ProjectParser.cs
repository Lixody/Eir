using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using PHPAnalysis.Configuration;
using PHPAnalysis.IO.Cmd;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Parsing
{
    public sealed class ProjectParser
    {
        private string ProjectPath { get; set; }
        private PHPConfiguration PHPSettings { get; set; }

        public ProjectParser(string projectPath, PHPConfiguration settings)
        {
            Preconditions.NotNull(projectPath, "projectPath");
            Preconditions.NotNull(settings, "settings");

            PHPSettings = settings;

            if (!Directory.Exists(projectPath))
            {
                string errorMsg = "Directory does not exist. (" + projectPath + ")";
                throw new DirectoryNotFoundException(errorMsg);
            }

            this.ProjectPath = projectPath;
        }

        public ParseResult ParseProjectFiles()
        {
            IEnumerable<string> files = Directory.GetFiles(ProjectPath, "*", SearchOption.AllDirectories)
                                                 .Where(file => PHPSettings.PHPFileExtensions.Contains(Path.GetExtension(file)))
                                                 .Select(file => file.Replace(@"\\", @"\"));

            var result = new ParseResult();

            var phpFileParser = new FileParser(PHPSettings.PHPParserPath);

            var progrssIndicator = ProgressIndicatorFactory.CreateProgressIndicator(files.Count());

            foreach (var file in files)
            {
                progrssIndicator.Step();
                try
                {
                    XmlDocument parseResult = phpFileParser.ParsePHPFile(file);
                    result.ParsedFiles.Add(file, parseResult);
                }
                catch (XmlException)
                {
                    result.FilesThatFailedToParse.Add(file);
                }
            }
            return result;
        }
    }
}
