using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Xml;
using NUnit.Framework;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Data;
using PHPAnalysis.Utils.XmlHelpers;
using Moq;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Parsing;
using PHPAnalysis.Analysis.CFG.Taint;
using System;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests.Analysis
{
    //TODO: All tests should be verifying taintstatutes. And as arguments are no longer extracted with the function call, this should be changes as well.
    [TestFixture]
    public class FunctionCallExtractionTests : ConfigDependentTests
    {
        [Test]
        public void FunctionCallExtration_OneArgumentWhichIsVar()
        {
            string php = @"<?php function Test($var) { echo $var; } $var2 = 'test'; Test($var2); ?>";
            var statements = ParseAndGetAstStatementContent(php);

            foreach (XmlNode node in statements)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_FuncCall)
                {
                    FunctionCall fc = new FunctionCallExtractor().ExtractFunctionCall(node);
                    Assert.AreEqual(1, fc.Arguments.Count);

                    var variableArg = fc.Arguments.FirstOrDefault(x => x.Key == 1);
                    Assert.IsNotNull(variableArg);
                    Assert.AreEqual(variableArg.Value.LocalName, AstConstants.Nodes.Expr_Variable);

                    var varName = variableArg.Value.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name).InnerText;
                    Assert.AreEqual("var2", varName);
                    Assert.AreEqual("Test", fc.Name);
                }
            }
        }

        [TestCase(@"<?php is_int($_GET['asdf']);"),
         TestCase("<?php is_int($_GET);")]
        public void FunctionCallExtraction_OneArgument(string phpCode)
        {
            var statements = ParseAndGetAstStatementContent(phpCode);

            foreach (XmlNode node in statements)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_FuncCall)
                {
                    FunctionCall fc = new FunctionCallExtractor().ExtractFunctionCall(node);
                    Assert.AreEqual(1, fc.Arguments.Count);
                }
            }
        }

        [Test]
        public void FunctionCallExtration_OneArgumentWhichIsClass()
        {
            string php = @"<?php
class ClassOne
{
    private $name;
    function __construct($var1) {
        $this->name = $var1;
    }

    function __toString(){
        return ""ClassOne "" . $this->name;
    }
}

function Test($var)
{
    echo $var;
}

Test(new ClassOne('test'));
?>";
            var statemets = ParseAndGetAstStatementContent(php);

            foreach (XmlNode node in statemets)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_FuncCall)
                {
                    FunctionCall fc = new FunctionCallExtractor().ExtractFunctionCall(node);
                    Assert.AreEqual("Test", fc.Name);
                    Assert.AreEqual(1, fc.Arguments.Count);
                    Assert.AreEqual(AstConstants.Nodes.Expr_New, fc.Arguments.ElementAt(0).Value.LocalName);

                    //Class value is no longer created.
                    /*var cv = (ClassValue)fc.ArgumentValues.ElementAt(0);
                    Assert.AreEqual("ClassOne", cv.ClassName);
                    Assert.AreEqual(1, cv.ClassValues.Count);
                    Assert.AreEqual("test", cv.ClassValues.ElementAt(0).ValueContent);
                    Assert.AreEqual("string", cv.ClassValues.ElementAt(0).Type);*/
                }
            }
        }

        [Test]
        public void FunctionCallExtration_OneArgumentWhichIsInstance()
        {
            var phpCode = @"<?php
function Test($var)
{
    echo $var;
}

Test('test');
?>";
            var statements = ParseAndGetAstStatementContent(phpCode);

            foreach (XmlNode node in statements)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_FuncCall)
                {
                    FunctionCall fc = new FunctionCallExtractor().ExtractFunctionCall(node);
                    Assert.AreEqual("Test", fc.Name);
                    Assert.AreEqual(1, fc.Arguments.Count);

                    var constantArg = fc.Arguments.FirstOrDefault(x => x.Key == 1);
                    Assert.IsNotNull(constantArg);
                    Assert.IsNotNull(constantArg.Value);
                    Assert.AreEqual((AstConstants.Scalar + "_" + AstConstants.Scalars.String).ToUpper(), constantArg.Value.LocalName.ToUpper());

                    var value = constantArg.Value.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value).FirstChild.InnerText;
                    Assert.AreEqual("test", value);
                }
            }
        }

        [Test]
        public void FunctionCallExtration_SeveralArguments()
        {
            var phpCode = @"<?php
function Test($var, $var2, $var3)
{
    echo $var . $var2 . $var3;
}

Test('test', 1, false);
?>";
            var statements = ParseAndGetAstStatementContent(phpCode); 

            foreach (XmlNode node in statements)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_FuncCall)
                {
                    FunctionCall fc = new FunctionCallExtractor().ExtractFunctionCall(node);
                    Assert.AreEqual("Test", fc.Name);
                    Assert.AreEqual(3, fc.Arguments.Count);
                    /*Assert.AreEqual("test", fc.ArgumentValues.ElementAt(0).ValueContent);
                    Assert.AreEqual("1", fc.ArgumentValues.ElementAt(1).ValueContent);
                    Assert.AreEqual("false", fc.ArgumentValues.ElementAt(2).ValueContent);*/
                }
            }
        }

        [Test]
        public void MethodExtraction_NewExpr()
        {
            var phpCode = @"<?php
class ClassOne {
    public $var1;

    function __construct($var) {
        $this->var1 = $var;
    }

    function printOut($extra) {
        echo ""ClassOne: "" . $this->var1 . "" "" . $extra . ""\n"";
    }
}

(new ClassOne('test'))->printOut('fisk');
?>";
            var stmts = ParseAndGetAstStatementContent(phpCode);

            foreach (XmlNode node in stmts)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_MethodCall)
                {
                    MethodCall mc = new FunctionCallExtractor().ExtractMethodCall(node, new Mock<IVariableStorage>().Object);
                    Assert.True(mc.ClassNames.Any(x => x == "ClassOne"));
                    Assert.AreEqual("printOut", mc.Name);
                    Assert.AreEqual(1, mc.Arguments.Count);
                }
            }
        }

        [Test]
        [Ignore("As it is, this test doesn't work. The classname can be resolved during analysis. But not 'directly' as this test tries to do.")]
        public void MethodExtration_ExtractMethodCallFromVar()
        {
            var phpCode = @"<?php
class ClassOne {
    public $var1;

    function __construct($var) {
        $this->var1 = $var;
    }

    function printOut($extra) {
        echo ""ClassOne: "" . $this->var1 . "" "" . $extra . ""\n"";
    }
}

$tmp = new ClassOne('test');
$tmp->printOut('fisk');
?>";
            var stmts = ParseAndGetAstStatementContent(phpCode);

            foreach (XmlNode node in stmts)
            {
                if (node.LocalName == AstConstants.Nodes.Expr_MethodCall)
                {
                    MethodCall mc = new FunctionCallExtractor().ExtractMethodCall(node, new Mock<IVariableStorage>().Object);
                    Assert.AreEqual(1, mc.ClassNames.Count, "The expected number of class names was not correct");
                    Assert.AreEqual("ClassOne", mc.ClassNames.First(), "Wrong class name extracted");
                    Assert.AreEqual("printOut", mc.Name, "Wrong method name extracted");
                    Assert.AreEqual(1, mc.Arguments.Count, "Argument list was not 1 as expected");
                }
            }
        }

        private XmlNode ParseAndGetAstStatementContent(string php)
        {
            var ast = PHPParseUtils.ParsePHPCode(php, Config.PHPSettings.PHPParserPath);
            return ast.FirstChild.NextSibling.FirstChild;
        }
    }
}