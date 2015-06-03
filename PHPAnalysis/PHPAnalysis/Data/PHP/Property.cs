using System.Xml;

namespace PHPAnalysis.Data.PHP
{
    public sealed class Property
    {
        public XmlNode AstNode { get; private set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public AstConstants.VisibilityModifiers VisibilityModifiers { get; set; }

        public string Name { get; set;}

        public bool HasDefault { get; set; }

        public Property(XmlNode astNode)
        {
            AstNode = astNode;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2};", 
                VisibilityModifiers,
                Name,
                HasDefault ? " = " : "");
        }
    }
}