using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public interface IBlockAnalyzerComponent
    {
        Func<IVariableStorage, FunctionAndMethodAnalyzer> FunctionMethodAnalyzerFactory { get; set; }

        ExpressionInfo Analyze(XmlNode node, ExpressionInfo exprInfo, IVariableStorage currentStorage, IVulnerabilityStorage vulnStorage);

        ExpressionInfo AnalyzeFunctionCall(XmlNode node, ExpressionInfo exprInfo, IVariableStorage varStorage, IVulnerabilityStorage vulnStorage, IDictionary<uint, ExpressionInfo> argumentInfos, AnalysisStacks analysisStacks);
    }
}