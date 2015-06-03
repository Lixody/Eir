using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PHPAnalysis.Utils
{
    public sealed class ImmutableDictionaryComparer<TKey, TValue> : IEqualityComparer<IImmutableDictionary<TKey, TValue>>
    {
        private readonly IEqualityComparer<TValue> valueComparer;
        public ImmutableDictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }
        public bool Equals(IImmutableDictionary<TKey, TValue> first, IImmutableDictionary<TKey, TValue> second)
        {
            if (first.Count != second.Count) { return false; }
            if (first.Keys.Except(second.Keys).Any()) { return false; }
            if (second.Keys.Except(first.Keys).Any()) { return false; }

            foreach (var pair in first)
            {
                if (!valueComparer.Equals(pair.Value, second[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(IImmutableDictionary<TKey, TValue> obj)
        {
            // Required by interface
            throw new NotImplementedException();
        }
    }
}