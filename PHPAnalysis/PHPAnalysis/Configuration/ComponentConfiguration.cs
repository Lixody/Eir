using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Annotations;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Configuration
{
    public sealed class ComponentConfiguration
    {
        public string ComponentPath { get; private set; }
        public bool IncludeComponents { get; private set; }

        public ComponentConfiguration(string componentPath, bool includeComponents)
        {
            Preconditions.NotNull(componentPath, "componentPath");

            this.ComponentPath = componentPath;
            this.IncludeComponents = includeComponents;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Path: " + this.ComponentPath);
            return sb.ToString();
        }
    }

    [UsedImplicitly]
    internal sealed class ComponentConfigurationMutable
    {
        public string ComponentFolder { get; set; }
        public bool IncludeComponents { get; set; }
    }
}
