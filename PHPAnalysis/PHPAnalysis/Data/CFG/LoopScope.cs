using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.CFG
{
    internal sealed class LoopScope : AbstractScope
    {
        public CFGBlock LoopConditionBlock { get; set; }
        public CFGBlock LoopBodyStartBlock { get; set; }
        public CFGBlock LoopUpdateBlock { get; set; }
        public CFGBlock ContinueDestination { get; set; }

        public LoopScope(CFGBlock entryBlock)
        {
            Preconditions.NotNull(entryBlock, "entryBlock");

            this.EntryBlock = entryBlock;
        }
    }
}
