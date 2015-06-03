using System.Collections.Generic;
using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class GlobalNode
    {
        public static IEnumerable<XmlNode> GetVariables(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Stmt_Global,
                "Node has to be a global statement. It was: " + node.Name, "node");

            var variables = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Vars)
                                .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array)
                                .GetSubnodes(AstConstants.Node + ":" + AstConstants.Nodes.Expr_Variable);
            return variables;
        }
    }
}