using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PHPAnalysis.Utils
{
    public static class IImmutableExtentions
    {
        public static IImmutableSet<T> AddRange<T>(this IImmutableSet<T> collection, IEnumerable<T> toAdd)
        {
            Preconditions.NotNull(toAdd, "toAdd");

            return toAdd.Aggregate(collection, (current, element) => current.Add(element));
        }

        public static System.Collections.Immutable.IImmutableDictionary<T1, T2> Merge<T1, T2>(this System.Collections.Immutable.IImmutableDictionary<T1, T2> first, System.Collections.Immutable.IImmutableDictionary<T1, T2> second)
            where T2 : IMergeable<T2>
        {
            var newDict = first.ToDictionary(x => x.Key, x => x.Value);
            foreach (var other in second)
            {
                T2 set;
                if (newDict.TryGetValue(other.Key, out set))
                {
                    set = set.Merge(other.Value);
                    newDict[other.Key] = set;
                }
                else
                {
                    newDict.Add(other.Key, other.Value);
                }
            }
            return ImmutableDictionary<T1, T2>.Empty.AddRange(newDict);
        }
    }
}
