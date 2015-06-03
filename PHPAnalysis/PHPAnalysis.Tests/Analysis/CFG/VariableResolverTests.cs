using System.Linq;
using System.Xml;
using NUnit.Framework;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Data;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis.CFG
{
    [TestFixture]
    public class VariableResolverTests : ConfigDependentTests
    {
        [Test]
        public void ResolveStandardVariable()
        {
            string phpCode = @"<?php $a;";
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>().ToList();
            var varNodes = xmlNodes.Where(node => node.LocalName == AstConstants.Nodes.Expr_Variable);

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            foreach (var varNode in varNodes)
            {
                var result = varResolver.ResolveVariable(varNode);
                Assert.AreEqual("a", result.Variable.Name, "Names should match");
                Assert.IsTrue(result.IsNew, "Variable should be new");
                result = varResolver.ResolveVariable(varNode);
                Assert.AreEqual("a", result.Variable.Name, "Names should still match");
                Assert.IsFalse(result.IsNew, "Variable is no longer new");
            }
        }

        [TestCase(@"<?php $a['asdf'];", "asdf"),
         TestCase(@"<?php $a['asfdsdf']['asdf'];", "asdf"),
         TestCase(@"<?php $a[1];", "1"),
         TestCase(@"<?php $a[4][3][2][1];", "1")]
        public void ResolveArrayElement(string phpCode, string varName)
        {
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>().ToList();
            var varNodes = xmlNodes.Where(node => node.LocalName == AstConstants.Nodes.Expr_ArrayDimFetch);

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            foreach (var varNode in varNodes)
            {
                var result = varResolver.ResolveVariable(varNode);
                Assert.AreEqual(varName, result.Variable.Name, "Names should match");
                Assert.IsTrue(result.IsNew, "Variable should be new");
                result = varResolver.ResolveVariable(varNode);
                Assert.AreEqual(varName, result.Variable.Name, "Names should still match");
                Assert.IsFalse(result.IsNew, "Variable is no longer new");
            }
        }

        [TestCase(@"<?php $a[1]; $a[2];")]
        public void ResolveMultipleArrayElements(string phpCode)
        {
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>().ToList();
            var varNodes = xmlNodes.Where(node => node.LocalName == AstConstants.Nodes.Expr_ArrayDimFetch);

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            var firstVar = varNodes.First();
            var result = varResolver.ResolveVariable(firstVar);
            result = varResolver.ResolveVariable(firstVar);
            Assert.IsFalse(result.IsNew, "Var");
            result = varResolver.ResolveVariable(varNodes.ElementAt(1));
            Assert.IsTrue(result.IsNew, "First lookup of second var");


        }
        [Test]
        public void ResolveArrayElementMixIndexType()
        {
            string phpCode = @"<?php $a['1']; $a[1];";
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>().ToList();
            var varNodes = xmlNodes.Where(node => node.LocalName == AstConstants.Nodes.Expr_ArrayDimFetch);

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            var arrayfetch = varNodes.First();

            var result = varResolver.ResolveVariable(arrayfetch);
            Assert.AreEqual("1", result.Variable.Name, "Names should match");
            Assert.IsTrue(result.IsNew, "Variable should be new");

            result = varResolver.ResolveVariable(varNodes.ElementAt(1));
            Assert.AreEqual("1", result.Variable.Name, "Names should still match");
            Assert.IsFalse(result.IsNew, "Variable is no longer new");
        }

        [TestCase(@"<?php $a->b;", "b", AstConstants.Nodes.Expr_PropertyFetch),
        TestCase(@"<?php $a->b->c;", "c", AstConstants.Nodes.Expr_PropertyFetch),
        TestCase(@"<?php $a->b->c->d;", "d", AstConstants.Nodes.Expr_PropertyFetch),]
        public void ResolvePropertyFetch(string phpCode, string expectedVarName, string nodeType)
        {
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>()
                                         .Where(node => node.LocalName == nodeType);

            var propFetch = xmlNodes.First();

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            var result = varResolver.ResolveVariable(propFetch);
            Assert.AreEqual(expectedVarName, result.Variable.Name, "Name should match");
            Assert.IsTrue(result.IsNew, "New var");
            result = varResolver.ResolveVariable(propFetch);
            Assert.AreEqual(expectedVarName, result.Variable.Name, "Names should still match");
            Assert.IsFalse(result.IsNew, "Variable is no longer new");
        }

        [TestCase(@"<?php a::$b;", "b", AstConstants.Nodes.Expr_StaticPropertyFetch),
        TestCase(@"<?php self::$d;", "d", AstConstants.Nodes.Expr_StaticPropertyFetch, Ignore = true, IgnoreReason = "Currently not supported, since we need class scope handling."),
        TestCase(@"<?php parent::$d;", "d", AstConstants.Nodes.Expr_StaticPropertyFetch, Ignore = true, IgnoreReason = "Currently not supported, since we need class scope handling."),]
        public void ResolveStaticPropertyFetch(string phpCode, string expectedVarName, string nodeType)
        {
            XmlNode ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath).FirstChild.NextSibling;

            var xmlNodes = ast.FirstChild.Cast<XmlNode>()
                                         .Where(node => node.LocalName == nodeType);

            var propFetch = xmlNodes.First();

            var varResolver = new VariableResolver(new VariableStorage(), AnalysisScope.File);

            var result = varResolver.ResolveVariable(propFetch);
            Assert.AreEqual(expectedVarName, result.Variable.Name, "Name should match");
            Assert.IsTrue(result.IsNew, "New var");
            result = varResolver.ResolveVariable(propFetch);
            Assert.AreEqual(expectedVarName, result.Variable.Name, "Names should still match");
            Assert.IsFalse(result.IsNew, "Variable is no longer new");
        }

        [Test]
        [Ignore("This is ignored as we do not keep track of variable values")]
        public void ResolveClassNameInVar()
        {
            string php = @"<?php
class TestClass {
    function __construct() {
        echo 'New Test' . '\n';
    }
}

$class = 'TestClass';
$tmp = new $class;
    ?>";
            //TODO: The class name should be available (somehow) in the var $tmp.
        }
    }
}
