using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using File = PHPAnalysis.Data.File;

namespace PHPAnalysis.Tests.Analysis
{
    class StoredVulnTests : ConfigDependentTests
    {
        [Test]
        public void StoredVulnInFunction()
        {
            string phpCode = @"<?php $test = 'INSERT INTO products (product_name) VALUES (' . $_GET['test'] . ')';
                                     mysql_query($test);
                                     $result = mysql_query('SELECT product_name FROM products');
                                     echo $result;
                                     mysql_query('INSERT INTO products (product_name) VALUES (' . $_GET['test2'] . ')');
                                     $result = mysql_query('SELECT product_name FROM products');
                                     echo $result;";
            var reporter = new Mock<IVulnerabilityReporter>();
            var vulnStorage = new ReportingVulnerabilityStorage(reporter.Object);

            ParseAndAnalyze(phpCode, vulnStorage);
            vulnStorage.CheckForStoredVulnerabilities();

            reporter.Verify(x => x.ReportStoredVulnerability(It.IsAny<IVulnerabilityInfo[]>()), Times.Exactly(2));
        }

        [TestCase(@"<?php $test = 'folks';
                          mysql_query('INSERT INTO ' . $test . ' VALUES (' . $_GET['a'] . ')');
                          $result = mysql_query('SELECT asdf FROM folks');
                          echo $result;", 1),
        TestCase(@"<?php $test = 'folks'; $test .= 'asddf';
                          mysql_query('INSERT INTO ' . $test . ' VALUES (' . $_GET['a'] . ')');
                          $result = mysql_query('SELECT asdf FROM folksasddf');
                          echo $result;", 1),]
        public void Stored_SqlConcatenated(string php, int vulns)
        {
            var reporter = new Mock<IVulnerabilityReporter>();
            var vulnStorage = new ReportingVulnerabilityStorage(reporter.Object);

            ParseAndAnalyze(php, vulnStorage);
            vulnStorage.CheckForStoredVulnerabilities();

            reporter.Verify(x => x.ReportStoredVulnerability(It.IsAny<IVulnerabilityInfo[]>()), Times.Exactly(vulns));
        }

        [TestCase(@"<?php $sup = new mysqli();
                          $sup->query(""insert into opgave values "" . $_GET[""test""]);
                          $rs2 = $sup->query(""select * from opgave where id = "" . $_GET[""opgave""]);
                          $rs3 = $rs2->fetch_assoc();
                          echo $rs3[""id""];", 1)]
        [TestCase(@"<?php mysql_query('insert into test values ' . $_GET['test']);
                          $var = mysql_query('SELECT * FROM test');
                          echo $var[0]->something;", 1)]
        [TestCase(@"<?php mysql_query('insert into test values ' . $_GET['test']);
                          $q = mysql_query('SELECT * FROM test');
                          $var = mysql_fetch_array($q);
                          echo $var;", 1)]
        public void StoredVulns_SQLMethods(string phpCode, int vulns)
        {
            var reporter = new Mock<IVulnerabilityReporter>();
            var vulnStorage = new ReportingVulnerabilityStorage(reporter.Object);
            
            ParseAndAnalyze(phpCode, vulnStorage);
            vulnStorage.CheckForStoredVulnerabilities();

            reporter.Verify(x => x.ReportStoredVulnerability(It.IsAny<IVulnerabilityInfo[]>()), Times.Exactly(vulns));
        }

        [TestCase(@"<?php $sup = new mysqli();
                          $sup->query(""insert into opgave values "" . $_GET[""test""]);
                          $rs2 = $sup->query(""select * from opgave where id = "" . $_GET[""opgave""]);
                          $rs3 = $rs2->fetch_assoc();
                          $temp = (int)$rs3['id'];
                          $temp = htmlentities($temp);
                          echo $rs3[""id""];", 1)]
        public void StoredVulns_TempSanitize(string phpCode, int vulns)
        {
            var reporter = new Mock<IVulnerabilityReporter>();
            var vulnStorage = new ReportingVulnerabilityStorage(reporter.Object);

            ParseAndAnalyze(phpCode, vulnStorage);
            vulnStorage.CheckForStoredVulnerabilities();

            reporter.Verify(x => x.ReportStoredVulnerability(It.IsAny<IVulnerabilityInfo[]>()), Times.Exactly(vulns));
        }

        private void ParseAndAnalyze(string php, IVulnerabilityStorage storage)
        {
            var extractedFuncs = PHPParseUtils.ParseAndIterate<ClassAndFunctionExtractor>(php, Config.PHPSettings.PHPParserPath).Functions;
            FunctionsHandler.Instance.CustomFunctions.AddRange(extractedFuncs);

            var cfg = PHPParseUtils.ParseAndIterate<CFGCreator>(php, Config.PHPSettings.PHPParserPath).Graph;

            var incResolver = new IncludeResolver(new List<File>());
            var fileStack = new Stack<File>();
            fileStack.Push(new File() { FullPath = @"C:\TestFile.txt" });
            var condAnalyser = new ConditionTaintAnalyser(AnalysisScope.File, incResolver, fileStack);

            var funcMock = new Mock<Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage>>();
            var blockAnalyzer = new TaintBlockAnalyzer(storage, incResolver, AnalysisScope.File, funcMock.Object, new AnalysisStacks(fileStack));
            var immutableInitialTaint = new DefaultTaintProvider().GetTaint();
            var cfgTaintAnalysis = new TaintAnalysis(blockAnalyzer, condAnalyser, immutableInitialTaint);
            var taintAnalysis = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new QueueWorklist());
            taintAnalysis.Analyze(cfg);
            
        }
    }
}
