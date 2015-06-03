using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class CFGCreator_Continue_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php while (true) { continue; }")]
        [TestCase(@"<?php  while (($v = someFunc()) == true) { continue; }")]
        public void CFGCreation_StandardWhile(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Standard loop should have " + expectedNodes + " nodes.");

            // Check structure
            CFGBlock conditionBlock = graph.Vertices.ElementAt(2);
            graph.AssertInEdges(conditionBlock, 2, "ConditionNode - in edges");
            graph.AssertOutEdges(conditionBlock, 2, "Condition node - out edges");

            var loopBodyBlock = graph.Vertices.ElementAt(3);
            var bodyOutEdge = graph.OutEdges(loopBodyBlock).Single();

            Assert.AreEqual(conditionBlock, bodyOutEdge.Target, "Loop body out should point to condition");
            Assert.IsTrue(loopBodyBlock.BreaksOutOfScope, "Loop body should break out of loop.");

            graph.AssertInEdges(graph.Vertices.ElementAt(4), 1, "Loop end - in edges.");
        }
        [TestCase(@"<?php for ($i = 0; $i < 10; $i++) { continue; }")]
        [TestCase(@"<?php for($i = 0, $j = 1;;) { continue; }")]
        public void CFGCreation_StandardFor(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 5;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Should have " + expectedNodes + " nodes.");

            // Check structure
            var loopInitBlock = graph.Vertices.ElementAt(2);
            var conditionBlock = graph.Vertices.ElementAt(3);
            var loopUpdateBlock = graph.Vertices.ElementAt(4);
            var loopBodyStart = graph.Vertices.ElementAt(5);

            var loopBodyStartOutEdge = graph.OutEdges(loopBodyStart).Single();
            Assert.AreEqual(loopUpdateBlock, loopBodyStartOutEdge.Target, "Loop body out should point to loop update block");

            Assert.IsTrue(loopBodyStart.BreaksOutOfScope, "Loop body should break out of loop.");
        }

        [TestCase(@"<?php while (true) if(true) continue;")]
        public void CFGCreation_ContinueWithinIf_InWhile(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 3 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Graph didn't have expected number of vertices.");

            // Check structure
            var loopConditionBlock = graph.Vertices.ElementAt(2);
            var loopExitBlock = graph.Vertices.ElementAt(4);
            var ifTrueBlock = graph.Vertices.ElementAt(6);
            var ifFalseBlock = graph.Vertices.ElementAt(7);

            graph.AssertInEdges(loopConditionBlock, 3, "Loop ConditionNode - in edges");
            graph.AssertOutEdges(loopConditionBlock, 2, "Loop Condition - out edges");

            var ifTrueOutEdge = graph.OutEdges(ifTrueBlock).Single();
            Assert.AreEqual(loopConditionBlock, ifTrueOutEdge.Target, "if body out edge should point to loop condition");

            graph.AssertInEdges(loopExitBlock, 1, "Loop end - in edges.");

            Assert.True(ifTrueBlock.BreaksOutOfScope, "If-true should break out of loop.");
            Assert.False(ifFalseBlock.BreaksOutOfScope, "If-false should not break out of loop.");
        }
        [TestCase(@"<?php for (;;) if(true) continue;")]
        public void CFGCreation_ContinueWithinIf_InFor(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            const int expectedNodes = 2 + 5 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Graph didn't have expected number of vertices.");

            // Check structure
            var loopConditionBlock = graph.Vertices.ElementAt(3);
            var loopUpdateBlock = graph.Vertices.ElementAt(4);
            var ifTrueBlock = graph.Vertices.ElementAt(8);

            graph.AssertInEdges(loopConditionBlock, 2, "Loop ConditionNode - in edges");
            graph.AssertOutEdges(loopConditionBlock, 2, "Loop Condition - out edges");

            var ifTrueBlockOutEdge = graph.OutEdges(ifTrueBlock).Single();
            Assert.AreEqual(loopUpdateBlock, ifTrueBlockOutEdge.Target, "Continue block should point to loop update");

            Assert.True(ifTrueBlock.BreaksOutOfScope, "If-true should break out of loop.");
            Assert.False(graph.Vertices.ElementAt(9).BreaksOutOfScope, "If-false should not break out of loop.");
        }

        [TestCase(@"<?php do { continue; } while (true);")]
        public void CFGCreation_StandardDoWhile(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Number of nodes");

            // Check structure
            var loopBodyBlock = graph.Vertices.ElementAt(2);
            var loopConditionBlock = graph.Vertices.ElementAt(3);
            var loopExitBlock = graph.Vertices.ElementAt(4);

            Assert.IsTrue(loopBodyBlock.BreaksOutOfScope, "Loop body should break out of loop.");

            var loopBodyOutEdge = graph.OutEdges(loopBodyBlock).Single();
            Assert.AreEqual(loopConditionBlock, loopBodyOutEdge.Target, "Continue stmt should go to condition block");

            graph.AssertInEdges(loopConditionBlock, 1, "ConditionNode - in edges");
            graph.AssertOutEdges(loopConditionBlock, 2, "Condition - out edges");

            graph.AssertInEdges(loopExitBlock, 1, "End - in edges");
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
