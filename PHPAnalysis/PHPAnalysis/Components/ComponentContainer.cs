using System.Collections.Generic;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Parsing.AstTraversing;

namespace PHPAnalysis.Components
{
    internal sealed class ComponentContainer
    {
        public ICollection<IXmlVisitor> AstVisitors { get; private set; } 

        public ICollection<IBlockAnalyzerComponent> BlockAnalyzers { get; private set; }

        public ICollection<IVulnerabilityReporter> VulnerabilityReporters { get; private set; } 

        public ICollection<ITaintProvider> TaintProviders { get; private set; }

        public ICollection<IAnalysisStartingListener> AnalysisStartingListeners { get; private set; }
        
        public ICollection<IAnalysisEndedListener> AnalysisEndedListeners { get; private set; }  

        public ComponentContainer()
        {
            this.AstVisitors = new List<IXmlVisitor>();
            this.BlockAnalyzers = new List<IBlockAnalyzerComponent>();
            this.VulnerabilityReporters = new List<IVulnerabilityReporter>();
            this.TaintProviders = new List<ITaintProvider>();
            this.AnalysisStartingListeners = new List<IAnalysisStartingListener>();
            this.AnalysisEndedListeners = new List<IAnalysisEndedListener>();
        }
    }
}