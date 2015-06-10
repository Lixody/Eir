using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class FunctionAndMethodAnalyzerFactory
    {
        public bool UseSummaries { get; set; }

        public FunctionAndMethodAnalyzer Create(ImmutableVariableStorage variableStorage, IIncludeResolver incResolver,
            AnalysisStacks stacks, CustomFunctionHandler customFuncHandler,
            IVulnerabilityStorage vulnerabilityStorage)
        {
            return new FunctionAndMethodAnalyzer(variableStorage, incResolver, stacks, customFuncHandler, vulnerabilityStorage)
                   {
                       UseSummaries = this.UseSummaries
                   };
        }
    }
}