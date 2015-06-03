using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    /// <summary>
    /// Represents the information being propagated up through the AST when analyzing a CFG block
    /// in the taint analysis.
    /// </summary>
    public sealed class ExpressionInfo : IMergeable<ExpressionInfo>
    {
        public TaintSets ExpressionTaint { get; set; }
        public StoredVulnInfo ExpressionStoredTaint { get; set; }
        public ValueInfo ValueInfo { get; set; }

        public ExpressionInfo()
        {
            this.ExpressionTaint = new TaintSets().ClearTaint();
            this.ExpressionStoredTaint = new StoredVulnInfo();
            this.ValueInfo = new ValueInfo();
        }

        public ExpressionInfo Merge(ExpressionInfo other)
        {
            return new ExpressionInfo() 
                   {
                       ExpressionTaint = this.ExpressionTaint.Merge(other.ExpressionTaint),
                       ExpressionStoredTaint = this.ExpressionStoredTaint.Merge(other.ExpressionStoredTaint),
                       ValueInfo = this.ValueInfo.Merge(other.ValueInfo)
                   };
        }

        public ExpressionInfo AssignmentClone()
        {
            return new ExpressionInfo() {
                                            ExpressionTaint = this.ExpressionTaint.DeepClone(),
                                            ExpressionStoredTaint = this.ExpressionStoredTaint, // TODO: CLONE!
                                            ValueInfo = this.ValueInfo.AssignmentClone()
                                        };
        }
    }
}