using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Data;
using QuickGraph.Algorithms.ShortestPath;

namespace PHPAnalysis.Utils.XmlHelpers
{
    internal static class ForLoop
    {
        public static XmlNode GetInitNode(XmlNode node)
        {
            const string initNodeName = AstConstants.Subnode + ":" + AstConstants.Subnodes.Init;
            return GetSubNode(node, initNodeName);
        }

        public static XmlNode GetConditionNode(XmlNode node)
        {
            const string conditionNodeName = AstConstants.Subnode + ":" + AstConstants.Subnodes.Cond;
            return GetSubNode(node, conditionNodeName);
        }

        public static XmlNode GetLoopNode(XmlNode node)
        {
            const string initNodeName = AstConstants.Subnode + ":" + AstConstants.Subnodes.Loop;
            return GetSubNode(node, initNodeName);
        }

        private static XmlNode GetSubNode(XmlNode node, string nodeName)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.LocalName == AstConstants.Nodes.Stmt_For, "Expected for-node but got " + node.Name);

            return node.GetSubNode(nodeName);
        }
    }
}
