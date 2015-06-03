using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public class CFGTaintInfo : IEquatable<CFGTaintInfo>, IMergeable<CFGTaintInfo>
    {
        public static readonly CFGTaintInfo Default = new CFGTaintInfo();

        public ImmutableVariableStorage In { get; private set; }
        public ImmutableDictionary<EdgeType, ImmutableVariableStorage> Out { get; private set; }

        private CFGTaintInfo()
        {
            In = ImmutableVariableStorage.Empty;
            Out = ImmutableDictionary<EdgeType, ImmutableVariableStorage>.Empty;
        }
        public CFGTaintInfo(IVariableStorage varTaintIn,
                            IImmutableDictionary<EdgeType, IVariableStorage> varTaintOut)
        {
            In = ImmutableVariableStorage.CreateFromMutable(varTaintIn);
            foreach (var variableStorage in varTaintOut)
            {
                Out = Out.Add(variableStorage.Key, ImmutableVariableStorage.CreateFromMutable(variableStorage.Value));
            }
        }
        public CFGTaintInfo(ImmutableVariableStorage varTaintIn,
                            IImmutableDictionary<EdgeType, ImmutableVariableStorage> varTaintOut)
        {
            In = varTaintIn;
            Out = varTaintOut.ToImmutableDictionary();
        }

        public CFGTaintInfo Merge(CFGTaintInfo other)
        {
            var mergedIn = this.In.Merge(other.In);
            var mergedOut = this.Out.Merge(other.Out);

            //foreach (var variable in In)
            //{
            //    var thisIn = variable;
            //    var otherIn = other.In.First(x => x.Name == variable.Name);
            //
            //    mergedIn.Add(thisIn.Merge(otherIn));
            //}
            //
            //foreach (var variable in Out)
            //{
            //    var thisOut = variable;
            //    var otherOut = other.Out.First(x => x.Name == variable.Name);
            //
            //    mergedOut.Add(thisOut.Merge(otherOut));
            //}

            return new CFGTaintInfo(mergedIn, mergedOut);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CFGTaintInfo);
        }
        public bool Equals(CFGTaintInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            // todo: Check taint set too
            var comparer = new ImmutableDictionaryComparer<EdgeType, ImmutableVariableStorage>();
            return In.Equals(other.In) && comparer.Equals(Out, other.Out);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((In != null ? In.GetHashCode() : 0) * 397) ^ (Out != null ? Out.GetHashCode() : 0);
            }
        }
    }
}