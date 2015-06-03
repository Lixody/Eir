using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Tests.Analysis
{
    class CFGCreator_General_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php while(true) {
                            if(true) { } 
                          }")]
        public void StandardIfWithinWhileLoop(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var cfg = cfgCreator.Graph;

            cfg.AssertInEdges(cfg.Vertices.ElementAt(3), 1, "First block in while true should have 1 in edge");
            cfg.AssertOutEdges(cfg.Vertices.ElementAt(3), 1, "First block in while true should have 1 out edge");

            cfg.AssertInEdges(cfg.Vertices.ElementAt(6), 1, "If true -> 1 in");
            cfg.AssertOutEdges(cfg.Vertices.ElementAt(6), 1, "If true -> 1 out");
        }
        [TestCase(@"<?php for(;;) {
                            if(true) { } 
                          }")]
        public void StandardIfWithinForLoop(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);
            var cfg = cfgCreator.Graph;

            cfg.AssertInEdges(cfg.Vertices.ElementAt(3), 2, "Loop Cond - 2 in edges");
            cfg.AssertOutEdges(cfg.Vertices.ElementAt(3), 2, "Loop Cond - 2 out edges");

            cfg.AssertInEdges(cfg.Vertices.ElementAt(8), 1, "If true -> 1 in");
            cfg.AssertOutEdges(cfg.Vertices.ElementAt(8), 1, "If true -> 1 out");
        }

        [TestCase(@"<?php ", 2)]
        [TestCase(@"<?php echo 'asdf';", 3)]
        [TestCase(@"<?php $myVar = $_GET['john']; echo $myVar; echo $_REQUEST['gdaw'];", 5)]
        [TestCase(@"<?php $_GUL['1']; $var; $var::$adsf; $asdf->asdf;       // 4
                          1; 2.1; 'adsf';                                   // 3
                          __LINE__; __FILE__; __DIR__; __FUNCTION__;        // 4
                          __CLASS__; __TRAIT__; __METHOD__;__NAMESPACE__;   // 4
                          2 == 3; 3 <= 1; 3 ** 2;                           // 3
                        ", 4 + 3 + 4 + 4 + 3 + 2)]
        public void NoBranchingStatements(string phpCode, int expectedBlocks)
        {
            var cfg = ParseAndBuildCFG(phpCode).Graph;

            //cfg.VisualizeGraph("graph", Config.GraphSettings);

            int expectedEdges = expectedBlocks - 1; // No branches, so always 1 less.

            Assert.AreEqual(expectedBlocks, cfg.VertexCount, "Didn't have expected number of vertices");
            Assert.AreEqual(expectedEdges, cfg.EdgeCount, "Didn't have expected number of edges");
        }

        [TestCase(@"<?php while(true) { break; return; }")]
        public void UnreachableReturn(string code)
        {
            var cfg = ParseAndBuildCFG(code).Graph;

            var returnBlock = cfg.Vertices.Single(v => v.AstEntryNode != null && v.AstEntryNode.LocalName == AstConstants.Nodes.Stmt_Return);
            Assert.IsEmpty(cfg.InEdges(returnBlock), "Return should not have any incomming edges.");

            //cfg.VisualizeGraph("graph", this.Config.GraphSettings);
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
