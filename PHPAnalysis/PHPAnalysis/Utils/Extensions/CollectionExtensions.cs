using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PHPAnalysis.Data;
using YamlDotNet.Core.Tokens;

namespace PHPAnalysis.Utils
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            Preconditions.NotNull(toAdd, "toAdd");
            if (collection.IsReadOnly) { throw new NotSupportedException(); }
            
            foreach (var element in toAdd)
            {
                collection.Add(element);
            }
        }

        public static void AddRange<T1, T2>(this IDictionary<T1, T2> collection, IEnumerable<KeyValuePair<T1, T2>> toAdd)
        {
            Preconditions.NotNull(collection, "collection");
            if (collection.IsReadOnly)
            {
                throw new NotSupportedException();
            }

            foreach (var keyValuePair in toAdd)
            {
                collection.Add(keyValuePair);
            }
        }

        public static bool DictionaryEquals<TKey, TValue>(
            this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
            IEqualityComparer<TValue> comparer = null)
        {
            if (ReferenceEquals(first, second)) { return true; }
            if (first == null || second == null) { return false; }
            if (first.Count != second.Count) { return false; }

            comparer = comparer ?? EqualityComparer<TValue>.Default;

            foreach (var keyValuePair in first)
            {
                TValue matchingValue;
                if (!second.TryGetValue(keyValuePair.Key, out matchingValue) ||
                    !comparer.Equals(keyValuePair.Value, matchingValue))
                {
                    return false;
                }
            }
            return true;
        }

        public static ImmutableStack<TValue> ToImmutableStack<TValue>(this Stack<TValue> mutableStack)
        {
            Preconditions.NotNull(mutableStack, "mutableStack");

            var result = ImmutableStack<TValue>.Empty;

            for (int i = mutableStack.Count - 1; i >= 0; i--)
            {
                result = result.Push(mutableStack.ElementAt(i));
            }
            return result; 
        }
    }
}
