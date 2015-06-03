using System.Collections.Generic;
using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.PHP
{
    public sealed class Interface
    {
        public XmlNode AstNode { get; set; }

        public IList<Function> Methods { get; private set; }

        public string Name { get; set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public Interface(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            this.AstNode = node;
            this.Methods = new List<Function>();
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, StartLine: {1}, EndLine: {2}", Name, StartLine, EndLine);
        }
    }
}