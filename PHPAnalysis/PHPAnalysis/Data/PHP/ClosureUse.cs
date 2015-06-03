using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.PHP
{
    public sealed class ClosureUse
    {
        public XmlNode AstNode { get; private set; }
        public string Name { get; set; }

        public bool ByReference { get; set; }

        public ClosureUse(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            this.AstNode = node;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", ByReference ? "&" : "", Name);
        }
    }
}