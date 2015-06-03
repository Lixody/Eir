using System;
using System.Collections;
using System.Collections.Generic;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public sealed class QueueSet<T> : ICollection, IReadOnlyCollection<T>
    {
        private readonly Queue<T> _queue;
        private readonly HashSet<T> _set; 

        public QueueSet()
        {
            this._queue = new Queue<T>();
            this._set = new HashSet<T>();
        }

        /// <summary>
        /// If item already exist it will not be inserted. 
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if (!_set.Contains(item))
            {
                _queue.Enqueue(item);
                _set.Add(item);
            }
        }

        public T Dequeue()
        {
            if (_queue.IsEmpty())
            {
                throw new InvalidOperationException("Queue is empty");
            }
            _set.Remove(_queue.Peek());
            return _queue.Dequeue();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void Clear()
        {
            _set.Clear();
            _queue.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) _queue).CopyTo(array, index);
        }

        public void CopyTo(T[] array, int index)
        {
            _queue.CopyTo(array, index);
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)_queue).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_queue).IsSynchronized; }
        }
    }
}