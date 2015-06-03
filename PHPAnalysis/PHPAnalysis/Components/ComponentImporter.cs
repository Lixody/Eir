using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Annotations;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Components
{
    internal sealed class ComponentImporter
    {
        [ImportMany(typeof(IXmlVisitor)), UsedImplicitly]
        private  IEnumerable<IXmlVisitor> AstTraversers { get; set; }

        [ImportMany(typeof(IVulnerabilityReporter)), UsedImplicitly]
        private IEnumerable<IVulnerabilityReporter> VulnerabilityReporters { get; set; } 

        [ImportMany(typeof(ITaintProvider)), UsedImplicitly]
        private IEnumerable<ITaintProvider> TaintProviders { get; set; } 

        [ImportMany(typeof(IBlockAnalyzerComponent)), UsedImplicitly]
        private IEnumerable<IBlockAnalyzerComponent> BlockAnalyzers { get; set; }

        [ImportMany(typeof(IAnalysisStartingListener)), UsedImplicitly]
        private IEnumerable<IAnalysisStartingListener> AnalysisStartingListener { get; set; }

        [ImportMany(typeof(IAnalysisEndedListener)), UsedImplicitly]
        private IEnumerable<IAnalysisEndedListener> AnalysisEndedListeners { get; set; }

        public ComponentContainer ImportComponents(string componentPath)
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(componentPath));
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(catalog);
            
            container.ComposeParts(this);

            return CreateComponentContainer();
        }

        private ComponentContainer CreateComponentContainer()
        {
            var componentContainer = new ComponentContainer();

            componentContainer.AstVisitors.AddRange(this.AstTraversers);
            componentContainer.VulnerabilityReporters.AddRange(this.VulnerabilityReporters);
            componentContainer.BlockAnalyzers.AddRange(this.BlockAnalyzers);
            componentContainer.TaintProviders.AddRange(this.TaintProviders);
            componentContainer.AnalysisStartingListeners.AddRange(this.AnalysisStartingListener);
            componentContainer.AnalysisEndedListeners.AddRange(this.AnalysisEndedListeners);

            return componentContainer;
        }
    }
}
