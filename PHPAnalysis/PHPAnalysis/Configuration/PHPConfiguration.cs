using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PHPAnalysis.Annotations;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Configuration
{
    public sealed class PHPConfiguration
    {
        public string PHPPath { get; private set; }
        public string PHPParserPath { get; private set; }
        public IList<string> PHPFileExtensions { get; private set; }

        public PHPConfiguration(string phpPath, string phpParserPath, IList<string> phpExtensions)
        {
            Preconditions.NotNull(phpPath, "phpPath");
            Preconditions.NotNull(phpParserPath, "phpParserPath");
            Preconditions.NotNull(phpExtensions, "phpExtensions");

            this.PHPParserPath = phpParserPath;
            this.PHPPath = phpPath;
            this.PHPFileExtensions = phpExtensions;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PHP Path: " + PHPPath);
            stringBuilder.AppendLine("PHP Parser: " + PHPParserPath);
            stringBuilder.AppendLine("File extensions: " + string.Join(", ", PHPFileExtensions.ToArray()));
            return stringBuilder.ToString();
        }
    }

    [UsedImplicitly]
    internal sealed class PHPConfigurationMutable
    {
        public string PHPPath { get; set; }
        public List<string> PHPExts { get; set; }
        public string PHPParsePath { get; set; }
    }
}