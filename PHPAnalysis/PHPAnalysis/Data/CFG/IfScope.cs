namespace PHPAnalysis.Data.CFG
{
    internal sealed class IfScope : AbstractScope
    {
        public CFGBlock IfConditionNode
        {
            get { return EntryBlock; }
            private set { EntryBlock = value; }
        }
        public CFGBlock TrueNode { get; set; }
        public CFGBlock FalseNode { get; set; }
        public CFGBlock ElseifBlock { get; set; }

        public IfScope(CFGBlock ifConditionNode, CFGBlock trueNode = null)
        {
            EntryBlock = ifConditionNode;
            this.TrueNode = trueNode;
        }

        public bool IsFalseNodeSet()
        {
            return FalseNode != null;
        }

        public bool IsTrueNodeSet()
        {
            return TrueNode != null;
        }
    }
}
