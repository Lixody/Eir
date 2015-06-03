using System.Collections.Generic;
using System.Linq;

namespace PHPAnalysis.Utils
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }
    }
}