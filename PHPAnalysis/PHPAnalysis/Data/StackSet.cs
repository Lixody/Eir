using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public sealed class StackSet<T> : ICollection, IReadOnlyCollection<T>
    {
        private readonly Stack<T> _stack;
        private readonly HashSet<T> _set;

        public StackSet()
        {
            this._stack = new Stack<T>();
            this._set = new HashSet<T>();
        }

        public void Enqueue(T item)
        {
            if (!_set.Contains(item))
            {
                _stack.Push(item);
                _set.Add(item);
            }
        }

        public T Dequeue()
        {
            if (_stack.IsEmpty())
            {
                throw new InvalidOperationException("Stack is empty");
            }

            _set.Remove(_stack.Peek());
            return _stack.Pop();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection) _stack).CopyTo(array, index);
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection) _stack).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection) _stack).IsSynchronized; }
        }
    }
}