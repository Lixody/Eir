using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Analysis.CFG
{
    public static class VariableInfoComposer
    {
        public static readonly Dictionary<CFGBlock, ValueInfo> VarInfoStorage = new Dictionary<CFGBlock, ValueInfo>();

        public static ValueInfo AnalyzeBlock(ValueInfo block)
        {
            if (block.Block.AstEntryNode == null)
                return null;

            if (VarInfoStorage.ContainsKey(block.Block))
            {
                return VarInfoStorage[block.Block];
            }

            // type, value, arraytree
            var astNode = block.Block.AstEntryNode;

            VarInfoStorage.Add(block.Block, block);
            return block;
        }
    }
}
