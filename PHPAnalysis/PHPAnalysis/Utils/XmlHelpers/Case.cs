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
    public sealed class Case
    {
        public static bool IsDefaultCase(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.LocalName == AstConstants.Nodes.Stmt_Case,
                                 "Expected a case node, but received a " + node.Name);

            XDocument xNode = XDocument.Parse(node.OuterXml);
            XNamespace nsa = AstConstants.Namespaces.SubNode;

            var defaultLine = xNode.Descendants(nsa + AstConstants.Subnodes.Cond)
                              .First().Value;

            return defaultLine == "";
        }
    }
}
