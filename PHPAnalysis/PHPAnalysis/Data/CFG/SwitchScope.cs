using System.Collections.Generic;
using System.Xml;

namespace PHPAnalysis.Data.CFG
{
    internal sealed class SwitchScope : AbstractScope
    {
        public CFGBlock SwitchStartNode
        {
            get { return EntryBlock; }
            private set { EntryBlock = value; }
        }

        public CFGBlock CurrentCondition { get; set; }
        public CFGBlock DefaultBlock { get; set; }
        public CFGBlock DefaultTrueBlock { get; set; }

        public SwitchScope(CFGBlock switchConditionNode, CFGBlock endNode)
        {
            EntryBlock = switchConditionNode;
            EndBlock = endNode;
        }
    }
}