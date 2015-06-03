using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.CFG
{
    /// <summary>
    /// Manages the different scopes (loop, if, switch, ..) were currently in, when traversing the CFG. 
    /// </summary>
    internal sealed class ScopeHandler
    {
        private readonly Stack<AbstractScope> allScopes = new Stack<AbstractScope>();
        private readonly Stack<IfScope> ifScopes = new Stack<IfScope>();
        /// <summary>
        /// Contains loopscopes. The stack can contain both actual loop (while, for, ..) and switch cases. 
        /// Switch cases are regarded as a special loop, since both break and continue works with them.
        /// </summary>
        private readonly Stack<AbstractScope> loopScopes = new Stack<AbstractScope>();
        public AbstractScope CurrentScope { get { return allScopes.Peek(); } }
        public bool IsInLoop
        {
            get
            {
                return loopScopes.Any();
            }
        }

        public bool IsInnermostScopeALoop
        {
            get
            {
                if (loopScopes.IsEmpty())
                {
                    return false;
                }
                return allScopes.Peek() == loopScopes.Peek();
            }
        }

        public void PushIfStmt(IfScope ifblock)
        {
            this.ifScopes.Push(ifblock);
            this.allScopes.Push(ifblock);
        }
        public IfScope PopIfStmt()
        {
            this.allScopes.Pop();
            return this.ifScopes.Pop();
        }
        public IfScope GetIfStmt()
        {
            return this.ifScopes.Peek();
        }

        public void EnterLoop(AbstractScope loopBlock)
        {
            this.loopScopes.Push(loopBlock);
            this.allScopes.Push(loopBlock);
        }
        public AbstractScope LeaveLoop()
        {
            this.allScopes.Pop();
            return this.loopScopes.Pop();
        }
        public AbstractScope GetInnermostLoop()
        {
            return this.loopScopes.Peek();
        }

        public AbstractScope GetLoopScope(int scopesToSkip)
        {
            return loopScopes.ElementAt(scopesToSkip);
        }
    }
}
