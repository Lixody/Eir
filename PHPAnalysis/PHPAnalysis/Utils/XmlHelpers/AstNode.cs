using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class AstNode
    {
        public static int GetStartLine(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Prefix == AstConstants.Node, "Expected node but received a " + node.Name);

            return GetLine(node, AstConstants.Attributes.StartLine);
        }

        public static int GetEndLine(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Prefix == AstConstants.Node, "Expected node but received a " + node.Name);

            return GetLine(node, AstConstants.Attributes.EndLine);
        }

        private static int GetLine(XmlNode node, string lineType)
        {
            var line = node.GetSubNode(AstConstants.Attribute + ":" + lineType);
            return Convert.ToInt32(line.InnerText);
        }
    }
}
