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
    public static class Conditional
    {
        private static readonly string[] NodesWithCondSubNode =
                                {
                                    AstConstants.Nodes.Stmt_Case,
                                    AstConstants.Nodes.Stmt_Do,
                                    AstConstants.Nodes.Stmt_ElseIf,
                                    AstConstants.Nodes.Stmt_For,
                                    AstConstants.Nodes.Stmt_If,
                                    AstConstants.Nodes.Stmt_Switch,
                                    AstConstants.Nodes.Stmt_While,
                                };

        public static bool HasConditionNode(XmlNode node)
        {
            return NodesWithCondSubNode.Contains(node.LocalName);
        }

        public static XmlNode GetCondNode(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(NodesWithCondSubNode.Contains(node.LocalName), 
                "Expected node representing conditional statement. Received a " + node.Name);

            return node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Cond);
        }
    }
}
