using System;
using NUnit.Framework;
using PHPAnalysis.Tests.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Parsing;
using PHPAnalysis.Data.CFG;
using QuickGraph.Algorithms;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests
{
    [TestFixture]
    public class CFGCreator_ClassTests : ConfigDependentTests
    {
        [TestCase(@"<?php
        class Test
        {
            function __construct()
            {
            }

            function stuff()
            {
                echo 'test';
            }
        }?>")]
        [TestCase(@"<?php
        class TestTwo
        {
            function __construct($var)
            {
                if($var)
                    echo 'test2';
            }
        }
        ?>")]
        public void CFGCreator_RootAndExitIsStmtClassMethod(string phpcode)
        {
            var extract = ParseAndExtract(phpcode);
            foreach (var @class in extract.Classes)
            {
                foreach (var method in @class.Methods)
                {
                    var ast = method.AstNode;
                    var traverser = new XmlTraverser();
                    var cfgcreator = new CFGCreator();
                    traverser.AddVisitor(cfgcreator);
                    traverser.Traverse(ast);

                    var graph = cfgcreator.Graph;

                    //Root assertions
                    Assert.AreEqual(AstConstants.Nodes.Stmt_ClassMethod, graph.Vertices.First().ToString());
                    graph.AssertInEdges(graph.Vertices.First(), 0, "Entry node - in edges");
                    graph.AssertOutEdges(graph.Vertices.First(), 1, "Entry node - out edges");
                    Assert.AreEqual(true, graph.Vertices.First().IsRoot);

                    //Leaf assertions
                    graph.AssertOutEdges(graph.Vertices.ElementAt(1), 0, "Exit node - out edges");
                    Assert.AreEqual(true, graph.Vertices.ElementAt(1).IsLeaf);
                }
            }
        }
            
        private ClassAndFunctionExtractor ParseAndExtract(string php)
        {
            return PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}