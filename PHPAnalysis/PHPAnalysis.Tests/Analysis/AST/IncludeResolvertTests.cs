using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Moq;
using NUnit.Framework;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing;
using PHPAnalysis.Tests.TestUtils;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Tests.Analysis.AST
{
    [TestFixture]
    public class IncludeResolvertTests : ConfigDependentTests
    {
        [TestCase(@"<?php include('./j.php');", new[] {@"j.php"}, true),
        TestCase(@"<?php include('./j.php');", new[] { @"f.php" }, false),
        TestCase(@"<?php require('./j.php');", new[] { @"j.php" }, true),
        TestCase(@"<?php include_once('./j.php');", new[] { @"j.php" }, true),
        TestCase(@"<?php require_once('./j.php');", new[] { @"j.php" }, true),
        TestCase(@"<?php include(__FILE__ . '/j.php');", new[] { @"j.php" }, true),
        TestCase(@"<?php include(__FILE__ . '/' . 'j.php');", new[] { @"j.php" }, true),
        TestCase(@"<?php include(__FILE__ . $_GET['a']);", new[] { @"j.php" }, false),]
        public void ResolveInclude(string phpCode, string[] existingFiles, bool shouldResolve)
        {
            var includeResolver = new IncludeResolver(existingFiles.Select(f => new File() {FullPath = f}).ToList());

            var ast = PHPParseUtils.ParsePHPCode(phpCode, Config.PHPSettings.PHPParserPath);

            ast.IterateAllNodes(node =>
            {
                if (node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Expr_Include)
                {
                    File file;
                    if (includeResolver.TryResolveInclude(node, out file))
                    {
                        Assert.IsTrue(shouldResolve);
                    }
                    else
                    {
                        Assert.IsFalse(shouldResolve);
                    }
                }
                return true;
            });
        }
    }
}
