namespace PHPAnalysis.Data.CFG
{
    abstract class AbstractScope
    {
        public CFGBlock EntryBlock { get; set; }
        public CFGBlock EndBlock { get; set; }

        protected AbstractScope()
        {
        }
    }
}
