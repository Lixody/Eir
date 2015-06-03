using System;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public class XSSTaintSet : VariableTaint, IEquatable<XSSTaintSet>, IMergeable<XSSTaintSet>, IDeepCloneable<XSSTaintSet>
    {
        private static readonly XSSTaintSet _noTaint = new XSSTaintSet();
        public string InitialTaintedVariable { get; set; }

        public override VariableTaint NoTaint
        {
            get { return _noTaint; }
        }

        public override VariableTaint Merge(VariableTaint other)
        {
            XSSTaintSet newSet = (XSSTaintSet) other;
            return Merge(newSet);
        }

        public XSSTaint TaintTag { get; private set; }
  
        public XSSTaintSet(XSSTaint taintStatus = XSSTaint.None)
        {
            this.TaintTag = taintStatus;
        }

        public XSSTaintSet AddTaint(XSSTaint taint)
        {
            return new XSSTaintSet()
                   {
                       TaintTag = TaintTag | taint
                   };
        }

        public void SetInitialTaintVar(Variable var)
        {
            InitialTaintedVariable = var.Name;
        }

        public XSSTaintSet Merge(XSSTaintSet other)
        {
            Preconditions.NotNull(other, "set");
            string newInitialVar = "";
            if (this.TaintTag != XSSTaint.None && string.IsNullOrWhiteSpace(this.InitialTaintedVariable))
            {
                newInitialVar = this.InitialTaintedVariable;
            }

            if (other.TaintTag != XSSTaint.None && string.IsNullOrWhiteSpace(this.InitialTaintedVariable))
            {
                if (string.IsNullOrWhiteSpace(newInitialVar))
                {
                    newInitialVar = other.InitialTaintedVariable;
                }
                else if(newInitialVar != other.InitialTaintedVariable)
                {
                    newInitialVar = newInitialVar + " + " + other.InitialTaintedVariable;
                }
            }

            return new XSSTaintSet()
                   {
                       TaintTag = this.TaintTag | other.TaintTag,
                       InitialTaintedVariable = newInitialVar
                   };
        }

        public bool Equals(XSSTaintSet other)
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

        public XSSTaintSet DeepClone()
        {
            return new XSSTaintSet() {
                InitialTaintedVariable = this.InitialTaintedVariable,
                TaintTag = this.TaintTag
            };
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
            return Equals((XSSTaintSet)obj);
        }

        public override int GetHashCode()
        {
            return (TaintTag != null ? TaintTag.GetHashCode() : 0);
        }
    }
}