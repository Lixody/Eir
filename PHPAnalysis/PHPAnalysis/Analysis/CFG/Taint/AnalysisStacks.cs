using System.Collections.Generic;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public sealed class AnalysisStacks
    {
        /// <summary>
        /// This stack should always contain the file currently being analyzed.
        /// </summary>
        public Stack<File> IncludeStack { get; private set; }
        public Stack<FunctionCall> CallStack { get; private set; }

        private AnalysisStacks()
        {
            IncludeStack = new Stack<File>();
            CallStack = new Stack<FunctionCall>();
        }

        public AnalysisStacks(File initialFile) : this()
        {
            Preconditions.NotNull(initialFile, "initialFile");

            IncludeStack.Push(initialFile);
        }

        public AnalysisStacks(Stack<File> initialIncludeStack) : this()
        {
            IncludeStack = initialIncludeStack;
        }
    }
}