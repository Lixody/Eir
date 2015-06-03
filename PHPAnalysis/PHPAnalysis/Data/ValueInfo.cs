using System;
using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public class ValueInfo : IEquatable<ValueInfo>, IMergeable<ValueInfo>
    {
        public CFGBlock Block { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public TaintSets Taints { get; set; }
        public StoredVulnInfo PossibleStoredTaint { get; set; }
        public Func<TaintSets> NestedVariableDefaultTaintFactory { get; set; }
        public Func<TaintSets> NestedVariablePossibleStoredDefaultTaintFactory { get; set; }
        public Func<TaintSets> DefaultDimensionTaintFactory { get; set; }
        public IList<string> ClassNames { get; private set; }

        /// <summary>
        /// Represents nested variables (i.e. in arrays or classes). 
        /// </summary>
        public readonly Dictionary<VariableTreeDimension, Variable> Variables;

        public ValueInfo()
        {
            Taints = new TaintSets().ClearTaint();
            Variables = new Dictionary<VariableTreeDimension, Variable>();
            NestedVariableDefaultTaintFactory = () => new TaintSets();
            NestedVariablePossibleStoredDefaultTaintFactory = () => new TaintSets();
            DefaultDimensionTaintFactory = () => new TaintSets();
            PossibleStoredTaint = new StoredVulnInfo();
            ClassNames = new List<string>();
        }

        public bool TryGetVariableByString(string key, out Variable variable)
        {
            variable = null;
            var matchingKey = Variables.Where(v => v.Key.Key == key);
            if (matchingKey.Any())
            {
                variable = matchingKey.Single().Value;
                return true;
            }
            return false;
        }

        public bool TryGetVariableByIndex(int index, out Variable variable)
        {
            variable = default(Variable);
            var matchingKey = Variables.Where(v => v.Key.Index == index);
            if (matchingKey.Any())
            {
                variable = matchingKey.Single().Value;
                return true;
            }
            return false;
        }

        public bool TryGetVariableByVariable(Variable var, out Variable variable)
        {
            throw new NotImplementedException("We currently do not support retrieving nested variables using other variables..");
        }

        /// <summary>
        /// This will create a clone of this instance. 
        /// All properties, EXCEPT Block, will be recursively cloned.
        /// </summary>
        public ValueInfo AssignmentClone()
        {
            var variableInfo = new ValueInfo() 
                               {
                                   Block = this.Block,
                                   NestedVariableDefaultTaintFactory = this.NestedVariableDefaultTaintFactory,
                                   NestedVariablePossibleStoredDefaultTaintFactory = this.NestedVariablePossibleStoredDefaultTaintFactory,
                                   DefaultDimensionTaintFactory = this.DefaultDimensionTaintFactory,
                                   Type = this.Type,
                                   Value = this.Value,
                                   ClassNames = this.ClassNames.ToList()
                               };

            if (this.PossibleStoredTaint != null)
            {
                variableInfo.PossibleStoredTaint = variableInfo.PossibleStoredTaint.Merge(this.PossibleStoredTaint);
            }
            variableInfo.Taints = variableInfo.Taints.Merge(this.Taints);

            var dictionary = new Dictionary<VariableTreeDimension, Variable>(this.Variables.ToDictionary(x => x.Key.DeepClone(), x => x.Value.AssignmentClone()));
            variableInfo.Variables.AddRange(dictionary);
            return variableInfo;
        }

        #region Equals override
        public bool Equals(ValueInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Block, other.Block) && Equals(Taints, other.Taints) 
                   && Equals(PossibleStoredTaint, other.PossibleStoredTaint) 
                   && this.Variables.DictionaryEquals(other.Variables);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Block != null ? Block.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == this.GetType() && Equals((ValueInfo) obj);
        }
        #endregion

        public ValueInfo Merge(ValueInfo other)
        {
            var varInfoResult = this.AssignmentClone();
            varInfoResult.Taints = this.Taints.Merge(other.Taints);
            varInfoResult.PossibleStoredTaint = this.PossibleStoredTaint.Merge(other.PossibleStoredTaint);

            var newVarTaintFactory = this.NestedVariableDefaultTaintFactory().Merge(other.NestedVariableDefaultTaintFactory());
            varInfoResult.NestedVariableDefaultTaintFactory = () => newVarTaintFactory;

            var newVarPossibleStoredTaintFactory = this.NestedVariablePossibleStoredDefaultTaintFactory().Merge(other.NestedVariablePossibleStoredDefaultTaintFactory());
            varInfoResult.NestedVariablePossibleStoredDefaultTaintFactory = () => newVarPossibleStoredTaintFactory;

            var distingClassNames = other.ClassNames.ToList();
            distingClassNames.AddRange(this.ClassNames);
            varInfoResult.ClassNames = distingClassNames.Distinct().ToList();
            return varInfoResult;
        }
    }
}
