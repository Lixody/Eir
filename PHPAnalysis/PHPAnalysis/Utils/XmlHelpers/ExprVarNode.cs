using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class ExprVarNode
    {
        /// <summary>
        /// Tries to get the variables name. This will only work if the variable name is static. 
        /// <example>$var : will be 'var'</example>
        /// <example>$$var : will fail.</example>
        /// </summary>
        public static bool TryGetVariableName(XmlNode node, out string varName)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Expr_Variable,
                "Node has to be an Expression Variable. It was: " + node.Name, "node");

            var nameNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);

            if (nameNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String, out nameNode))
            {
                varName = nameNode.InnerText;
                return true;
            }
            varName = null;
            return false;
        }
    }
}