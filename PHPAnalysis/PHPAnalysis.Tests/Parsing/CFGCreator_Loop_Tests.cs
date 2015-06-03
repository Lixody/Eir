using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using QuickGraph;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class CFGCreator_Loop_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php while (true) { }")]
        [TestCase(@"<?php while (true) ;")]
        [TestCase(@"<?php while (list(, $value) = each($arr)) { }")]
        [TestCase(@"<?php while (list($key, $value) = each($arr)) { }")]
        [TestCase(@"<?php while(false): 
                          endwhile;")]
        [TestCase(@"<?php foreach($_GET as $x) { }")]
        [TestCase(@"<?php foreach($_GET as $key => $val) { }")]
        [TestCase(@"<?php foreach($_GET as &$x) ;")]
        [TestCase(@"<?php foreach($_GET as &$x): 
                          endforeach;")]
        public void CFGCreation_StandardWhileAndForeachLoops(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var graph = cfgCreator.Graph;

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Standard loop should have " + expectedNodes + " nodes.");

            // Expected placement of vertices:
            //     0
            //     |
            //     1
            //   // \
            //   2   3
            //       |
            //       4

            // Check structure
            graph.AssertInEdges(graph.Vertices.First(), 0, "Entry node - in edges");
            graph.AssertOutEdges(graph.Vertices.First(), 1, "Entry node - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(2), 2, "Cond node - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(2), 2, "Condition node - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(3), 1, "Loop body - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 1, "Loop body - out edges");
        }

        [TestCase(@"<?php for ($i = 0; $i < 10; $i++) { }"), 
        TestCase(@"<?php for (; $i < 10; $i++) { }"), 
        TestCase(@"<?php for (; $i < 10; $i++) ;"),
        TestCase(@"<?php for($i = 0, $j = 0; $i++ >= $j++; $i++, $j++) { }"), 
        ]
        public void CFGCreation_StandardEmptyForLoop(string phpCode)
        {
            //  ↓
            // init     loop
            //    \     /  ^
            //  +--cond    |
            //  |    ↓     |
            //  |  body ---+
            //  ↓
            //  +->end

            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedBlocks = 2 + 5;
            Assert.AreEqual(expectedBlocks, graph.VertexCount, "Graph doesn't have expecter number of nodes.");

            var condBlock = graph.Vertices.ElementAt(3);
            Assert.AreEqual(AstConstants.Nodes.Stmt_For, condBlock.AstEntryNode.LocalName);
            graph.AssertInEdges(condBlock, 2, "Cond block - in edges");
            graph.AssertOutEdges(condBlock, 2, "Cond block - out edges");

            var loopBlock = graph.Vertices.ElementAt(4);
            Assert.AreEqual(AstConstants.Subnodes.Loop, loopBlock.AstEntryNode.LocalName);
            graph.AssertInEdges(loopBlock, 1, "loop block - in edges");
            graph.AssertOutEdges(loopBlock, 1, "loop block - out edges");
        }

        [TestCase(@"<?php do { } while (true);")]
        [TestCase(@"<?php do { } while ($x = someFunction());")]
        public void CFGCreation_StandardDoWhileLoop(string phpCode)
        {
            var graph = ParseAndBuildCFG(phpCode).Graph;

            const int expectedNodes = 2 + 3;
            Assert.AreEqual(expectedNodes, graph.VertexCount, "Standard loop should have " + expectedNodes + " nodes.");


            //        ↓
            //  +-> entry (body start)  
            //  |     ↓
            //  +---cond
            //        ↓
            //       end (loop end)


            // Check structure
            graph.AssertInEdges(graph.Vertices.ElementAt(2), 2, "Loop body start - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(2), 1, "Loop body start - out");

            graph.AssertInEdges(graph.Vertices.ElementAt(3), 1, "Condition - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(3), 2, "Condtion - out edges");

            graph.AssertInEdges(graph.Vertices.ElementAt(4), 1, "End - in edges");
            graph.AssertOutEdges(graph.Vertices.ElementAt(4), 1, "End - out edges");
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
