using NUnit.Framework;
using PHPAnalysis.Data;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data.CFG;

namespace PHPAnalysis.Tests.TestUtils
{
    public static class GraphAssertions
    {
        public static void AssertOutEdges(this BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock block, int expectedOutEdges, string message = "")
        {
            IEnumerable<TaggedEdge<CFGBlock, EdgeTag>> edges;
            graph.TryGetOutEdges(block, out edges);

            Assert.AreEqual(expectedOutEdges, edges.Count(), message);
        }
        public static void AssertInEdges(this BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock block, int expectedInEdges, string message = "")
        {
            IEnumerable<TaggedEdge<CFGBlock, EdgeTag>> edges;
            graph.TryGetInEdges(block, out edges);

            Assert.AreEqual(expectedInEdges, edges.Count(), message);
        }
    }
}
