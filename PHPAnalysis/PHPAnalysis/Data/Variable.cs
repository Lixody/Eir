using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public class Variable : IMergeable<Variable>, IEquatable<Variable>
    {
        private Variable _unknown;

        public Variable Unknown
        {
            // Lazy-loaded to allow all variables to have a "default" (i.e. infinitely many nested arrays)
            // while avoiding stack overflow due to infinitely many Variable instantiations.
            get
            {
                return _unknown ?? (_unknown = new Variable()
                                               {
                                                   Scope = VariableScope.Instance,
                                                   Info =
                                                   {
                                                       Taints = Info.NestedVariableDefaultTaintFactory()
                                                   },
                                                   Name = "$UNKNOWN$"
                                               });
            }
        }
        public string Name { get; set; }
        public VariableScope Scope { get; set; }

        /// <summary>
        /// All information that is known about this variable.
        /// </summary>
        public ValueInfo Info { get; set; }

        private Variable()
        {
            this.Info = new ValueInfo();
        }
        public Variable(string name, VariableScope scope) : this()
        {
            this.Name = name;
            this.Scope = scope;
        }

        public Variable SanitizeVariable()
        {
            Info.Taints = new TaintSets().ClearTaint();
            
            return this;
        }
        public Variable XssTaintVariable(XSSTaintSet taintSet)
        {
            Info.Taints.XssTaint.Add(taintSet);
            
            return this;
        }
        public Variable SqliTaintVariable(SQLITaintSet taintSet)
        {
            Info.Taints.SqliTaint.Add(taintSet);

            return this;
        }
        public Variable SqlXssTaintVariable(XSSTaintSet xsstaintSet, SQLITaintSet sqliTaintSet)
        {
            XssTaintVariable(xsstaintSet);
            SqliTaintVariable(sqliTaintSet);

            return this;
        }

        public Variable Merge(Variable other)
        {
            if (this.Name != other.Name)
            {
                throw new InvalidOperationException("Trying to merge " + this.Name + " with " + other.Name + ". Merging of different variables is not supported! ");
            }
            var resultGenSet = this.AssignmentClone();
            resultGenSet.Info = resultGenSet.Info.Merge(other.Info);
            
            return resultGenSet;
        }

        public Variable AddVarInfo(ValueInfo info)
        {
            return new Variable(Name, this.Scope)
            {
            };
        }

        public bool Equals(Variable other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && Equals(Info, other.Info);
        }

        public Variable AssignmentClone()
        {
            var v = new Variable() {
                                      Info = this.Info.AssignmentClone(),
                                      Name = this.Name,
                                      Scope = this.Scope
                                  };
            // Only move unknown if it has been used (i.e. exist).
            if (_unknown != null) 
            {
                v._unknown = _unknown.AssignmentClone();
            }
            return v;
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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Variable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Info != null ? Info.GetHashCode() : 0);
            }
        }
    }

    public static class VariableListExtentions
    {
        public static IImmutableList<Variable> MergeVarList(this IEnumerable<Variable> first, 
                                                                 IEnumerable<Variable> second)
        {
            var newFirst = first.ToList();
            var other = second.ToList();

            List<Variable> mergedList = newFirst.Concat(other)
                                                .ToLookup(x => x.Name)
                                                .Select(z => z.Aggregate((l1, l2) => new Variable(l1.Name, l1.Scope) 
                                                                                     {
                                                                                         Info = l1.Info.Merge(l2.Info)
                                                                                     }))
                                                .ToList();
            
            return ImmutableList<Variable>.Empty.AddRange(mergedList);
        }
    }
}
