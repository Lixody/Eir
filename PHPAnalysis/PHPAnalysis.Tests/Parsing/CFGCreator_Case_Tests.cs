using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis
{
    class CFGCreator_Case_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php
                    switch(10)
                    {
                        default:
  		                    //echo 'yo';
  		                    break;
	                    case null:
		                    if(1 == 1)
		                    {
			                    //echo 'asdf';
		                    }
		                    break;
	                    case 10:
                            //echo 'yo';
 		                    break; 	
                    }")]
        public void CFGCreation_StandardSwitchCaseWithIf(string phpCode)
        {
            var cfgCreator = ParseAndBuildCFG(phpCode);

            const int expectedNodes = 13;
            Assert.False(cfgCreator.Graph.IsVerticesEmpty, "Vertices are empty!");
            foreach (var vertice in cfgCreator.Graph.Vertices)
            {
                int edgecount = cfgCreator.Graph.OutEdges(vertice).Count();
                Assert.That(edgecount, Is.LessThanOrEqualTo(2),
                            "Incorrect edgecount on vertice!");
            }
            Assert.AreEqual(expectedNodes, cfgCreator.Graph.VertexCount,
                            "If sentence should have " + expectedNodes + " nodes");
        }

        [TestCase(@"<?php switch(10) { }"), 
         TestCase(@"<?php switch(10) { default: }"),
         TestCase(@"<?php switch(10) { case 10: default: }")]
        public void CFGCreation_StandardSwitch_ShouldNotFail(string phpCode)
        {
            var cfg = ParseAndBuildCFG(phpCode).Graph;
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
