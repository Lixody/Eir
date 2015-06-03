using System.Collections.Generic;
using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.PHP
{
    public sealed class Class
    {
        public XmlNode AstNode { get; set; }
        public string Name { get; set; }
        public string File { get; set; }

        public IList<Property> Properties { get; private set; }
        public IList<Function> Methods { get; private set; }

        public int StartLine { get; set; }
        public int EndLine { get; set; }

        public AstConstants.VisibilityModifiers VisibilityModifiers { get; set; }

        public bool IsFinal
        {
            get { return (VisibilityModifiers & AstConstants.VisibilityModifiers.Final) != 0; }
        }

        public Class(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            this.AstNode = node;
            this.Methods = new List<Function>();
            this.Properties = new List<Property>();
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, StartLine: {1}, EndLine: {2}", Name, StartLine, EndLine);
        }
    }
}