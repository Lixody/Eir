using System;
using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using QuickGraph;
using YamlDotNet.Core;

namespace PHPAnalysis.Analysis.CFG
{

    public sealed class ReversePostOrderWorkList : IWorklist<CFGBlock>
    {
        private readonly IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> _graph;
        private readonly IDictionary<CFGBlock, int> _blockOrder;

        private readonly SortedList<int, CFGBlock> _workList; 

        public ReversePostOrderWorkList(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            Preconditions.NotNull(graph, "graph");
            this._graph = graph;

            this._workList = new SortedList<int, CFGBlock>();
            this._blockOrder = new ReversePostOrderAlgorithm(graph).CalculateReversePostOrder();
        }

        public bool Any()
        {
            return _workList.Any();
        }

        public void Add(CFGBlock elem)
        {
            int orderNumber = _blockOrder[elem];
            if (!_workList.ContainsKey(orderNumber))
            {
                _workList.Add(orderNumber, elem);
            }
        }

        public CFGBlock GetNext()
        {
            var nextElem = _workList.ElementAt(0).Value;
            _workList.RemoveAt(0);
            return nextElem;
        }

        public bool Contains(CFGBlock elem, IEqualityComparer<CFGBlock> comparer = null)
        {
            int visitNumber = _blockOrder[elem];
            return _workList.ContainsKey(visitNumber);
        }
    }

    public sealed class ReversePostOrderAlgorithm
    {
        private readonly Dictionary<CFGBlock, VisitedNumber> _reversePostOrder;
        private readonly IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> _graph;

        private readonly Dictionary<EdgeType, int> orderMap = new Dictionary<EdgeType, int>() {
            { EdgeType.Normal, 0 },
            { EdgeType.False, 1 },
            { EdgeType.True, 2 }
        };

        public ReversePostOrderAlgorithm(IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            Preconditions.NotNull(graph, "graph");
            this._graph = graph;

            this._reversePostOrder = new Dictionary<CFGBlock, VisitedNumber>();
        }
        public IDictionary<CFGBlock, int> CalculateReversePostOrder()
        {
            MarkAllVerticesUnvisited();

            int vertexCount = _graph.Vertices.Count();

            foreach (KeyValuePair<CFGBlock, VisitedNumber> tuple in _reversePostOrder)
            {
                if (tuple.Value.Visited) { continue; }

                DepthFirstWalk(tuple.Key, ref vertexCount);
            }

            return _reversePostOrder.ToDictionary(x => x.Key, x => x.Value.VisitOrder);
        }

        private void MarkAllVerticesUnvisited()
        {
            foreach (var cfgBlock in _graph.Vertices)
            {
                _reversePostOrder.Add(cfgBlock, new VisitedNumber());
            }
        }

        private void DepthFirstWalk(CFGBlock block, ref int counter)
        {
            _reversePostOrder[block].Visited = true;

            var successors = _graph.OutEdges(block)
                                   .OrderBy(e => orderMap[e.Tag.EdgeType]);
            foreach (var successor in successors.Select(e => e.Target))
            {
                if (!_reversePostOrder[successor].Visited)
                {
                    DepthFirstWalk(successor, ref counter);
                }
            }

            _reversePostOrder[block].VisitOrder = counter;
            counter = counter - 1;
        }

        private sealed class VisitedNumber
        {
            public bool Visited = false;
            public int VisitOrder = 0;
        }
    }
}