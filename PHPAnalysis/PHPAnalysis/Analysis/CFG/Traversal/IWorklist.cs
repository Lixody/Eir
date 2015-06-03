using System.Collections.Generic;

namespace PHPAnalysis.Analysis.CFG.Traversal
{
    public interface IWorklist<T>
    {
        bool Any();

        void Add(T elem);

        T GetNext();

        bool Contains(T elem, IEqualityComparer<T> comparer = null);
    }
}