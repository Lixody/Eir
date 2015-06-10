using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Utils.XmlHelpers;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Traversal;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Parsing;
using System.Collections.Immutable;
using PHPAnalysis.Utils;
using QuickGraph;

namespace PHPAnalysis.Analysis.AST
{
    public class FunctionAnalyzer : IFunctionAnalyzer
    {
    }

    public interface IFunctionAnalyzer
    {
    }

    public class CustomFunctionHandler
    {
        public ICollection<IBlockAnalyzerComponent> AnalysisExtensions { get; private set; } 


        private readonly Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> fileAnalyzer;
        private readonly FunctionAndMethodAnalyzerFactory subroutineAnalyzerFactory;
        public CustomFunctionHandler(Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> fileAnalyzer,
                                     FunctionAndMethodAnalyzerFactory subroutineAnalyzerFactory)
        {
            this.AnalysisExtensions = new List<IBlockAnalyzerComponent>();
            this.fileAnalyzer = fileAnalyzer;
            this.subroutineAnalyzerFactory = subroutineAnalyzerFactory;
        }
        /// <summary>
        /// Analyses a custom function in for security issues, with the currenctly known taint for actual parameters.
        /// </summary>
        /// <returns>A TainSets for the custom function that is being analyzed</returns>
        /// <param name="customFunction">Custom function object to perform the analysis on</param>
        /// <param name="varStorage">The currently known variable storage (this is to included because of superglobals, globals etc.)</param>
        /// <param name="paramActualVals">Parameter actual values</param>
        /// <param name="resolver">File inclusion resolver</param>
        /// <param name="includeStack">Currently known includes</param>
        /// <param name="functionCalls">Currently known function calls</param>
        internal ExpressionInfo AnalyseCustomFunction(Function customFunction, ImmutableVariableStorage varStorage, IVulnerabilityStorage vulnerabilityStorage,
            IList<ExpressionInfo> paramActualVals, IIncludeResolver resolver, AnalysisStacks stacks)
        {
            var stmts = customFunction.AstNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Stmts).FirstChild;

            var traverser = new XmlTraverser();
            var cfgcreator = new CFGCreator();
            traverser.AddVisitor(cfgcreator);
            traverser.Traverse(stmts);

            var cfgPruner = new CFGPruner();
            cfgPruner.Prune(cfgcreator.Graph);

            var initialTaint = varStorage.ToMutable();
            initialTaint.SuperGlobals.Clear();
            initialTaint.SuperGlobals.AddRange(varStorage.SuperGlobals);
            initialTaint.LocalVariables.Clear();
            initialTaint.LocalAccessibleGlobals.Clear();

            for(int i = 1; i <= paramActualVals.Count; i++)
            {
                var paramFormal = customFunction.Parameters.FirstOrDefault(x => x.Key.Item1 == i);
                if (paramFormal.Value == null)
                {
                    continue;
                }
                var @var = new Variable(paramFormal.Value.Name, VariableScope.Function) {Info = paramActualVals[i - 1].ValueInfo};
                initialTaint.LocalVariables.Add(paramFormal.Value.Name, @var);
            }

            var blockAnalyzer = new TaintBlockAnalyzer(vulnerabilityStorage, resolver, AnalysisScope.Function, fileAnalyzer, stacks, subroutineAnalyzerFactory);
            blockAnalyzer.AnalysisExtensions.AddRange(AnalysisExtensions);
            var condAnalyser = new ConditionTaintAnalyser(AnalysisScope.Function, resolver, stacks.IncludeStack);
            var cfgTaintAnalysis = new TaintAnalysis(blockAnalyzer, condAnalyser, ImmutableVariableStorage.CreateFromMutable(initialTaint));
            //var taintAnalysis = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new QueueWorklist());
            var taintAnalysis = new CFGTraverser(new ForwardTraversal(), cfgTaintAnalysis, new ReversePostOrderWorkList(cfgcreator.Graph));
            taintAnalysis.Analyze(cfgcreator.Graph);

            var exprInfoAll = new ExpressionInfo();

            foreach (ExpressionInfo exprInfo in blockAnalyzer.ReturnInfos)
            {
                exprInfoAll = exprInfoAll.Merge(exprInfo);
            }

            return exprInfoAll;
        }
    }
}