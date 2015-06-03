using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class QueueWorklist : IWorklist<CFGBlock>
    {
        private readonly QueueSet<CFGBlock> queue = new QueueSet<CFGBlock>(); 
        private readonly StackSet<CFGBlock> queue1 = new StackSet<CFGBlock>();
         
        public bool Any()
        {
            return queue.Any();
        }

        public void Add(CFGBlock elem)
        {
            queue.Enqueue(elem);
        }

        public CFGBlock GetNext()
        {
            return queue.Dequeue();
        }

        public bool Contains(CFGBlock elem, IEqualityComparer<CFGBlock> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<CFGBlock>.Default;
            return queue.Contains(elem, comparer);
        }
    }
}