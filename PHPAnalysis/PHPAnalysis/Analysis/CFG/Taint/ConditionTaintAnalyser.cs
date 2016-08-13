using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using PHPAnalysis.Analysis.AST;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public sealed class ConditionTaintAnalyser
    {
        private readonly Dictionary<EdgeType, IVariableStorage> _variables = new Dictionary<EdgeType, IVariableStorage>();
        private readonly IIncludeResolver includeResolver; 
        private VariableResolver _varResolver;
        private bool isConditional = false;
        private bool isNegated = false;
        private readonly AnalysisScope _analysisScope;
        private readonly Stack<File> _includeStack;
        private readonly FunctionsHandler _funcHandler;

        public ConditionTaintAnalyser(AnalysisScope scope, IIncludeResolver inclusionResolver, Stack<File> includeStack, FunctionsHandler fh)
        {
            this._analysisScope = scope;
            this.includeResolver = inclusionResolver;
            this._includeStack = includeStack;
            this._funcHandler = fh;
        }

        public IImmutableDictionary<EdgeType, ImmutableVariableStorage> AnalyzeCond(XmlNode node, ImmutableVariableStorage knownTaint)
        {
            Preconditions.NotNull(knownTaint, "knownTaint");

            _variables.Remove(EdgeType.Normal);
            _variables.Add(EdgeType.Normal, knownTaint.ToMutable());
            _varResolver = new VariableResolver(_variables[EdgeType.Normal], _analysisScope);

            _variables.Remove(EdgeType.False);
            _variables.Remove(EdgeType.True);

            Analyze(node);
            isConditional = false;

            var variablestore = _variables.Keys.ToDictionary(edgetype => edgetype, edgetype => ImmutableVariableStorage.CreateFromMutable(_variables[edgetype]));
            return ImmutableDictionary<EdgeType, ImmutableVariableStorage>.Empty.AddRange(variablestore);
        }


        private TaintSets Analyze(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            switch (node.Prefix)
            {
                case AstConstants.Node:
                    return AnalyzeNode(node);
                case AstConstants.Subnode:
                    return AnalyzeSubnode(node);
                default:
                    return null;
            }
        }
        private TaintSets AnalyzeNode(XmlNode node)
        {
            switch (node.LocalName)
            {
                case AstConstants.Nodes.Expr_BooleanNot:
                    return Expr_BooleanNot(node);
                case AstConstants.Nodes.Expr_FuncCall:
                    return Node_FuncCall(node);

                case AstConstants.Nodes.Stmt_Case:
                case AstConstants.Nodes.Stmt_Do:
                case AstConstants.Nodes.Stmt_ElseIf:
                case AstConstants.Nodes.Stmt_For:
                case AstConstants.Nodes.Stmt_If:
                case AstConstants.Nodes.Stmt_While:
                    return Analyze(Conditional.GetCondNode(node));
                //case AstConstants.Nodes.Stmt_Switch:
                // Conditional sanitization is currently not supported with switches. 
                // Since they require special handling (the condition is a mix of the switch node as well as the case nodes).
                case AstConstants.Nodes.Expr_BinaryOp_Equal:
                case AstConstants.Nodes.Expr_BinaryOp_Identical:
                    return EqualsComparison(node);
                case AstConstants.Nodes.Expr_BinaryOp_NotEqual:
                case AstConstants.Nodes.Expr_BinaryOp_NotIdentical:
                    isNegated = !isNegated;
                    return EqualsComparison(node);

                default:
                    return new TaintSets().ClearTaint();
            }
        }
        private TaintSets AnalyzeSubnode(XmlNode node)
        {
            switch (node.LocalName)
            {
                case AstConstants.Subnodes.Cond:
                    return Node_Cond(node);
                default:
                    var subNodes = node.GetSubNodesByPrefix(AstConstants.Node);
                    if (subNodes.Any())
                    {
                        return AnalyzeNode(subNodes.Single());
                    }
                    return new TaintSets().ClearTaint();
            }
        }

        private TaintSets Node_Cond(XmlNode node)
        {
            isConditional = true;

            _variables.Add(EdgeType.False, _variables[EdgeType.Normal].AssignmentClone());
            _variables.Add(EdgeType.True, _variables[EdgeType.Normal].AssignmentClone());
            _variables.Remove(EdgeType.Normal);

            var subnodes = node.GetSubNodesByPrefix(AstConstants.Node);
            return subnodes.Any() ? AnalyzeNode(subnodes.Single()) : new TaintSets().ClearTaint();
        }

        private TaintSets Expr_BooleanNot(XmlNode node)
        {
            isNegated = !isNegated;
            var result = Analyze(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr));
            return result;
        }

        private TaintSets Node_FuncCall(XmlNode node)
        {
            if (!isConditional)
            {
                return null;
            }

            // We use the True scope as both True and False are the same at this point in time
            var functionCallExtractor = new FunctionCallExtractor();
            var functionCall = functionCallExtractor.ExtractFunctionCall(node);

            var condSaniFunc = _funcHandler.FindCondSanitizerByName(functionCall.Name);
            if (condSaniFunc != null)
            {
                var parameter = functionCall.Arguments;
                var varResolverFalse = new VariableResolver(_variables[EdgeType.False]);
                if (parameter.Any(x => varResolverFalse.IsResolvableNode(x.Value)) 
                                    && condSaniFunc.DefaultStatus == MixedStatus.XSSSQL_SAFE)
                {
                    if (isNegated)
                    {
                        var var = varResolverFalse.ResolveVariable(parameter.First(x => varResolverFalse.IsResolvableNode(x.Value)).Value);
                        var.Variable.Info.Taints = new TaintSets().ClearTaint();
                    }
                    else
                    {
                        var varResolverTrue = new VariableResolver(_variables[EdgeType.True]);
                        var var = varResolverTrue.ResolveVariable(parameter.First(x => varResolverTrue.IsResolvableNode(x.Value)).Value);
                        var.Variable.Info.Taints = new TaintSets().ClearTaint();
                    }
                }
            }
            return new TaintSets().ClearTaint();
        }

        private TaintSets EqualsComparison(XmlNode node)
        {
            if (!isConditional)
            {
                return null;
            }
            var leftNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Left)
                               .GetSubNodesByPrefix(AstConstants.Node).Single();
            var rightNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Right)
                                .GetSubNodesByPrefix(AstConstants.Node).Single();

            var scalarNodes = new[]
                              {
                                  AstConstants.Nodes.Scalar_DNumber,
                                  AstConstants.Nodes.Scalar_LNumber,
                                  AstConstants.Nodes.Scalar_MagicConst_Class,
                                  AstConstants.Nodes.Scalar_MagicConst_Dir,
                                  AstConstants.Nodes.Scalar_MagicConst_File,
                                  AstConstants.Nodes.Scalar_MagicConst_Function,
                                  AstConstants.Nodes.Scalar_MagicConst_Line,
                                  AstConstants.Nodes.Scalar_MagicConst_Method,
                                  AstConstants.Nodes.Scalar_MagicConst_Namespace,
                                  AstConstants.Nodes.Scalar_MagicConst_Trait,
                                  AstConstants.Nodes.Scalar_String,
                              };

            if (_varResolver.IsResolvableNode(leftNode) && scalarNodes.Contains(rightNode.LocalName))
            {
                var varResolver = new VariableResolver(_variables[isNegated ? EdgeType.False : EdgeType.True]);
                var var = varResolver.ResolveVariable(leftNode);
                var.Variable.Info.Taints = new TaintSets().ClearTaint();
            }
            else if (scalarNodes.Contains(leftNode.LocalName) && _varResolver.IsResolvableNode(rightNode))
            {
                var varResolver = new VariableResolver(_variables[isNegated ? EdgeType.False : EdgeType.True]);
                var var = varResolver.ResolveVariable(rightNode);
                var.Variable.Info.Taints = new TaintSets().ClearTaint();
            }

            return new TaintSets().ClearTaint();
        }
    }
}
