using System.IO.IsolatedStorage;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public interface ISinkAnalyzer
    {
        bool IsSink(CFGBlock target);
        void AnalyzeSink(CFGBlock target, CFGTaintInfo taintInfo);
    }

    public sealed class XSSSinkAnalyzer : ISinkAnalyzer
    {
        private readonly string[] sinks = {
                                              AstConstants.Nodes.Stmt_Echo, 
                                              AstConstants.Nodes.Expr_Print,
                                          };

        private readonly IVulnerabilityReporter vulnerabilityReporter;

        public XSSSinkAnalyzer(IVulnerabilityReporter vulnerabilityReporter)
        {
            Preconditions.NotNull(vulnerabilityReporter, "vulnerabilityReporter");
            this.vulnerabilityReporter = vulnerabilityReporter;
        }

        public bool IsSink(CFGBlock target)
        {
            Preconditions.NotNull(target, "target");

            if (target.AstEntryNode == null) { return false; }

            return this.sinks.Contains(target.AstEntryNode.LocalName);
        }

        public void AnalyzeSink(CFGBlock target, CFGTaintInfo taintInfo)
        {
            switch (target.AstEntryNode.LocalName)
            {
                case AstConstants.Nodes.Stmt_Echo:
                    AnalyzeEcho(target, taintInfo);
                    break;
                case AstConstants.Nodes.Expr_Print:
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeEcho(CFGBlock block, CFGTaintInfo taintInfo)
        {
            //var xssTaintedVars = taintInfo.In.Where(info => info.Value.XssTaint.TaintTags.Contains(XSSTaint.XSS_ALL))
            //                                 .Select(info => info.Key);
            //foreach (var taintedVar in xssTaintedVars)
            //{
            //    if (block.AstEntryNode.InnerText.Contains(taintedVar))
            //    {
            //        vulnerabilityReporter.ReportVulnerability(block, "XSS");
            //    }
            //}
        }
    }
}