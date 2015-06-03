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
    public class CFGCreator_SkipElements_Tests : ConfigDependentTests
    {
        [TestCase(@"<?php function myFunc() { }"),
         TestCase(@"<?php function myFunc() { echo 'asdf'; }"),
         TestCase(@"<?php class John { function Asdf() {} }"),
         TestCase(@"<?php interface IJohn { function Asdf();}"),
         TestCase(@"<?php trait myTrait { function getReturnType() { /*1*/ } }"),
         TestCase(@"<?php function() { echo 'yoyo';};")]
        public void FileCFG_OnlySkippableElements(string phpCode)
        {
            var graph= ParseAndBuildCFG(phpCode).Graph;
            //graph.VisualizeGraph("graph", this.Config.GraphSettings);
            const int expectedNodes = 2;
            Assert.AreEqual(expectedNodes, graph.VertexCount);
        }

        private CFGCreator ParseAndBuildCFG(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
