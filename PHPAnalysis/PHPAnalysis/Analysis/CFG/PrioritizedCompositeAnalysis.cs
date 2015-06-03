using System;
using System.Collections.Generic;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class PrioritizedCompositeAnalysis<TBlockResult> : ICFGAnalysis
    {
        private readonly SortedList<uint, ICFGAnalysis> _analyses;

        public PrioritizedCompositeAnalysis()
        {
            this._analyses = new SortedList<uint, ICFGAnalysis>();
        }

        public void AddAnalysis(ICFGAnalysis analysis, uint priority)
        {
            Preconditions.NotNull(analysis, "analysis");
            string message = "Two analyses cannot have the same priority (" + priority + ").";
            Preconditions.IsFalse(_analyses.ContainsKey(priority), message, "priority");

            _analyses.Add(priority, analysis);
        }

        public void Initialize(CFGBlock cfgBlock)
        {
            foreach (var cfgAnalysis in _analyses)
            {
                cfgAnalysis.Value.Initialize(cfgBlock);
            }
        }

        public bool Analyze(TaggedEdge<CFGBlock, EdgeTag> edge)
        {
            var didAnyChange = false;
            foreach (var cfgAnalysis in _analyses)
            {
                if (cfgAnalysis.Value.Analyze(edge))
                {
                    didAnyChange = true;
                }
            }
            return didAnyChange;
        }

        public bool Analyze2(CFGBlock block, IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            throw new NotImplementedException();
        }
    }
}