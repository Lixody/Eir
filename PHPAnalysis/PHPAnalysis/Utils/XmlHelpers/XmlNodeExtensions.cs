using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class XmlNodeExtensions
    {
        public static XmlNode GetSubNode(this XmlNode node, string subNodeName)
        {
            XmlNode subNode;
            if (node.TryGetSubNode(subNodeName, out subNode))
            {
                return subNode;
            }
            throw new ArgumentException("Node (" + node.Name + ") does not have a subnode named " + subNodeName);
        }

        public static IEnumerable<XmlNode> GetSubNodesByPrefix(this XmlNode node, string prefix)
        {
            Preconditions.NotNull(node, "node");
            return node.Cast<XmlNode>().Where(n => n.Prefix == prefix);
        }

        public static IEnumerable<XmlNode> GetSubNodesByPrefixes(this XmlNode node, params string[] prefixes)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.NotNull(prefixes, "prefixes");
            return node.Cast<XmlNode>().Where(n => prefixes.Contains(n.Prefix));
        }

        public static bool TryGetSubNode(this XmlNode node, string subNodeName, out XmlNode result)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.NotNull(subNodeName, "subNodeName");

            result = node.Cast<XmlNode>().FirstOrDefault(n => n.Name == subNodeName);
            return result != null;
        }

        public static IEnumerable<XmlNode> GetSubnodes(this XmlNode node, string subNodeName)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.NotNull(subNodeName, "subNodeName");

            return node.Cast<XmlNode>().Where(n => n.Name == subNodeName);
        }

        public static void IterateAllNodes(this XmlNode node, Func<XmlNode, bool> visitor)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.NotNull(visitor, "visitor");

            IterateAllNodesImpl(node, visitor);
        }

        private static void IterateAllNodesImpl(XmlNode node, Func<XmlNode, bool> visitor)
        {
            visitor(node);
            
            foreach (XmlNode childNode in node.ChildNodes)
            {
                IterateAllNodesImpl(childNode, visitor);
            }
        }

        public static XmlNode GetVarNameXmlNode(this XmlNode node)
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

        public static bool IsAssignmentNode(this XmlNode node)
        {
            if (node.Prefix != AstConstants.Node)
            {
                return false;
            }

            var assignmentNodes = new [] {
                                             AstConstants.Nodes.Expr_Assign,
                                             AstConstants.Nodes.Expr_AssignOp_BitwiseAnd,
                                             AstConstants.Nodes.Expr_AssignOp_BitwiseOr,
                                             AstConstants.Nodes.Expr_AssignOp_BitwiseXor,
                                             AstConstants.Nodes.Expr_AssignOp_Concat,
                                             AstConstants.Nodes.Expr_AssignOp_Div,
                                             AstConstants.Nodes.Expr_AssignOp_Minus,
                                             AstConstants.Nodes.Expr_AssignOp_Mod,
                                             AstConstants.Nodes.Expr_AssignOp_Mul,
                                             AstConstants.Nodes.Expr_AssignOp_Plus,
                                             AstConstants.Nodes.Expr_AssignOp_Pow,
                                             AstConstants.Nodes.Expr_AssignOp_ShiftLeft,
                                             AstConstants.Nodes.Expr_AssignOp_ShiftRight,
                                             AstConstants.Nodes.Expr_AssignRef
                                         };
            return assignmentNodes.Contains(node.LocalName);
        }
    }
}
