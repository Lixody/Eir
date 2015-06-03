using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public sealed class TaintSets : IMergeable<TaintSets>, IDeepCloneable<TaintSets>, IEquatable<TaintSets>
    {
        public List<XSSTaintSet> XssTaint { get; private set; }
        public List<SQLITaintSet> SqliTaint { get; private set; }

        public TaintSets()
        {
            SqliTaint = new List<SQLITaintSet>();
            XssTaint = new List<XSSTaintSet>();
        }

        public TaintSets(SQLITaintSet sqliTaint, XSSTaintSet xssTaint) : this()
        {
            Preconditions.NotNull(sqliTaint, "sqliTaint");
            Preconditions.NotNull(xssTaint, "xssTaint");

            this.XssTaint.Add(xssTaint);
            this.SqliTaint.Add(sqliTaint);
        }

        public TaintSets ClearTaint()
        {
            XssTaint = new List<XSSTaintSet>() { new XSSTaintSet() };
            SqliTaint = new List<SQLITaintSet>() { new SQLITaintSet() };
            return this;
        }

        public TaintSets Merge(TaintSets other)
        {
            Preconditions.NotNull(other, "other");

            var result = new TaintSets();

            var resultSqliTaint = new SQLITaintSet();
            if (this.SqliTaint.Any())
            {
                var leftSqliTaint = this.SqliTaint.Aggregate((curr, next) => curr.Merge(next));
                resultSqliTaint = resultSqliTaint.Merge(leftSqliTaint);
            }
            if (other.SqliTaint.Any())
            {
                var rightSqliTaint = other.SqliTaint.Aggregate((curr, next) => curr.Merge(next));
                resultSqliTaint = resultSqliTaint.Merge(rightSqliTaint);
            }

            XSSTaintSet resultXssTaintSet = new XSSTaintSet();
            if (this.XssTaint.Any())
            {
                resultXssTaintSet = resultXssTaintSet.Merge(this.XssTaint.Aggregate((curr, next) => curr.Merge(next)));
            }
            if (other.XssTaint.Any())
            {
                var rightXssTaint = other.XssTaint.Aggregate((curr, next) => curr.Merge(next));
                resultXssTaintSet = resultXssTaintSet.Merge(rightXssTaint);
            }

            result.SqliTaint.Add(resultSqliTaint);
            result.XssTaint.Add(resultXssTaintSet);

            return result;
        }

        public TaintSets DeepClone()
        {
            return new TaintSets() {
                                       SqliTaint = this.SqliTaint.Select(s => s.DeepClone()).ToList(),
                                       XssTaint = this.XssTaint.Select(x => x.DeepClone()).ToList()
                                   };
        }

        public bool Equals(TaintSets other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return XssTaint.SequenceEqual(other.XssTaint) && SqliTaint.SequenceEqual(other.SqliTaint);
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
            return Equals((TaintSets) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SqliTaint != null ? SqliTaint.GetHashCode() : 0) * 397) ^ (XssTaint != null ? XssTaint.GetHashCode() : 0);
            }
        }
    }
}
