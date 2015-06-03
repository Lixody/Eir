using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using QuickGraph;
using QuickGraph.Algorithms;

namespace PHPAnalysis.Analysis.CFG
{
    public interface ICFGPruner
    {
        void Prune(BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph);
    }

    public sealed class CFGPruner : ICFGPruner
    {
        private BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph;

        public void Prune(BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            Preconditions.NotNull(graph, "graph");
            this.graph = graph;

            RemoveUnreachableBlocks();
            RemoveEmptyBlocks();

            this.graph = null;
        }

        private void RemoveUnreachableBlocks()
        {
            var root = graph.Roots().Single(v => v.IsSpecialBlock);

            var reachableBlocks = graph.ReachableBlocks(root);
            var unreachableBlocks = graph.Vertices.Except(reachableBlocks).ToList();

            foreach (var unreachableBlock in unreachableBlocks)
            {
                graph.RemoveVertex(unreachableBlock);
            }
        }

        private void RemoveEmptyBlocks()
        {
            //HACK: Not the most efficient way of solving this.
            //-||-: However it works.
            var lastTimeCount = int.MaxValue;
            var numberOfVertices = graph.Vertices.Count();
            while (numberOfVertices != lastTimeCount)
            {
                var toRemove = new List<CFGBlock>();

                foreach (var vertex in graph.Vertices)
                {
                    int inEdgesCount = graph.InEdges(vertex).Count();
                    int outEdgesCount = graph.OutEdges(vertex).Count();

                    if (!(vertex.IsLeaf || vertex.IsRoot || vertex.IsSpecialBlock || vertex.AstEntryNode != null))
                    {
                        if (inEdgesCount == 1 && outEdgesCount == 0)
                        {
                            toRemove.Add(vertex);
                        }
                        else if (inEdgesCount > 0 && outEdgesCount == 1)
                        {
                            foreach (var edge in graph.InEdges(vertex))
                            {
                                var parent = edge.Source;
                                var child = graph.OutEdge(vertex, 0).Target;

                                graph.AddEdge(new TaggedEdge<CFGBlock, EdgeTag>(parent, child, edge.Tag));
                                toRemove.Add(vertex);
                            }
                        }
                    }
                }

                foreach (var vertex in toRemove)
                {
                    graph.RemoveVertex(vertex);
                }

                lastTimeCount = numberOfVertices;
                numberOfVertices = graph.Vertices.Count();
            }
        }
    }
}
