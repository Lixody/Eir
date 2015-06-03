using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class AstNodeInfo
    {
        public static XmlNode GetVarNameXmlNode(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            var varNode = node;

            while (varNode.HasChildNodes)
            {
                XmlNode tempNode;
                if (varNode.TryGetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var, out tempNode))
                {
                    varNode = tempNode;
                    if (tempNode.TryGetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Expr_Variable, out tempNode))
                    {
                        if (tempNode.TryGetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name, out tempNode))
                        {
                            return tempNode;
                        }
                    }
                }
                varNode = varNode.FirstChild;
            }

            throw new ArgumentException("Unknown XmlNode structure - Unable to find variable name - Implementation needed");
        }

        public static XmlNode GetValueFromXmlNode(XmlNode node)
        {
            

            return null;
        }
    }
}
