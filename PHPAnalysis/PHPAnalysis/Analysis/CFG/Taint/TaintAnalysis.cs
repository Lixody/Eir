using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class TaintAnalysis : ICFGAnalysis
    {
        private readonly TaintBlockAnalyzer _blockAnalyzer;
        private readonly ConditionTaintAnalyser _conditionTaintAnalyser;
        private readonly Dictionary<CFGBlock, CFGTaintInfo> _taints;

        public IImmutableDictionary<CFGBlock, CFGTaintInfo> Taints
        {
            get { return ImmutableDictionary<CFGBlock, CFGTaintInfo>.Empty.AddRange(this._taints); }
        }

        private readonly ImmutableVariableStorage initialTaint;

        public TaintAnalysis(TaintBlockAnalyzer blockAnalyzer, ConditionTaintAnalyser condAnalyser, ImmutableVariableStorage initialTaint)
        {
            Preconditions.NotNull(blockAnalyzer, "blockAnalyzer");
            Preconditions.NotNull(initialTaint, "initialTaint");
            Preconditions.NotNull(condAnalyser, "condAnalyzer");

            this._blockAnalyzer = blockAnalyzer;
            this._conditionTaintAnalyser = condAnalyser;
            this.initialTaint = initialTaint;
            this._taints = new Dictionary<CFGBlock, CFGTaintInfo>();
        }

        public void Initialize(CFGBlock cfgBlock)
        {
            var taintInfo = CFGTaintInfo.Default;
            if (cfgBlock.IsRoot)
            {
                var varStorage = ImmutableDictionary<EdgeType, ImmutableVariableStorage>.Empty.Add(EdgeType.Normal, initialTaint);
                taintInfo = new CFGTaintInfo(initialTaint, varStorage);
            }
            _taints.Add(cfgBlock, taintInfo);
        }

        public bool Analyze(TaggedEdge<CFGBlock, EdgeTag> edge)
        {
            var oldTaint = Taints[edge.Target];

            var newTaint = AnalyzeNode(edge);

            if (MonotonicChange(oldTaint, newTaint))
            {
                _taints[edge.Target] = newTaint;
                return true;
            }
            return false;
        }

        public bool Analyze2(CFGBlock block, IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            var oldTaint = Taints[block];

            var newTaint = AnalyzeNode2(block, graph);

            if (MonotonicChange(oldTaint, newTaint))
            {
                _taints[block] = newTaint;
                return true;
            }
            return false; 
        }

        private CFGTaintInfo AnalyzeNode2(CFGBlock block, IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            var oldTaint = Taints[block];


            var predecessorsOut = graph.InEdges(block);
            var outTaints = predecessorsOut.Select(p => new { EdgeType = p.Tag, Source = p.Source })
                                           .Where(s => Taints[s.Source].Out != null && Taints[s.Source].Out.Any())
                                           .Select(s => Taints[s.Source].Out[s.EdgeType.EdgeType])
                                           .Where(o => o != null);
            ImmutableVariableStorage newInTaint;
            if (outTaints.Any())
            {
                newInTaint = oldTaint.In.Merge(outTaints.Aggregate((current, next) => current.Merge(next)));
            }
            else
            {
                newInTaint = oldTaint.In;
            }

            


            ImmutableDictionary<EdgeType, ImmutableVariableStorage> newOutTaint;

            if (block.AstEntryNode == null)
            {
                newOutTaint = ImmutableDictionary<EdgeType, ImmutableVariableStorage>.Empty.Add(EdgeType.Normal, newInTaint);
            }
            else
            {
                var newOut = _blockAnalyzer.Analyze(block.AstEntryNode, newInTaint);
                var newOutWithCondSani = _conditionTaintAnalyser.AnalyzeCond(block.AstEntryNode, newOut);

                newOutTaint = newOutWithCondSani.ToImmutableDictionary();
            }

            return new CFGTaintInfo(newInTaint, newOutTaint);
        }

        private CFGTaintInfo AnalyzeNode(TaggedEdge<CFGBlock, EdgeTag> edge)
        {
            var oldTaint = Taints[edge.Target];

            // IN:
            //    ∪   TAINT_OUT( l')
            // (l'~>l)
            var newInTaint = oldTaint.In.Merge(Taints[edge.Source].Out[edge.Tag.EdgeType]);

            // OUT:
            // ( TAINT_IN(l) \ KILL(l) ) ∪ GEN(l)
            ImmutableDictionary<EdgeType, ImmutableVariableStorage> newOutTaint;
            

            if (edge.Target.AstEntryNode == null)
            {
                newOutTaint = ImmutableDictionary<EdgeType, ImmutableVariableStorage>.Empty.Add(EdgeType.Normal, newInTaint);
            }
            else
            {
                //var blockTaintAnalyzer = new TaintBlockVisitor(null, newInTaint);
                //var astTraverser = new XmlTraverser();
                //astTraverser.AddVisitor(blockTaintAnalyzer);
                //astTraverser.Traverse(edge.Target.AstEntryNode);

                var newOut =_blockAnalyzer.Analyze(edge.Target.AstEntryNode, newInTaint);
                var newOutWithCondSani = _conditionTaintAnalyser.AnalyzeCond(edge.Target.AstEntryNode, newOut);

                newOutTaint = newOutWithCondSani.ToImmutableDictionary();
            }
            
            return new CFGTaintInfo(newInTaint, newOutTaint);
        }

        private bool MonotonicChange(CFGTaintInfo oldResult, CFGTaintInfo newResult)
        {
            if (oldResult == null) { return true; }

            return !oldResult.Equals(newResult);
        }
    }
}
