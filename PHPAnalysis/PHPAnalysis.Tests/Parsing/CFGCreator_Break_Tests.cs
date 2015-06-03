using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class CFGCreator_Break_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php while (true) { break; }")]
        [TestCase(@"<?php while (($v = someFunc()) == true) { break; }")]
        public void CFGCreation_StandardWhileWithBreak(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            //graph.VisualizeGraph("graph", Config.GraphSettings);

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Standard loop with break should have " + expectedNodes + " nodes.");

            // Check structure
            graph.AssertInEdges(graph.Vertices.ElementAt(2), 1, "ConditionNode - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(2), 2, "The 2nd node (condition node) did not have expected number of out edges");

            var loopBodyBlock = graph.Vertices.ElementAt(3);
            graph.AssertInEdges(loopBodyBlock, 1, "3nd node (loop body) - in edges");
            graph.AssertOutEdges(loopBodyBlock, 1, "The 3nd node did not have expected number of out edges");
            Assert.IsTrue(loopBodyBlock.BreaksOutOfScope, "Loop body should break out of loop.");
            Assert.IsNull(loopBodyBlock.AstEntryNode, "Loop body should be empty.");

            graph.AssertInEdges(graph.Vertices.ElementAt(4), 2, "Loop end - in edges.");
            graph.AssertOutEdges(graph.Vertices.ElementAt(4), 1, "Loop end -  out edges");
        }
        [TestCase(@"<?php for ($i = 0; $i < 10; $i++) { break; }")]
        [TestCase(@"<?php for($i = 0, $j = 1;;) { break; }")]
        public void CFGCreation_StandardForWithBreak(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            const int expectedNodes = 2 + 5;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Should have " + expectedNodes + " nodes.");

            // Check structure
            graph.AssertInEdges(graph.Vertices.ElementAt(3), 2, "Condition node - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 2, "Condition node - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(4), 0, "Loop node - in");
            graph.AssertOutEdges(graph.Vertices.ElementAt(4), 1, "Loop node - out");

            const int loopbodyIndex = 5;
            var loopBody = graph.Vertices.ElementAt(loopbodyIndex);
            graph.AssertInEdges(loopBody, 1, "Loop body - in edges");
            graph.AssertOutEdges(loopBody, 1, "Loop body - out edges");
            Assert.IsTrue(loopBody.BreaksOutOfScope, "Loop body should break out of loop.");

            graph.AssertInEdges(graph.Vertices.ElementAt(6), 2, "Loop end - in edges.");
            graph.AssertOutEdges(graph.Vertices.ElementAt(6), 1, "Loop end -  out edges");
        }

        [TestCase(@"<?php while (true) if(true) break;")]
        public void CFGCreation_BreakWithinIf_InWhile(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            const int expectedNodes = 2 + 3 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Graph didn't have expected number of vertices.");

            // Check structure
            graph.AssertInEdges(graph.Vertices.ElementAt(2), 2, "Loop ConditionNode - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(2), 2, "Loop Condition - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(3), 1, "3nd node (loop start body) - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 1, "The 3nd node did not have expected number of out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(4), 2, "Loop end - in edges.");
            graph.AssertOutEdges(graph.Vertices.ElementAt(4), 1, "Loop end -  out edges");

            Assert.True(graph.Vertices.ElementAt(6).BreaksOutOfScope, "If-true should break out of loop.");
            Assert.False(graph.Vertices.ElementAt(7).BreaksOutOfScope, "If-false should not break out of loop.");
        }
        [TestCase(@"<?php for (;;) if(true) break;")]
        public void CFGCreation_BreakWithinIf_InFor(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            const int expectedNodes = 2 + 5 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Graph didn't have expected number of vertices.");

            // Check structure
            graph.AssertInEdges(graph.Vertices.ElementAt(3), 2, "Loop ConditionNode - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 2, "Loop Condition - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(5), 1, "3nd node (loop start body) - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(5), 1, "The 3nd node did not have expected number of out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(6), 2, "Loop end - in edges.");
            graph.AssertOutEdges(graph.Vertices.ElementAt(6), 1, "Loop end -  out edges");

            Assert.True(graph.Vertices.ElementAt(8).BreaksOutOfScope, "If-true should break out of loop.");
            Assert.False(graph.Vertices.ElementAt(9).BreaksOutOfScope, "If-false should not break out of loop.");
        }

        [TestCase(@"<?php do { break; } while (true);")]
        public void CFGCreation_StandardDoWhileWithBreak(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Number of nodes");

            // Check structure
            Assert.IsTrue(graph.Vertices.ElementAt(2).BreaksOutOfScope, "Loop body should break out of loop.");

            graph.AssertInEdges(graph.Vertices.ElementAt(3), 0, "ConditionNode - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 2, "Condition - out edges");

            const int loopEndIndex = 4;
            CFGBlock loopExitBlock = graph.Vertices.ElementAt(loopEndIndex);
            graph.AssertInEdges(loopExitBlock, 2, "End - in edges");
            graph.AssertOutEdges(loopExitBlock, 1, "End - out edges");
        }

        [TestCase(@"<?php for(;;) { echo """"; break; for(;;) { break; }}"),
         TestCase(@"<?php for(;;) { break; echo """"; echo """"; echo """"; }"),
         TestCase(@"<?php while(true) { break; while(true) {} }"),
         TestCase(@"<?php while(true) { break; do {} while(true); }"),
         TestCase(@"<?php while(true) { break; foreach ($var as $k) {}}"),
         TestCase(@"<?php while(true) { break; if (true) { } }"),
         TestCase(@"<?php while(true) { break; switch ($var) { } }")]
        public void CFGCreation_BreakShouldHaveOnlyOneOutEdge(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            foreach (var block in graph.Vertices.Where(v => v.BreaksOutOfScope))
            {
                Assert.AreEqual(1, graph.OutDegree(block), "Breaking block (" + block + ") does not have expected number of out edges");
            }
        }

        [TestCase(@"<?php if ($count == 0) { return false; break; };")] //IRL code example (mystat)! - No error because of return!
        public void CFGCreation_UnreachableBreak_ShouldNotFail(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
