using System.Diagnostics;
using System.Xml;
using PHPAnalysis.Annotations;
using PHPAnalysis.Utils;
using System.Text;
using System;

namespace PHPAnalysis.Parsing
{
    public sealed class FileParser
    {
        private string _parserPath;
        public string ParserPath
        {
            get
            {
                return _parserPath.StartsWith("\"") ? _parserPath
                                                    : "\"" + _parserPath + "\"";
            }
            set { this._parserPath = value; }
        }

        public FileParser(string parserPath)
        {
            Preconditions.NotNull(parserPath, "parserPath");

            this.ParserPath = parserPath;
        }

        public XmlDocument ParsePHPFile(string pathToFile)
        {
            Preconditions.NotNull(pathToFile, "pathToFile");

            var xmlDocument = new XmlDocument();

            var process = CreateParseProcess(pathToFile);
            process.Start();

			string tmp;
			var finalOutput = new StringBuilder ();
			while ((tmp = process.StandardOutput.ReadLine ()) != null)
			{
				tmp = XmlHelper.ReplaceIllegalXmlCharacters(tmp);
				finalOutput.AppendLine (tmp);
			}
			xmlDocument.LoadXml(finalOutput.ToString());
            return xmlDocument;
        }

        private Process CreateParseProcess([NotNull] string fileToParse)
        {
            string arguments = this.ParserPath + " parse ";
            
            fileToParse = fileToParse.StartsWith("\"") ? fileToParse
                                                       : "\"" + fileToParse + "\"";
            arguments += fileToParse;

            var processStartInfo = new ProcessStartInfo() {
                                                              FileName = "php",
                                                              Arguments = arguments,
                                                              UseShellExecute = false,
                                                              RedirectStandardOutput = true,
                                                              CreateNoWindow = true
                                                          };
            var process = new Process() {
                                            StartInfo = processStartInfo
                                        };
            return process;
        }
    }
}