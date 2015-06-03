using System.Collections.Generic;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class FunctionSummary
    {
        public string FunctionName { get; private set; }
        public ICollection<ExpressionInfo> ArgInfos { get; private set; }

        public ExpressionInfo ReturnValue { get; set; }

        public FunctionSummary(string functionName)
        {
            Preconditions.NotNull(functionName, "functionName");

            this.FunctionName = functionName;
            this.ArgInfos = new List<ExpressionInfo>();
            //this.GlobalElements = new Dictionary<string, ExpressionInfo>();
            //this.SuperglobalElements = new Dictionary<string, ExpressionInfo>();
            //this.ClassElements = new Dictionary<string, ExpressionInfo>();
        }
    }
}