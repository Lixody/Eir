using NUnit.Framework;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Tests.Analysis;
using PHPAnalysis.Parsing;
using QuickGraph.Algorithms;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests
{
    [TestFixture]
    public class CFGCreator_FunctionCreation_tests : ConfigDependentTests
    {
        [TestCase(@"<?php
function Test($var1)
{
    if($var1)
        return 'fisk';
    else
        echo 'test';
}
?>")]
       [TestCase(@"<?php
function Test($var1)
{
    if($var1 != NULL)
        echo $var1;
    else
        echo 'missing';
}
testStuff('lawl this is fucking fun');?>")]
        public void CFGCreator_RootAndExitIsStmtFunction(string phpcode)
        {
            var extract = ParseAndExtract(phpcode);
            foreach (var func in extract.Functions)
            {
                var ast = func.AstNode;
                var traverser = new XmlTraverser();
                var cfgcreator = new CFGCreator();
                traverser.AddVisitor(cfgcreator);
                traverser.Traverse(ast);

                var graph = cfgcreator.Graph;

                //Root assertions
                Assert.IsTrue(graph.Vertices.First().IsRoot, "first node was not the root node");
                Assert.IsTrue(graph.Vertices.First().IsSpecialBlock, "first node was not marked as IsSpecialBlock");
                graph.AssertInEdges(graph.Vertices.First(), 0, "Entry node - in edges");
                graph.AssertOutEdges(graph.Vertices.First(), 1, "Entry node - out edges");
                Assert.AreEqual(AstConstants.Nodes.Stmt_Function, graph.Vertices.First().ToString());

                //Leaf assertions
                Assert.IsTrue(graph.Vertices.ElementAt(1).IsSpecialBlock, "The element at position 1 was not marked with IsSpecialBlock");
                Assert.AreEqual(true, graph.Vertices.ElementAt(1).IsLeaf, "The element at position 1 was not marked with IsLeaf");
                graph.AssertOutEdges(graph.Vertices.ElementAt(1), 0, "Exit node - out edges");
            }
        }

        public void CFGCreator_RootAndExitIsClousure(string phpCode)
        {
            var extract = ParseAndExtract(phpCode);
            foreach (var closure in extract.Closures)
            {
                var ast = closure.AstNode;
                var traverser = new XmlTraverser();
                var cfgcreator = new CFGCreator();
                traverser.AddVisitor(cfgcreator);
                traverser.Traverse(ast);

                var graph = cfgcreator.Graph;

                //Root
                Assert.IsTrue(graph.Vertices.First().IsRoot, "the first vertix is not the root node");
                Assert.IsTrue(graph.Vertices.First().IsSpecialBlock, "The first node was not marked with IsSpecialBlock");
                graph.AssertInEdges(graph.Vertices.First(), 0, "Entry node contains in edges");
                graph.AssertOutEdges(graph.Vertices.First(), 1,  "Entry node did not have exactly one out edge");
                Assert.AreEqual(AstConstants.Nodes.Expr_Closure, graph.Vertices.First().ToString(), "The root node was not a closure, and was expected to be a closure");

                //Leaf
                Assert.IsTrue(graph.Vertices.ElementAt(1).IsLeaf, "The element at position one was not the exit block");
                Assert.IsTrue(graph.Vertices.ElementAt(1).IsSpecialBlock, "The element at position one was not marked with IsSpecialBlock");
                graph.AssertOutEdges(graph.Vertices.ElementAt(1), 0, "The exit block contained out edged");
                Assert.AreEqual(AstConstants.Nodes.Expr_Closure, graph.Vertices.ElementAt(1).ToString());

            }
        }

        private ClassAndFunctionExtractor ParseAndExtract(string php)
        {
            return PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}

