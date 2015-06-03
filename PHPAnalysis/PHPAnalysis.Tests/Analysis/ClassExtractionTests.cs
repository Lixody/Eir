using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Tests.Analysis;
using PHPAnalysis.Tests.TestUtils;

namespace PHPAnalysis.Tests
{
    [TestFixture]
    public class ClassExtractionTests : ConfigDependentTests
    {
        [Test]
        public void ClassExtraction_SingleClass_1()
        {
            string php = @"<?php class Foo { }";

            var extractor = ParseAndExtract(php);

            Assert.AreEqual("Foo", extractor.Classes.Single().Name, "Class name is not correct");
        }
        [Test]
        public void ClassExtraction_MultipleClasses_1()
        {
            string php = @"<?php class Foo { } class Bar { }";

            var extractor = ParseAndExtract(php);

            Assert.AreEqual(2, extractor.Classes.Count, "There should be 2 classes");
            Assert.AreEqual("Foo", extractor.Classes.First().Name, "Class name is not correct");
            Assert.AreEqual("Bar", extractor.Classes.ElementAt(1).Name, "2nd class name is not correct");
            Assert.AreEqual(1, extractor.Classes.First().StartLine, "1st class startline is not correct");
            Assert.AreEqual(1, extractor.Classes.First().EndLine, "1st class endline is not correct");
        }

        [Test]
        public void MethodExtraction_SingleMethodInClass_1()
        {
            string php = @"<?php 
class Foo { 
    function aMemberFunc() { 
        print 'Inside `aMemberFunc()`'; 
    } 
} ";
            var extractor = ParseAndExtract(php);

            Assert.AreEqual("aMemberFunc", extractor.Classes.Single().Methods.Single().Name, "Method name is not correct");
            Assert.AreEqual(2, extractor.Classes.First().StartLine, "Class startline is not correct");
            Assert.AreEqual(6, extractor.Classes.First().EndLine, "Class endline is not correct");
        }

        [Test]
        public void MethodExtraction_MultipleMethodsInClass_1()
        {
            string php = @"<?php 
class Foo { 
    function aMemberFunc() { 
        print 'Inside `aMemberFunc()`'; 
    } 
    function aMemberFunc1() { 
        print 'Inside `aMemberFunc()`'; 
    }
    function John() { 
        print 'Inside `aMemberFunc()`'; 
    }  
} ";
            var extractor = ParseAndExtract(php);

            Assert.AreEqual("aMemberFunc", extractor.Classes.Single().Methods.First().Name, "1st method name is not correct");
            Assert.AreEqual("aMemberFunc1", extractor.Classes.Single().Methods[1].Name, "2st method name is not correct");
            Assert.AreEqual("John", extractor.Classes.Single().Methods[2].Name, "3st method name is not correct");
        }

        [TestCase(@"<?php class TestTwo { function __construct() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __destruct() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __call() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __callStatic() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __get() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __set() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __isset() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __unset() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __sleep() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __wakeup() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __toString() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __invoke() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __set_state() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __clone() { } } ?>")]
        [TestCase(@"<?php class TestTwo { function __debugInfo() { } } ?>")]
        public void MethodExtraction_MarkedAsMagicMethod(string phpcode)
        {
            var extract = ParseAndExtract(phpcode);
            foreach (var method in extract.Classes.SelectMany(@class => @class.Methods))
            {
                Assert.IsTrue(method.IsMagicMethod, method.ToString());
            }
        }

        [TestCase(@"<?php class TestTwo { function TestMethod() { } } ?>"),
         TestCase(@"<?php class TestTwo { 
                        function _construct() { }  
                        function _destruct() { }
                        function _call() { } 
                        function _callStatic() { }
                        function _get() { }       
                        function _set() { }       
                        function _isset() { }     
                        function _unset() { }     
                        function _sleep() { }     
                        function _wakeup() { }    
                        function _toString() { }  
                        function _invoke() { }    
                        function _set_state() { } 
                        function _clone() { }     
                        function _debugInfo() { }   }?>")]
        public void MethodExtraction_StandardMethod_NotMarkedAsMagicMethod(string phpcode)
        {
            var extract = ParseAndExtract(phpcode);
            foreach (var method in extract.Classes.SelectMany(@class => @class.Methods))
            {
                Assert.IsFalse(method.IsMagicMethod);
            }
        }

        [TestCase(@"<?php class TestTwo { var $prop; }", 1),
         TestCase(@"<?php class TestTwo { var $prop; var $prop; var $prop; var $prop; var $prop;}", 5)]
        public void PropertyExtraction_IndividualProperties(string phpCode, int properties)
        {
            var extract = ParseAndExtract(phpCode);
            Assert.AreEqual(properties, extract.Classes.SelectMany(c => c.Properties).Count());
            foreach (var property in extract.Classes.SelectMany(c => c.Properties))
            {
                Assert.AreEqual("prop", property.Name);
            }
        }

        [TestCase(@"<?php class Test { var $prop; }", AstConstants.VisibilityModifiers.Public),
         TestCase(@"<?php class Test { public $prop; }", AstConstants.VisibilityModifiers.Public),
         TestCase(@"<?php class Test { private $prop; }", AstConstants.VisibilityModifiers.Private),
         TestCase(@"<?php class Test { protected $prop; }", AstConstants.VisibilityModifiers.Protected),
         TestCase(@"<?php class Test { var $prop,$prop,$prop,$prop; }", AstConstants.VisibilityModifiers.Public),
         TestCase(@"<?php class Test { public $prop,$prop,$prop,$prop; }", AstConstants.VisibilityModifiers.Public),
         TestCase(@"<?php class Test { private $prop,$prop,$prop,$prop; }", AstConstants.VisibilityModifiers.Private),
         TestCase(@"<?php class Test { protected $prop,$prop,$prop,$prop; }", AstConstants.VisibilityModifiers.Protected),]
        public void PropertyExtraction_VisibilityModifier(string phpCode, AstConstants.VisibilityModifiers modifiers)
        {
            var extract = ParseAndExtract(phpCode);
            foreach (var property in extract.Classes.SelectMany(c => c.Properties))
            {
                Assert.AreEqual(modifiers, property.VisibilityModifiers, "Property: " + property);
            }
        }

        private ClassAndFunctionExtractor ParseAndExtract(string php)
        {
            return PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath);
        }
    }
}