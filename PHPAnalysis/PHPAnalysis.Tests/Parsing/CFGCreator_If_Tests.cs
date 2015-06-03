using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Configuration;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class CFGCreator_If_Tests : ConfigDependentTests
    {
        [Test]
        public void CFGCreation_StandardIf()
        {
            #region PHP Code
            string phpCode = @"<?php if(1 == 1)
                                     {
	                                     if(1 == 1)
	                                     {
		                                     //echo 'hello';
	                                     }
                                         //echo 'what';
                                     }
                                     else if(2 == 2)
                                     {
	                                     //echo 'world';
                                     }
                                     else
                                     {
	                                     //echo 'bob';
                                     }";
            #endregion

            var cfgCreator = ParseAndBuildCFG(phpCode);
            
            const int expectedNodes = 2 + 4 + 3 + 4;
            Assert.False(cfgCreator.Graph.IsVerticesEmpty, "Vertices are empty!");
            foreach(var vertice in cfgCreator.Graph.Vertices)
            {
                int edgecount = cfgCreator.Graph.OutEdges(vertice).Count();
                Assert.That(edgecount, Is.LessThanOrEqualTo(2), 
                            "Incorrect edgecount on vertice!");
            }
            Assert.AreEqual(expectedNodes, cfgCreator.Graph.VertexCount, 
                            "If sentence should have " + expectedNodes + " nodes");
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
