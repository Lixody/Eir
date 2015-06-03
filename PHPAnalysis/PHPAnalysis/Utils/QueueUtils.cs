using System.Collections.Generic;

namespace PHPAnalysis.Utils
{
    public static class QueueUtils
    {
        public static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> newElements)
        {
            Preconditions.NotNull(queue, "queue");
            Preconditions.NotNull(newElements, "newElements");

            foreach (var element in newElements)
            {
                queue.Enqueue(element);
            }
        }
    }
}