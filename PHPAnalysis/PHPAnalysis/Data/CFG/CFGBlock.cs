using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Xml;

namespace PHPAnalysis.Data.CFG
{
    public sealed class CFGBlock
    {
        public bool IsRoot { get; set; }
        public bool IsLeaf { get; set; }
        public XmlNode AstEntryNode { get; set; }
        public bool IsSpecialBlock { get; private set; }
        /// <summary>
        /// Specify whether this node breaks out of the existing scope before the execution
        /// reaches the end of the block. E.g. by reaching a break or continue statement.
        /// </summary>
        public bool BreaksOutOfScope { get; set; }

        public CFGBlock(bool isSpecial = false)
        {
            this.IsSpecialBlock = isSpecial;
        }

        public bool CanBeOverridden 
        {
            get { return !IsSpecialBlock && AstEntryNode == null; }
        }

        public override string ToString()
        {
            return AstEntryNode == null ? base.ToString() : AstEntryNode.LocalName;
        }
    }
}
