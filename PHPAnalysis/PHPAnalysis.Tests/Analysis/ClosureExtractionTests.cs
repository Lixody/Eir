using System.Linq;
using System.Xml;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Data;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Tests.Analysis;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests
{
    [TestFixture]
    public class ClosureExtractionTests : ConfigDependentTests
    {
        [TestCase(@"<?php $greet = function($name) { printf(""Hello % s\r\n"", $name); };")]
        public void ClosureExtraction_SingleClosureWithParameters_1(string phpCode)
        {
            var extractor = ParseAndExtract(phpCode);

            Closure closure = extractor.Closures.Single();

            Assert.AreEqual(1, extractor.Closures.Count, "Should be 1 closure in code.");
            Assert.AreEqual(1, closure.StartLine, "StartLine is not correct");
            Assert.AreEqual(1, closure.EndLine, "EndLine is not correct");
            Assert.AreEqual(1, closure.Parameters.Length, "Not enough parameters");
        }

        [Test]
        public void ClosureExtraction_NoClosure_1()
        {
            string phpCode = @"<?php ";
            var extractor = ParseAndExtract(phpCode);

            Assert.IsEmpty(extractor.Closures, "Should be 0 closures in code.");
        }

        [TestCase(@"<?php $greet = function($name) use ($m) { echo $m; };", new[] {"m"}),
         TestCase(@"<?php $greet = function($name) use ($m, $ma, &$asd) { echo $m; };", new[] { "m", "ma", "asd" }),]
        public void ClosureExtraction_CapturingClosure(string phpCode, string[] captureNames)
        {
            var extractor = ParseAndExtract(phpCode);

            Closure closure = extractor.Closures.Single();

            Assert.AreEqual(1, extractor.Closures.Count, "Should be 1 closure in code.");
            Assert.AreEqual(1, closure.Parameters.Length, "Not enough parameters");
            CollectionAssert.AreEqual(captureNames, closure.UseParameters.Select(u => u.Name).ToArray(), "Captured names should match");
        }

        [TestCase(@"<?php $greet = function($name) use (&$m) { echo $m; };")]
        public void ClosureExtraction_ClosureUseByRef(string phpCode)
        {
            var extractor = ParseAndExtract(phpCode);

            Closure closure = extractor.Closures.Single();

            Assert.IsTrue(closure.UseParameters.Single().ByReference, "use param by reference");
        }

        [TestCase(@"<?php function asdf($callback) {
                        return function($v) use ($callback) { return $v; };
                    }")]
        public void ClosureExtraction_ClosureInsideFunction(string phpCode)
        {
            var extractor = ParseAndExtract(phpCode);

            Closure closure = extractor.Closures.Single();

            Assert.AreEqual(1, extractor.Closures.Count, "Should be 1 closure in code.");
            Assert.AreEqual(1, closure.Parameters.Length, "Not enough parameters");
        }

        private ClassAndFunctionExtractor ParseAndExtract(string php)
        {
            return PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}
