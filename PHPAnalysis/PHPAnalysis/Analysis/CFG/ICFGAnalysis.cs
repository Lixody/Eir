using PHPAnalysis.Data.CFG;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public interface ICFGAnalysis
    {
        void Initialize(CFGBlock cfgBlock);
        bool Analyze(TaggedEdge<CFGBlock, EdgeTag> edge);

        bool Analyze2(CFGBlock block, IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph);
    }
}