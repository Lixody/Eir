using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Annotations;
using PHPAnalysis.Utils;
using YamlDotNet.Serialization;

namespace PHPAnalysis.Configuration
{
    public sealed class FuncSpecConfiguration
    {
        public IList<string> PHPSpecs { get; private set; }
        public IList<string> ExtensionSpecs { get; private set; }

        public FuncSpecConfiguration(IList<string> phpSpecs, IList<string> extensionSpecs)
        {
            Preconditions.NotNull(phpSpecs, "phpSpecs");
            this.PHPSpecs = phpSpecs;

            this.ExtensionSpecs = extensionSpecs ?? new List<string>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PHP specs paths: ");
            foreach (var path in PHPSpecs)
            {
                sb.AppendLine("-" + "".PadLeft(2) + path);
            }
            sb.AppendLine("Extension paths:");
            foreach (var path in ExtensionSpecs)
            {
                sb.AppendLine("-" + "".PadLeft(2) + path);
            }
            return sb.ToString();
        }
    }

    [UsedImplicitly]
    internal sealed class FuncSpecConfigurationMutable
    {
        public List<string> PHPSpecs { get; set; }
        public List<string> ExtensionSpecs { get; set; }
    }
}
