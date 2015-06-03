using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Tests.Analysis.CFG
{
    [TestFixture]
    public class CFGTraverserTests
    {
        [Test]
        public void CFGTraverser_ShouldTraverseAllBlocksAtLeastOnce()
        {
            var analysisMock = new Mock<ICFGAnalysis>();
            analysisMock.Setup(a => a.Analyze(null)).Returns(false);
            
            var traverser = new CFGTraverser(new ForwardTraversal(), analysisMock.Object, new QueueWorklist());
        }
    }

    [TestFixture]
    public sealed class ReversePostOrderTests : ConfigDependentTests
    {
        [Test]
        public void ReversePostOrderAlgorithm1()
        {
            string phpCode = @"<?php 
$i = 0; 

if (true) {
$x;
} else if (true) {
$y;
} else {
$sdf;
}
echo $x;";
            var cfg = PHPParseUtils.ParseAndIterate<CFGCreator>(phpCode, Config.PHPSettings.PHPParserPath).Graph;
            new CFGPruner().Prune(cfg);

            //cfg.VisualizeGraph("cfg", Config.GraphSettings);

            var postOrder = new ReversePostOrderAlgorithm(cfg).CalculateReversePostOrder();


        }
    }
}
