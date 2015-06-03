using System.Collections.Generic;
using PHPAnalysis.Data.CFG;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public interface ITraversalTechnique
    {
        /// <summary>
        /// Returns the blocks to be used as initial blocks in the traversal. 
        /// In a normal forward analysis, this should return the root block.
        /// </summary>
        IEnumerable<CFGBlock> GetStartBlocks(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph);
        /// <summary>
        /// Return the next edges to consider in the analysis. 
        /// </summary>
        IEnumerable<TaggedEdge<CFGBlock, EdgeTag>> NextEdges(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock block);
        /// <summary>
        /// Returns the block of interest when looking at a specific edge.
        /// E.g. in a forward analysis this would be the edge target. In a backwards analysis it would be the edge source.
        /// </summary>
        CFGBlock EdgeTarget(Edge<CFGBlock> edge);
    }
}