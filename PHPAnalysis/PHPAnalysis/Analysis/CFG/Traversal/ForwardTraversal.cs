using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using QuickGraph;
using QuickGraph.Algorithms;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class ForwardTraversal : ITraversalTechnique
    {
        public IEnumerable<CFGBlock> GetStartBlocks(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            return graph.Roots().Where(r => r.IsRoot);
        }

        public IEnumerable<TaggedEdge<CFGBlock, EdgeTag>> NextEdges(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock block)
        {
            return graph.OutEdges(block);
        }

        public CFGBlock EdgeTarget(Edge<CFGBlock> edge)
        {
            return edge.Target;
        }
    }
}