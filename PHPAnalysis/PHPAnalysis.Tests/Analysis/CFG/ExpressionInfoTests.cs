using System.Linq;
using NUnit.Framework;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;

namespace PHPAnalysis.Tests.Analysis.CFG
{
    [TestFixture]
    class ExpressionInfoTests
    {
        [Test]
        public void ExpressionInfo_Merge()
        {
            var sqliTaint = new SQLITaintSet(SQLITaint.SQL_ALL);
            var xsstaint = new XSSTaintSet(XSSTaint.XSS_ALL);
            var ts1 = new TaintSets(sqliTaint, xsstaint);
            var exprInfo1 = new ExpressionInfo { ExpressionTaint = ts1 };
            var exprInfo2 = new ExpressionInfo();
            
            var exprInfo = exprInfo2.Merge(exprInfo1);

            Assert.AreEqual(sqliTaint, exprInfo.ExpressionTaint.SqliTaint.Single(), "SQL Taint was not the expected");
            Assert.AreEqual(xsstaint, exprInfo.ExpressionTaint.XssTaint.Single(), "XSS Taint was not the expected");
        }
    }
}
