using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils;
using QuickGraph;

namespace PHPAnalysis.Tests.Analysis.Taint
{
    [TestFixture]
    public class TaintAnalysis : ConfigDependentTests
    {
        [TestCase(@"<?php die;", 0),
         TestCase(@"<?php die();", 0),
         TestCase(@"<?php die($_GET['asdf']);", 1),
         TestCase(@"<?php exit;", 0),
         TestCase(@"<?php exit();", 0),
         TestCase(@"<?php exit($_GET['asdf']);", 1)]
        public void XSSVulnDetection_Exit(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php echo -$_GET['asdf'];", 0),
         TestCase(@"<?php echo +$_GET['asdf'];", 0),
         TestCase(@"<?php echo $_GET['asdf'];", 1),]
        public void XSSVulnDetection_UnaryPlusMinus_AlwaysReturnNumeric(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $v = (isset($_GET['a']) && is_numeric($_GET['a'])) ? $_GET['a'] : 1; 
                          echo $v;", 0),
        TestCase(@"<?php $v = is_numeric($_GET['a']) ? $_GET['a'] : 1; echo $v;", 0)]
        public void ConditionalSanitization(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $var = 'a' . $_GET['asdf']; echo $var;", 1),
         TestCase(@"<?php $var = $_GET['asdf'] . 'a'; echo $var;", 1),
         TestCase(@"<?php $var = 'a' . 'a'; echo $var;", 0),
         TestCase(@"<?php $var = $_GET['asdf']; $var .= 'asdf'; echo $var;", 1),
         TestCase(@"<?php $var = 'asdf'; $var .= 'asdf'; echo $var;", 0),
         TestCase(@"<?php $var = 'asdf'; $var .= $_GET['asdf']; echo $var;", 1),
         ]
        public void TaintTracking_StringConcat(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $var = $_GET['asdf']++; echo $var;", 1),
         TestCase(@"<?php $var = ++$_GET['asdf']; echo $var;", 1),
         TestCase(@"<?php $var = $_GET['asdf']; $var++; echo $var;", 1),
         TestCase(@"<?php $_GET['asdf']++; echo $_GET['asdf'];", 1),]
        public void TaintTracking_IncrementDecrement(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $var = $_GET; echo $var['asdf'];", 1),
         TestCase(@"<?php $var = $_GET; echo $var[1][1];", 1),
         TestCase(@"<?php $_GET['asdf'] = 1; $var = $_GET; echo $var['asdf'];", 0),]
        public void TaintTracking_SimpleAssignmentOfArrays(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $v = $_GET['a']; echo is_int($v) ? $v : 'a';", 0),
         TestCase(@"<?php echo is_int($_GET['a']) ? $_GET['a'] : 'a';", 0),
         TestCase(@"<?php echo is_int($_GET['a']) ? 'a' : $_GET['a'];", 1),
         TestCase(@"<?php echo is_int($_GET['a']) ?: $_GET['a'];", 1),
         TestCase(@"<?php echo $_GET['a'] === 'asdf' ? $_GET['a'] : 'a';", 0),
         TestCase(@"<?php echo 'asdf' === $_GET['a'] ? $_GET['a'] : 'a';", 0),
         TestCase(@"<?php echo 'asdf' === $_GET['a'] ? 'a' : $_GET['a'];", 1),
         TestCase(@"<?php echo true ? '' : '';", 0),
         TestCase(@"<?php echo true ? $_GET['1'] : '';", 1),
         TestCase(@"<?php echo false ?: $_GET['1'];", 1),
         TestCase(@"<?php echo is_int($_GET['asdf']) ? $_GET['asdf'] : 1;", 0)]
        public void Ternary(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $v = array(); echo $v;", 0),
         TestCase(@"<?php $v = array('a'); echo $v['a'];", 0),
         TestCase(@"<?php $v = array($_GET['a']); echo $v['a'];", 0),
         TestCase(@"<?php $v = array('a' => $_GET['a']); echo $v['a'];", 1),
         TestCase(@"<?php $v = array(1 => $_GET['a']); echo $v[1]; echo $v['1'];", 2),
         TestCase(@"<?php $v = array(1.1 => $_GET['a']); echo $v[1];", 1),
         //TestCase(@"<?php $v = array(true => $_GET['a'] ); echo $v[1];", 1, Ignore = true, IgnoreReason = "Not currently supported"),
         //TestCase(@"<?php $v = array(false => $_GET['a']); echo $v[0];", 1, Ignore = true, IgnoreReason = "Not currently supported"),
         TestCase(@"<?php $v = array(false => $_GET['a']); echo $v[1];", 0),
         TestCase(@"<?php $v = array('1' => array('2' => $_GET['a'])); echo $v[1][2];", 1),
         TestCase(@"<?php $v[] = $_GET['a']; foreach($v as $k) echo $k;", 1)
        ]
        public void TaintTracking_ArrayDeclaration(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $v = 'asdf'; $s = array('asdf' => $_GET['a']); echo $s[$v];", 1)]
        public void TaintTracking_ArrayAccess(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $adsf ==  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf === ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf !=  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf !== ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php false ||  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php false OR  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php true  AND ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php true  &&  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf <   ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf >   ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php $adsf <=  ($v = $_GET['a']); echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) ==  'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) === 'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) !=  'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) !== 'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) ||  'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) OR  'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) AND 'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) &&  'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) <   'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) >   'adsf'; echo $v;", 1),
         TestCase(@"<?php ($v = $_GET['a']) <=  'adsf'; echo $v;", 1)]
        public void BinaryOperations(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php 
function asdf($c, $v) { echo $v; }
asdf($_GET['a'], 'asdf');
asdf('asdf', $_GET['a']);", 1),
         TestCase(@"<?php 
function asdf($c, $v) { return $v; }
echo asdf($_GET['a'], 'asdf');
echo asdf('asdf', $_GET['a']);", 1),
         TestCase(@"<?php 
function asdf($c, $v) { return $v . $c; }
echo asdf($_GET['a'], 'asdf');
echo asdf('asdf', $_GET['a']);", 2),]
        public void Summaries(string phpCode, int vulns)
        {
            AssertNoOfVulnsInCode(phpCode, vulns);
        }

        [TestCase(@"<?php $v = $_GET['a']; echo ""adsf $v asdf"";", 1),
         TestCase(@"<?php echo ""adsfadsadsfadsfads$_GET[1]"";", 1),
         TestCase(@"<?php echo ""adsf $vgh"";", 0)]
        public void EncapsedStrings(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php include(($v = $_GET['a'])); echo $v;", 1)]
        public void TaintTracking_OddAssignments(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php function dosubtasks($id, $nyforaelder) {
                                dosubtasks($rowu['id'], $blevtil[$rowu['id']]);
                            }
                            dosubtasks(1, 'asdf');", 0)]
        public void RecursiveFunction(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php foreach($_GET as $x => $y) { echo $x; }", 1),
        TestCase(@"<?php foreach($_GET as $x => $y) { echo $ä; }", 0),
        TestCase(@"<?php foreach($_GET as $x => $y) { echo $y; }", 1),
        TestCase(@"<?php foreach($_GET as $y) { echo $y; }", 1),
        TestCase(@"<?php foreach(array('a' => $_GET['a']) as $x => $y) { echo $y; }", 1),
        TestCase(@"<?php foreach(array('a' => $_GET['a']) as $x => $y) { echo $x; }", 0),
        TestCase(@"<?php foreach(array($_GET['1'] => $_GET['a']) as $x => $y) { echo $x; }", 1),
        TestCase(@"<?php $v = $_GET['a']; foreach($asdf as $v => $s) { echo $v; }", 0)
        ]
        public void Foreach(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php include('./file2.php'); echo $v;", @"<?php $v = $_GET['a'];", 1),
        TestCase(@"<?php $v = $_GET['a']; include('./file2.php');", @"<?php echo $v;", 1),
        ]
        public void TaintFlow_AccrossFiles(string file1, string file2, int numberOfVulns)
        {
            AssertNoOfVulnsInMultipleCodeFiles(new [] {
                                                          new Tuple<string, string>("file1.php", file1),
                                                          new Tuple<string, string>("file2.php", file2), 
                                                      }, numberOfVulns);
        }

        [TestCase(@"<?php $mixedArray = array(""a"" => $_GET['a']);
                          $newString = implode("", "", $mixedArray);
                          $query = mysql_query(""SELECT * FROM tmp_users WHERE id IN ($newString)"");", 1),
        TestCase(@"<?php extract($_GET); echo $myVar;", 1),
        TestCase(@"<?php $v = htmlentities($_GET['a']); echo $v;", 0),
        TestCase(@"<?php $v = explode("";;;"", $_GET['a']); echo $v[0];", 1) // IRL example - easy-meta (WP)
        ]
        public void ThroughFunctionCalls(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php $v = $_GET['a'];
                      function adsf() { global $v; echo $v; }
                      adsf();", 1),
         TestCase(@"<?php $v = 'adsf';
                      function adsf() { $v = $_GET['a'];  echo $v; }
                      adsf();", 1),
         TestCase(@"<?php $v = $_GET['a'];
                      function asdf() { global $v; return $v; }
                      echo asdf();", 1),
         TestCase(@"<?php $v = $_GET['a'];
                      function asdf() { global $v; echo $v; $v = 'asdf'; }
                      echo $v;", 1)]
        public void GlobalVariablesInFunctions(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }
        [TestCase(@"<?php mysql_query(addslashes(""asdfasdfasdfdsa""));", 0),] // default should only override if less than argument taint.
        public void DefaultSanitizerTaint(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [TestCase(@"<?php 
	$_REQUEST[""someVar2""] = htmlentities($_REQUEST[""someVar2""]);
    echo $_REQUEST[""someVar2""];", 0),
         TestCase(@"<?php $val == 'true';", 0),
        TestCase(@"<?php if(!($_GET['test'] == 'safe')) { echo $_GET['test']; }
                         else { echo $_GET['test']; }", 1),
        TestCase(@"<?php if($_GET['test'] !== 'safe') { echo $_GET['test']; } 
                         else { echo $_GET['test']; }", 1),
        TestCase(@"<?php if($_GET['test'] != 'safe') { echo $_GET['test']; }
                         else { echo $_GET['test']; }", 1),
        TestCase(@"<?php if(!($_GET['test'] !== 'safe')) { echo $_GET['test']; }
                         else { echo $_GET['test']; }", 1),
        TestCase(@"<?php if(!($_GET['test'] != 'safe')) { echo $_GET['test']; }
                         else { echo $_GET['test']; }", 1),
        TestCase(@"<?php echo $_SERVER['SERVER_SOFTWARE'];", 0),
        TestCase(@"<?php echo ""Server Env: "".$_SERVER['SERVER_SOFTWARE'].""\n"";", 0),
        TestCase(@"<?php echo ""PHP Version: "".phpversion().""\n"";
	echo ""Plugin URL: "".GDE_PLUGIN_URL.""\n"";
	echo ""Server Env: "".$_SERVER['SERVER_SOFTWARE'].""\n"";", 0),
        TestCase(@"<?php $v = htmlentities($_GET['a']);
                         $sql = new mysqli();
                         $sql->query($v);",1)
        ]
        public void JustSomeCodeSamples(string phpCode, int numberOfVulns)
        {
            AssertNoOfVulnsInCode(phpCode, numberOfVulns);
        }

        [Test]
        public void ReportedVarName()
        {
            string phpCode = @"<?php 
$x = $_GET['a'];
$y = 2;
echo $y . $x;";

            var vulnStorage = new ReportingVulnerabilityStorage(new Mock<IVulnerabilityReporter>().Object);

            ParseAndAnalyze(phpCode, vulnStorage);

            // Kenneth this is not supposed to happen? :P The message says var y, but should be var x.
            Assert.IsTrue(vulnStorage.Vulnerabilities.First().Message.Contains("variable: x "));
        }

        [Test]
        public void SanitizationPropagateTaintedArgsAndAnalyzeWithReturn_ShouldCreateTwoVulns()
        {
            string phpcode = @"<?php
$dbConnection = mysqli_connect(""localhost"",""root"",""1234"",""SomeDB"");

$test = $_REQUEST['test'];

$sql = ""SELECT * FROM someTable WHERE id="" . $test;
$sqlQuery = mysqli_query($dbConnection, $sql);

echo $test;

$test = mysqli_real_escape_string($dbConnection, $test);
$sql = ""SELECT * FROM someTable WHERE id="" . $test;
$sqlQuery = mysqli_query($dbConnection, $sql);

$test = htmlentities($test);
echo $test;

$sqlQuery = mysqli_query($db, ""SELECT * FROM someTable WHERE id="" . $test);
?>";
            var vulnStorage = new ReportingVulnerabilityStorage(new Mock<IVulnerabilityReporter>().Object);
            ParseAndAnalyze(phpcode, vulnStorage);
            Assert.AreEqual(2, vulnStorage.DetectedVulns.Count);
            Assert.NotNull(vulnStorage.Vulnerabilities.First(x => x.Message.ToUpper().Contains("SQL")));
            Assert.NotNull(vulnStorage.Vulnerabilities.First(x => x.Message.ToUpper().Contains("XSS")));
        }

        [Test]
        public void UnknownFunctionTest_ShouldCreateOneError()
        {
            string phpCode = @"<?php echo hello($_GET['test']); ?>";
            var vulnStorage = new ReportingVulnerabilityStorage(new Mock<IVulnerabilityReporter>().Object);
            ParseAndAnalyze(phpCode, vulnStorage);
            Assert.AreEqual(1, vulnStorage.DetectedVulns.Count);
            Assert.True(vulnStorage.Vulnerabilities.First().Message.ToUpper().Contains("XSS"));
        }

        [Test]
        public void UnknownFunctionTest_NoTaintedInput()
        {
            string phpCode = @"<?php echo hello('fisk'); ?>";
            var vulnStorage = new ReportingVulnerabilityStorage(new Mock<IVulnerabilityReporter>().Object);
            ParseAndAnalyze(phpCode, vulnStorage);
            Assert.AreEqual(0, vulnStorage.Vulnerabilities.Count());
        }



        [TestCase(@"<?php $function = 'asdf'; 
                          $function('Dynamic function call');"),
         TestCase(@"<?php $var = 'asdf'; $$var = 'asdf';"),
         TestCase(@"<?php $var = 'asdf'; $var = $$var;")]
        public void DynamicFeatures_ShouldNotCrashAnalysis(string phpCode)
        {
            ParseAndAnalyze(phpCode, new Mock<IVulnerabilityStorage>().Object);
        }

        private void AssertNoOfVulnsInCode(string phpCode, int numberOfVulns)
        {
            var vulnStorage = new Mock<IVulnerabilityStorage>();

            ParseAndAnalyze(phpCode, vulnStorage.Object);

            vulnStorage.Verify(x => x.AddVulnerability(It.IsAny<IVulnerabilityInfo>()), Times.Exactly(numberOfVulns));
        }

        private void AssertNoOfVulnsInMultipleCodeFiles(Tuple<string, string>[] codeFiles, int numberOfVulns)
        {
            FunctionsHandler fh = new FunctionsHandler();
            fh.FunctionSpecification = Config.FuncSpecSettings;
            fh.LoadJsonSpecifications();
            
            var vulnStorage = new Mock<IVulnerabilityStorage>();

            var parsedFiles = codeFiles.Select(code => new File(PHPParseUtils.ParsePHPCode(code.Item2, Config.PHPSettings.PHPParserPath)) 
                                                       {
                                                            FullPath = code.Item1,
                                                            CFG =  PHPParseUtils.ParseAndIterate<CFGCreator>(code.Item2, Config.PHPSettings.PHPParserPath).Graph
                                                       }).ToArray();

            Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> fileTaintAnalyzer = null;
            fileTaintAnalyzer = (varStorage, inclResolver, scope, stacks) =>
            {
                Preconditions.NotNull(varStorage, "varStorage");
                Preconditions.NotNull(inclResolver, "inclResolver");
                var fileToAnalyze = stacks.IncludeStack.Peek();
                var blockAnalyzer = new TaintBlockAnalyzer(vulnStorage.Object, inclResolver,
                    scope, fileTaintAnalyzer, stacks, new FunctionAndMethodAnalyzerFactory(), fh);
                var condAnalyser = new ConditionTaintAnalyser(scope, inclResolver, stacks.IncludeStack, fh);
                var cfgTaintAnalysis = new PHPAnalysis.Analysis.CFG.TaintAnalysis(blockAnalyzer, condAnalyser, varStorage);
                var analyzer = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new ReversePostOrderWorkList(fileToAnalyze.CFG));
                
                analyzer.Analyze(fileToAnalyze.CFG);
                return cfgTaintAnalysis.Taints[fileToAnalyze.CFG.Vertices.Single(block => block.IsLeaf)].Out[EdgeType.Normal];
            };

            foreach (var file in parsedFiles)
            {
                var inclusionResolver = new IncludeResolver(parsedFiles);
                var fileStack = new Stack<File>();
                fileStack.Push(file);
                var immutableInitialTaint = new DefaultTaintProvider().GetTaint();

                var stacks = new AnalysisStacks(fileStack);
                fileTaintAnalyzer(immutableInitialTaint, inclusionResolver, AnalysisScope.File, stacks);
            }

            vulnStorage.Verify(x => x.AddVulnerability(It.IsAny<IVulnerabilityInfo>()), Times.Exactly(numberOfVulns));
        }

        private void ParseAndAnalyze(string php, IVulnerabilityStorage storage)
        {
            FunctionsHandler fh = new FunctionsHandler();
            fh.FunctionSpecification = Config.FuncSpecSettings;
            fh.LoadJsonSpecifications();
            
            var extractedFuncs = PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath).Functions;
            fh.CustomFunctions.AddRange(extractedFuncs);

            var cfg = PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath).Graph;

            var incResolver = new IncludeResolver(new List<File>());
            var fileStack = new Stack<File>();
            fileStack.Push(new File() { FullPath = @"C:\TestFile.txt" });
            var condAnalyser = new ConditionTaintAnalyser(AnalysisScope.File, incResolver, fileStack, fh);

            var funcMock = new Mock<Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage>>();
            var blockAnalyzer = new TaintBlockAnalyzer(storage, incResolver, AnalysisScope.File, funcMock.Object,
                new AnalysisStacks(fileStack), new FunctionAndMethodAnalyzerFactory(), fh);
            var immutableInitialTaint = new DefaultTaintProvider().GetTaint();
            var cfgTaintAnalysis = new PHPAnalysis.Analysis.CFG.TaintAnalysis(blockAnalyzer, condAnalyser, immutableInitialTaint);
            var taintAnalysis = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new ReversePostOrderWorkList(cfg));
            taintAnalysis.Analyze(cfg);
        }
    }
}
