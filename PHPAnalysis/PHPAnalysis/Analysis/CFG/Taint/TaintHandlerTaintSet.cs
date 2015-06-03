using System.Collections.Generic;
using System.Collections.Immutable;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class TaintHandlerTaintSet : IMergeable<TaintHandlerTaintSet>
    {
        public IImmutableDictionary<Variable, VariableTaint> Taint { get; private set; }

        public TaintHandlerTaintSet(Dictionary<Variable, VariableTaint> variableTaints)
        {
            Preconditions.NotNull(variableTaints, "variableTaints");

            Taint = ImmutableDictionary<Variable, VariableTaint>.Empty;
            Taint = Taint.AddRange(variableTaints);
        }
        public TaintHandlerTaintSet(IImmutableDictionary<Variable, VariableTaint> variableTaint)
        {
            Preconditions.NotNull(variableTaint, "variableTaint");
            Taint = variableTaint;
        }

        public void AddTaintedVar(Variable var, VariableTaint varTaint)
        {
            Taint = Taint.SetItem(var, varTaint);
        }

        public TaintHandlerTaintSet Merge(TaintHandlerTaintSet other)
        {
            var taintDict = new Dictionary<Variable, VariableTaint>();
            foreach (KeyValuePair<Variable, VariableTaint> variableTaint in Taint)
            {
                var mergedTaint = variableTaint.Value.Merge(other.Taint[variableTaint.Key]);
                taintDict.Add(variableTaint.Key, mergedTaint);
            }

            return new TaintHandlerTaintSet(taintDict);
        }
    }
}