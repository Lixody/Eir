using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    class ReachDefTests : ConfigDependentTests
    {
        [Test]
        public void IfElseVarDefTest()
        {
            const string php = @"<?php
                                 if(true)
                                 {
	                                 $var = 1;
                                 }
                                 else
                                 {
	                                 $var = 2;
                                 }
                                 $var = 3;";

            var cfgCreator = ParseAndBuildCFG(php);
            var reachDef = new ReachingDefinitionAnalysis();
            var analysis = new CFGTraverser(new ForwardTraversal(), reachDef, new QueueWorklist());
            analysis.Analyze(cfgCreator.Graph);

            var inLineNumbers = new List<int>() { 4, 8 };
            var outLineNumbers = new List<int>() { 4, 8, 10 };

            foreach (var block in reachDef.ReachingSetDictionary)
            {
                if (block.Key.AstEntryNode != null && block.Key.AstEntryNode.Name == "node:Expr_Assign")
                {
                    if (block.Value.DefinedInVars.Any())
                    {
                        int ins = AstNode.GetStartLine(block.Value.DefinedInVars.Values.First().Info.Block.AstEntryNode);
                        inLineNumbers.RemoveAll(x => ins == x);
                    }
                    if (block.Value.DefinedOutVars.Any())
                    {
                        int outs = AstNode.GetStartLine(block.Value.DefinedOutVars.Values.First().Info.Block.AstEntryNode);
                        outLineNumbers.RemoveAll(x => x == outs);
                    }
                }
            }
 
            Assert.IsTrue(inLineNumbers.IsEmpty(), "The InLineNumbers are incorrect!");
            Assert.IsTrue(outLineNumbers.IsEmpty(), "The OutLineNumbers are incorrect!");
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
