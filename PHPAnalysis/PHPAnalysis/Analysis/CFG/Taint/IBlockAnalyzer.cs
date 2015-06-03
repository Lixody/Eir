using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public interface IBlockAnalyzer
    {
        ImmutableVariableStorage Analyze(XmlNode node, ImmutableVariableStorage knownTaint);
    }
}