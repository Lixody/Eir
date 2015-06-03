using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class CFGPrunerTests : ConfigDependentTests
    {
        
        [TestCase(@"<?php for(;;) { break; }"),
         TestCase(@"<?php for(;;) { break; } for(;;) { break; }"),
         TestCase(@"<?php for(;;) { break; for(;;) { break; }}"),
         TestCase(@"<?php for(;;) { for(;;) { break; }}"),
         TestCase(@"<?php for(;;$i++) { break; }"),
         TestCase(@"<?php for(;;$i++) { continue; }"),
         TestCase(@"<?php while(true) { break; do {} while(true); }")
        ]
        public void CFGPruning_ShouldRemoveVerticesWithNoInEdges(string phpCode)
        {
            var cfg = ParseAndBuildCFG(phpCode).Graph;
            new CFGPruner().Prune(cfg);

            //cfg.VisualizeGraph("graph-pruned");

            var reachableBlocks = cfg.ReachableBlocks(cfg.Roots().Single());

            CollectionAssert.AreEquivalent(reachableBlocks, cfg.Vertices, "Not all vertices in graph are reachable");
        }

        [Test]
        public void CFGPruning_IfSentence_ShouldRemoveThreeBlocks()
        {
            string code = @"<?php
if(isset($_GET['test']))
{
    echo $_GET['test'];
}
else 
{
    echo 'set var pls!';
}?>";
            var cfg = ParseAndBuildCFG(code).Graph;
            Assert.AreEqual(8, cfg.Vertices.Count());

            new CFGPruner().Prune(cfg);
            Assert.AreEqual(5, cfg.Vertices.Count());
        }

        [Test]
        public void CFGPruning_SwitchStatement_ShouldRemoveFourBlocks()
        {
            string code = @"<?php
switch ($_GET['animal'])
{
    case 'horse':
        echo 'wrinsk wrinsk';
        break;
    case 'dog':
        echo 'WUFF WUFF';
        break;
    
    default:
        echo 'input a fucking animal';
        break;
}
?>";
            var cfg = ParseAndBuildCFG(code).Graph;
            Assert.AreEqual(13, cfg.Vertices.Count());

            new CFGPruner().Prune(cfg);
            Assert.AreEqual(9, cfg.Vertices.Count());
        }

        [Test]
        public void CFGPruning_foreach_ShouldRemoveTwoBlocks()
        {
            var code = @"<?php
foreach ($_GET as $value) {
    echo 'Key: ' . $key . "" value: "" . $value;
}
?>";
            var cfg = ParseAndBuildCFG(code).Graph;

            //cfg.VisualizeGraph("graph", Config.GraphSettings);

            Assert.AreEqual(6, cfg.Vertices.Count());

            new CFGPruner().Prune(cfg);
            Assert.AreEqual(4, cfg.Vertices.Count());
        }

        [Test]
        public void CFGPruner_foreach_shouldRemoveSixNodes()
        {
            var phpcode = @"<?php
if(true) {
}
else {
	if(true) {
	}
	else {
	}
}
?>";
            var cfg = ParseAndBuildCFG(phpcode).Graph;
            Assert.AreEqual(10, cfg.Vertices.Count());
            
            new CFGPruner().Prune(cfg);
            Assert.AreEqual(4, cfg.Vertices.Count());
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
