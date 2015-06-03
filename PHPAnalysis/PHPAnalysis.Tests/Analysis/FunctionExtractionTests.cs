using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Tests.Analysis;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests
{
    [TestFixture]
    public class FunctionExtractionTests : ConfigDependentTests
    {
        [Test]
        public void FunctionExtraction_SingleFunctionWithParameters_1()
        {
            string phpCode = @"<?php 
function myFunc(&$var1 = array(""cappuccino""), &...$var2) {
    return $var2 . $var2;
}";

            var extractor = ParseAndExtract(phpCode);

            Assert.AreEqual(1, extractor.Functions.Count, "Should be 1 function in code.");
            Assert.AreEqual("myFunc", extractor.Functions.First().Name, "Name is not correct");
            Assert.AreEqual(2, extractor.Functions.First().StartLine, "StartLine is not correct");
            Assert.AreEqual(4, extractor.Functions.First().EndLine, "EndLine is not correct");
        }

        [Test]
        public void FunctionExtraction_NoFunction_1()
        {
            string phpCode = @"<?php ";
            var extractor = ParseAndExtract(phpCode);

            Assert.IsEmpty(extractor.Functions, "Should be 0 functions in code.");
        }
        [Test]
        public void FunctionExtraction_MultipleFunctions_1()
        {
            string phpCode = @"<?php 
function myFunc(&$var1 = array(""cappuccino""), &...$var2) { return $var2 . $var2; }
function func2(){};
function func3($var) {return 2;}";

            var extractor = ParseAndExtract(phpCode);

            Assert.AreEqual(3, extractor.Functions.Count, "Should be 3 functions in code.");
            Assert.AreEqual("myFunc", extractor.Functions.First().Name, "1st functions name is not correct");
            Assert.AreEqual("func2", extractor.Functions.ElementAt(1).Name, "2nd functions name is not correct");
            Assert.AreEqual("func3", extractor.Functions.ElementAt(2).Name, "3rd functions name is not correct");
        }

        [Test]
        public void FunctionExtraction_NestedFunctions()
        {
            string phpCode = @"<?php 
function myFunc(&$var1 = array(""cappuccino""), &...$var2) { 
    function func2(){ return 'a'; }
    return $var2 . func2(); 
}";

            var extractor = ParseAndExtract(phpCode);
            Assert.AreEqual(2, extractor.Functions.Count, "Expected no of functions.");
            CollectionAssert.AreEquivalent(extractor.Functions.Select(f => f.Name), new[] { "myFunc", "func2" }, "Function names");
        }

        [Test]
        public void ParameterExtraction_SingleParameter_1()
        {
            string phpCode = @"<?php 
function myFunc($var1) { return $var1; }";

            var extractor = ParseAndExtract(phpCode);

            var function = extractor.Functions.First();
            Assert.AreEqual(1, function.Parameters.Count, "Should be 1 parameter");

            var param = function.Parameters.First(x => x.Key.Item1 == 1);
            Assert.IsNotNull(param, "The parameter was null although one was expected");
            Assert.AreEqual("var1", param.Value.Name, "Parameter name is not correct");
            Assert.IsFalse(param.Value.IsVariadic, "Parameter is not variadic.");
            Assert.IsFalse(param.Value.IsOptional, "Parameter does not have a default value");
            Assert.IsFalse(param.Value.ByReference, "Parameter is not by reference");
        }

        [Test]
        public void ParameterExtraction_SingleParameter_2()
        {
            string phpCode = @"<?php 
function myFunc(&$var1) { return $var1; }";

            var extractor = ParseAndExtract(phpCode);

            var function = extractor.Functions.First();
            Assert.AreEqual(1, function.Parameters.Count, "Should be 1 parameter");

            var param = function.Parameters.First(x => x.Key.Item1 == 1);
            Assert.IsNotNull(param, "The Parameter was null although one parameter was expected");
            Assert.AreEqual("var1", param.Value.Name, "Parameter name is not correct");
            Assert.IsFalse(param.Value.IsVariadic, "Parameter is not variadic.");
            Assert.IsFalse(param.Value.IsOptional, "Parameter does not have a default value");
            Assert.IsTrue(param.Value.ByReference, "Parameter is by reference");
        }

        [TestCase(@"<?php function parseDateTime($string, $timezone = null) { }")]
        public void ParameterExtraction_DefaultParamValue(string phpCode)
        {
            var extractor = ParseAndExtract(phpCode);

            var function = extractor.Functions.Single();

            var firstParam = function.Parameters.First(x => x.Key.Item1 == 1);
            var secondParam = function.Parameters.First(x => x.Key.Item1 == 2);


            Assert.AreEqual("string", firstParam.Value.Name);
            Assert.AreEqual("timezone", secondParam.Value.Name);
            Assert.IsTrue(secondParam.Value.IsOptional, "2nd parameter should have default value");
        }

        private ClassAndFunctionExtractor ParseAndExtract(string php)
        {
            return PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
