using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Data.CFG;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class BackwardTraversal : ITraversalTechnique
    {
        public IEnumerable<CFGBlock> GetStartBlocks(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            return graph.Vertices.Where(v => v.IsLeaf);
        }

        public IEnumerable<TaggedEdge<CFGBlock, EdgeTag>> NextEdges(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock block)
        {
            return graph.InEdges(block);
        }

        public CFGBlock EdgeTarget(Edge<CFGBlock> edge)
        {
            return edge.Source;
        }
    }
}