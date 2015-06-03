using System;
using PHPAnalysis.Utils;
using System.Text;
using PHPAnalysis.Annotations;

namespace PHPAnalysis
{
    public sealed class GraphConfiguration
    {
        public string GraphvizPath { get; private set; }
        public string GraphvizArguments { get; private set; }

        public GraphConfiguration(string graphvizPath, string graphvizArguments)
        {
            Preconditions.NotNull(graphvizPath, "graphvizPath");
            Preconditions.NotNull(graphvizArguments, "graphvizArguments");

            this.GraphvizPath = graphvizPath;
            this.GraphvizArguments = graphvizArguments;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("[Graph configuration:");
            stringBuilder.AppendLine("    Graphviz path: " + GraphvizPath);
            stringBuilder.AppendLine("    Graphviz arguments: " + GraphvizArguments);
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }
    }

    [UsedImplicitly]
    internal sealed class GraphConfigurationMutable
    {
        public string GraphvizPath { get; set; }
        public string GraphvizArguments { get; set; }
    }
}