using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG.Traversal
{
    public class CFGTraverser
    {
        private readonly ICFGAnalysis _analysis;
        private readonly IWorklist<CFGBlock> _workList;
        private readonly ITraversalTechnique _traversalTechnique;

        private readonly HashSet<CFGBlock> _visited;

        public CFGTraverser(ITraversalTechnique traversalStrategy, ICFGAnalysis analysis, IWorklist<CFGBlock> worklist)
        {
            Preconditions.NotNull(traversalStrategy, "traversalStrategy");
            Preconditions.NotNull(analysis, "analysis");
            Preconditions.NotNull(worklist, "worklist");
            this._traversalTechnique = traversalStrategy;
            this._analysis = analysis;
            this._workList = worklist;
            this._visited = new HashSet<CFGBlock>();
        }

        public void Analyze(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            Preconditions.NotNull(graph, "graph");

            Initialize(graph);
            AddStartingBlocksToWorklist(graph);
            WorklistTraversal2(graph);
        }

        private void Initialize(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            foreach (var cfgBlock in graph.Vertices)
            {
                _analysis.Initialize(cfgBlock);
            }
        }

        private void AddStartingBlocksToWorklist(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            foreach (var startBlock in _traversalTechnique.GetStartBlocks(graph))
            {
                _workList.Add(startBlock);
            }
        }

        private void WorklistTraversal(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            while (_workList.Any())
            {
                var block = _workList.GetNext();
                var nextEdges = _traversalTechnique.NextEdges(graph, block);
                foreach (var edge in nextEdges)
                {
                    var didChange = _analysis.Analyze(edge);
                    var target =_traversalTechnique.EdgeTarget(edge);

                    if ((didChange || !_visited.Contains(target)) && !_workList.Contains(target))
                    {
                        _workList.Add(target);
                    }

                    _visited.Add(target);
                }
            }
        }

        private void WorklistTraversal2(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            while (_workList.Any())
            {
                var block = _workList.GetNext();

                var didChange = _analysis.Analyze2(block, graph);

                var successors = graph.OutEdges(block)
                                      .OrderBy(e => e.Tag.EdgeType)
                                      .Select(e => e.Target);

                foreach (var successor in successors)
                {
                    if (didChange || !_visited.Contains(successor))
                    {
                        _workList.Add(successor);
                    }
                }

                _visited.Add(block);
            }
        }
    }
}
