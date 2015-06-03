using System.Linq;
using System.Text;
using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.PHP
{
    public sealed class Closure
    {
        public XmlNode AstNode { get; private set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string File { get; set; }
        public Parameter[] Parameters { get; set; }
        public ClosureUse[] UseParameters { get; set; }

        public Closure(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            this.AstNode = node;
            Parameters = new Parameter[0];
            UseParameters = new ClosureUse[0];
        }

        public override string ToString()
        {
            var builder = new StringBuilder("function");
            builder.Append("(")
                   .Append(string.Join<Parameter>(", ", Parameters))
                   .Append(") ");
            if (UseParameters.Any())
            {
                builder.Append("use (")
                       .Append(string.Join<ClosureUse>(", ", UseParameters))
                       .Append(") ");
            }

            builder.Append(StartLine)
                   .Append(" ")
                   .Append(EndLine);

            return builder.ToString();
        }
    }
}
