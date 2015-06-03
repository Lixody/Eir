using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    internal static class BreakContinue
    {
        public static readonly int DefaultScopeNumber = 1;

        /// <summary>
        /// Tries to get the number associated with the break/continue statement.
        /// e.g. Continue 3; or Break 10;
        /// </summary>
        public static bool TryGetScopeNumber(XmlNode node, out int scopeNumber)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.LocalName == AstConstants.Nodes.Stmt_Break ||
                                 node.LocalName == AstConstants.Nodes.Stmt_Continue, 
                                 "Expected a break/continue node, but received a " + node.Name);


            const int minimumValue = 1;
            scopeNumber = minimumValue;

            XDocument xNode = XDocument.Parse(node.OuterXml);
            XNamespace nsa = AstConstants.Namespaces.SubNode;

            var numDescendants = xNode.Descendants(nsa + AstConstants.Subnodes.Num)
                                      .SingleOrDefault();

            if (numDescendants == null)
            {
                return false;
            }
            var number = numDescendants.Descendants(nsa + AstConstants.Subnodes.Value)
                                       .Select(n => Convert.ToInt32(n.Value))
                                       .SingleOrDefault();

            if (number < minimumValue)
            {
                return false;
            }

            scopeNumber = number;
            return true;
        }
    }
}
