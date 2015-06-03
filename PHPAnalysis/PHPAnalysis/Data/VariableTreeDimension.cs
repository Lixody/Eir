using System;
using System.Globalization;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public sealed class VariableTreeDimension : IEquatable<VariableTreeDimension>, IDeepCloneable<VariableTreeDimension>
    {
        public int Index { get; set; }
        public string Key { get; set; }

        //public Variable VariableKey { get; set; }

        public VariableTreeDimension()
        {
            this.Index = -1;
        }

        #region Equality members 
        public bool Equals(VariableTreeDimension other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Index == other.Index && string.Equals(Key, other.Key);
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
            return obj is VariableTreeDimension && Equals((VariableTreeDimension) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Index * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(VariableTreeDimension left, VariableTreeDimension right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VariableTreeDimension left, VariableTreeDimension right)
        {
            return !Equals(left, right);
        }
        #endregion

        public VariableTreeDimension DeepClone()
        {
            return new VariableTreeDimension() {
                                                   Index = this.Index,
                                                   Key = this.Key
                                               };
        }

        public override string ToString()
        {
            return Key ?? Index.ToString(CultureInfo.InvariantCulture);
        }
    }
}