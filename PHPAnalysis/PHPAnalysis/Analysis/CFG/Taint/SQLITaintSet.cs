using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public class SQLITaintSet : VariableTaint, IEquatable<SQLITaintSet>, IMergeable<SQLITaintSet>, IDeepCloneable<SQLITaintSet>
    {
        private static readonly SQLITaintSet _noTaint = new SQLITaintSet();
        public string InitialTaintedVariable { get; set; }

        public override VariableTaint NoTaint
        {
            get { return _noTaint; }
        }

        public override VariableTaint Merge(VariableTaint other)
        {
            SQLITaintSet newSet = (SQLITaintSet)other;
            return Merge(newSet);
        }

        public SQLITaint TaintTag { get; private set; }

        public SQLITaintSet(SQLITaint initialTaint = SQLITaint.None)
        {
            this.TaintTag = initialTaint;
        }

        public SQLITaintSet AddTaint(SQLITaint taint, Variable initTaintedVar = null)
        {
            return new SQLITaintSet()
                      {
                          InitialTaintedVariable = initTaintedVar.Name,
                          TaintTag = TaintTag | taint
                      };
        }

        public void SetInitialTaintVar(Variable var)
        {
            InitialTaintedVariable = var.Name;
        }

        public SQLITaintSet Merge(SQLITaintSet other)
        {
            Preconditions.NotNull(other, "set");
            string newInitialVar = "";
            if (this.TaintTag != SQLITaint.None && string.IsNullOrWhiteSpace(this.InitialTaintedVariable))
                newInitialVar = this.InitialTaintedVariable;
            if (other.TaintTag != SQLITaint.None && string.IsNullOrWhiteSpace(this.InitialTaintedVariable))
            {
                if (string.IsNullOrWhiteSpace(newInitialVar))
                {
                    newInitialVar = other.InitialTaintedVariable;
                }
                else if (newInitialVar != other.InitialTaintedVariable)
                {
                    newInitialVar = newInitialVar + " + " + other.InitialTaintedVariable;
                }
            }
            return new SQLITaintSet()
                   {
                       TaintTag = this.TaintTag | other.TaintTag,
                       InitialTaintedVariable = newInitialVar
                   };
        }

        public SQLITaintSet DeepClone()
        {
            return new SQLITaintSet() {
                                          InitialTaintedVariable = this.InitialTaintedVariable,
                                          TaintTag = this.TaintTag
                                      };
        }

        public bool Equals(SQLITaintSet other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(TaintTag, other.TaintTag);
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
            return Equals((SQLITaintSet)obj);
        }

        public override int GetHashCode()
        {
            return (TaintTag != null ? TaintTag.GetHashCode() : 0);
        }
    }
}