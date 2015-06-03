using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Moq;
using NUnit.Framework;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.Data;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis
{
    [TestFixture]
    public class TestWithNoGoodLocationYet : ConfigDependentTests
    {
        //[TestCase(@"<?php $a = 1;")]
        //[TestCase(@"<?php $a = ""asdf"";")]
        //[TestCase(@"<?php $a = 1;")]
        //[TestCase(@"<?php $a = 2.2;")]
        //[TestCase(@"<?php $a = NULL;")]
        //[TestCase(@"<?php $a = array();")]
        //[TestCase(@"<?php $a = new John();")]
        //[TestCase(@"<?php $b = $a;")]
        //[TestCase(@"<?php $b->a = ""john"";")]
        //[TestCase(@"<?php $b->s->a = ""john2"";")]
        //[TestCase(@"<?php b::$staticvar = ""asdf"";", Ignore = true, IgnoreReason = "Not currently supported")]
        //[TestCase(@"<?php b::$$staticvar = 'asdf';", Ignore = true, IgnoreReason = "Not currently supported")]
        //[TestCase(@"<?php $sd::$$jdf = 'asdf';", Ignore = true, IgnoreReason = "Not currently supported")]
        //[TestCase(@"<?php self::$var = 1;", Ignore = true, IgnoreReason = "Not currently supported")]
        //[TestCase(@"<?php parent::$asdf = 1;", Ignore = true, IgnoreReason = "Not currently supported")]
        //[TestCase(@"<?php $c[] = 1;")]
        //[TestCase(@"<?php $c[] = [1,2,3];")]
        //[TestCase(@"<?php $c[1] = ""asdf"";")]
        //[TestCase(@"<?php $c[""asdf""] = ""asdf"";")]
        //[TestCase(@"<?php $c[""a""][] = ""fds"";")]
        //[TestCase(@"<?php $c[func()] = ""asdf"";")]
        //[TestCase(@"<?php $d = myFunc();")]
        //[TestCase(@"<?php list($a, $b) = array(1,2);", Ignore = true, IgnoreReason = "Not currently supported")]
        //public void Assignments(string code)
        //{
        //    var cfg = ParsePHPCode(code).Graph;
        //    var analysis = new CFGTraverser(new ForwardTraversal(), new ReachingDefinitionAnalysis(), new QueueWorklist());
        //    analysis.Analyze(cfg);
        //}

        [TestCase(AstConstants.Node, typeof(AstConstants.Nodes))]
        [TestCase(AstConstants.Subnode, typeof(AstConstants.Subnodes))]
        [TestCase(AstConstants.Scalar, typeof(AstConstants.Scalars))]
        [TestCase(AstConstants.Attribute, typeof(AstConstants.Attributes))]
        public void InterpreterAnalysis_ShouldSupportAllTypes(string prefix, Type type)
        {
            var sut = new Mock<AstNodeAnalyzer<int, int>>();

            var nodeTypes = type.GetConstants();
            var doc = new XmlDocument();
     
            foreach (var node in nodeTypes)
            {
                var xmlNode = doc.CreateNode(XmlNodeType.Element, prefix, (string)node.GetValue(null), "someUri");
                sut.Object.Analyze(xmlNode);
            }
        }

        private CFGCreator ParsePHPCode(string php)
        {
            return PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
