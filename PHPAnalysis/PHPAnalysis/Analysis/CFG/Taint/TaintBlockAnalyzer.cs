using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using QuickGraph.Serialization;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public sealed class TaintBlockAnalyzer : IBlockAnalyzer
    {
        public ICollection<IBlockAnalyzerComponent> AnalysisExtensions { get; private set; } 

        internal List<ExpressionInfo> ReturnInfos { get; private set; }
        private IVariableStorage _variableStorage;
        private VariableResolver _varResolver;

        private readonly IVulnerabilityStorage _vulnerabilityStorage;
        private readonly IIncludeResolver _inclusionResolver;
        private readonly Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> _analyzer;
        private readonly AnalysisScope _analysisScope;
        private readonly AnalysisStacks _analysisStacks;
        private readonly FunctionAndMethodAnalyzerFactory _subroutineAnalyzerFactory; 

        private TaintBlockAnalyzer()
        {
            this.AnalysisExtensions = new List<IBlockAnalyzerComponent>();
        }

        public TaintBlockAnalyzer(IVulnerabilityStorage vulnerabilityStorage, IIncludeResolver inclusionResolver, AnalysisScope scope,
            Func<ImmutableVariableStorage, IIncludeResolver, AnalysisScope, AnalysisStacks, ImmutableVariableStorage> analyzeTaint, 
            AnalysisStacks stacks, FunctionAndMethodAnalyzerFactory subroutineAnalyzerFactory) : this()
        {
            Preconditions.NotNull(vulnerabilityStorage, "vulnerabilityStorage");
            Preconditions.NotNull(inclusionResolver, "inclusionResolver");
            Preconditions.NotNull(analyzeTaint, "analyzeTaint");
            Preconditions.NotNull(stacks, "stacks");
            Preconditions.NotNull(subroutineAnalyzerFactory, "subroutineAnalyzerFactory");

            this._vulnerabilityStorage = vulnerabilityStorage;
            this._inclusionResolver = inclusionResolver;
            this._analyzer = analyzeTaint;
            this._analysisScope = scope;
            this.ReturnInfos = new List<ExpressionInfo>();
            this._analysisStacks = stacks;
            this._subroutineAnalyzerFactory = subroutineAnalyzerFactory;
        }

        public ImmutableVariableStorage Analyze(XmlNode node, ImmutableVariableStorage knownTaint)
        {
            Preconditions.NotNull(knownTaint, "knownTaint");

            _variableStorage = knownTaint.ToMutable();
            _varResolver = new VariableResolver(_variableStorage, _analysisScope);

            Analyze(node);

            return ImmutableVariableStorage.CreateFromMutable(_variableStorage);
        }

        private ExpressionInfo Analyze(XmlNode node)
        {
            Preconditions.NotNull(node, "node");

            ExpressionInfo result;

            switch (node.Prefix)
            {
                case AstConstants.Node:
                    result = AnalyzeNode(node);
                    break;
                case AstConstants.Subnode:
                    result = AnalyzeSubnode(node);
                    break;
                case AstConstants.Scalar:
                    result = AnalyzeScalar(node);
                    break;
                case AstConstants.Attribute:
                    result = AnalyzeAttribute(node);
                    break;
                default:
                    throw new ArgumentException("Unknown nodetype. Was: " + node.Prefix, "node");
            }

            result = ApplyAnalysisExtensions(node, result);

            return result;
        }

        private ExpressionInfo ApplyAnalysisExtensions(XmlNode node, ExpressionInfo currentInfo)
        {
            
            foreach (var analysisExtension in AnalysisExtensions)
            {
                analysisExtension.FunctionMethodAnalyzerFactory = storage =>
                {
                    var customFunctionHandler = new CustomFunctionHandler(this._analyzer, _subroutineAnalyzerFactory);
                    customFunctionHandler.AnalysisExtensions.AddRange(this.AnalysisExtensions);
                    var varStorage = ImmutableVariableStorage.CreateFromMutable(storage);
                    return _subroutineAnalyzerFactory.Create(varStorage, _inclusionResolver, _analysisStacks,
                                                             customFunctionHandler, _vulnerabilityStorage);
                };
                currentInfo = analysisExtension.Analyze(node, currentInfo, _variableStorage, _vulnerabilityStorage);
            }
            return currentInfo;
        }

        /// <summary>
        /// Calls AnalyzeFuncCall on all external components. This was created because we currently do not call Analyze on "Node:Arg",
        /// which is needed to let the components find the arguments themselves.
        /// TODO: Handle Node:Arg in the analysis explicitly, to allow this.
        /// </summary>
        private ExpressionInfo ApplyAnalysisExtensionsToFuncCall(XmlNode node, ExpressionInfo currentInfo, IDictionary<uint, ExpressionInfo> argInfos)
        {
            foreach (var analysisExtension in AnalysisExtensions)
            {
                analysisExtension.FunctionMethodAnalyzerFactory = storage =>
                {
                    var customFunctionHandler = new CustomFunctionHandler(this._analyzer, _subroutineAnalyzerFactory);
                    customFunctionHandler.AnalysisExtensions.AddRange(this.AnalysisExtensions);
                    var varStorage = ImmutableVariableStorage.CreateFromMutable(storage);
                    return _subroutineAnalyzerFactory.Create(varStorage, _inclusionResolver, _analysisStacks,
                                                             customFunctionHandler, _vulnerabilityStorage);
                };
                currentInfo = analysisExtension.AnalyzeFunctionCall(node, currentInfo, _variableStorage, _vulnerabilityStorage, argInfos, this._analysisStacks);
            }
            return currentInfo;
        }

        private ExpressionInfo AnalyzeNode(XmlNode node)
        {
            switch (node.LocalName)
            {
                case AstConstants.Nodes.Expr_ArrayDimFetch:
                case AstConstants.Nodes.Expr_PropertyFetch: //Yes, array and property fetch can (currently) be handled alike.
                    return Node_Expr_ArrayDimFetch(node);
                case AstConstants.Nodes.Expr_Variable:
                    return Node_Expr_Variable(node);

                case AstConstants.Nodes.Expr_AssignRef:
                    // INVALID: Reference assignments _should_ be handled differently than normal assignments.
                case AstConstants.Nodes.Expr_Assign:
                    return Node_Expr_Assign(node);
                case AstConstants.Nodes.Expr_AssignOp_BitwiseAnd:
                case AstConstants.Nodes.Expr_AssignOp_BitwiseOr:
                case AstConstants.Nodes.Expr_AssignOp_BitwiseXor:
                    // POSSIBLY INCORRECT: See BinaryOp_Bitwise..
                case AstConstants.Nodes.Expr_AssignOp_Div:
                case AstConstants.Nodes.Expr_AssignOp_Minus:
                case AstConstants.Nodes.Expr_AssignOp_Mod:
                case AstConstants.Nodes.Expr_AssignOp_Plus:
                case AstConstants.Nodes.Expr_AssignOp_Mul:
                case AstConstants.Nodes.Expr_AssignOp_ShiftLeft:
                case AstConstants.Nodes.Expr_AssignOp_ShiftRight:
                case AstConstants.Nodes.Expr_AssignOp_Pow:
                    return Expr_AssignOp_NonSpecial_AlwaysSafe(node);
                case AstConstants.Nodes.Expr_AssignOp_Concat:
                    return Expr_AssignOp_Concat(node);

                case AstConstants.Nodes.Expr_Cast_Array:
                case AstConstants.Nodes.Expr_Cast_Bool:
                case AstConstants.Nodes.Expr_Cast_Double:
                case AstConstants.Nodes.Expr_Cast_Int:
                case AstConstants.Nodes.Expr_Cast_Object:
                case AstConstants.Nodes.Expr_Cast_String:
                case AstConstants.Nodes.Expr_Cast_Unset:
                    return Node_Expr_Cast(node);
                case AstConstants.Nodes.Expr_ConstFetch:
                    // POSSIBLY INCORRECT: Not necessarily the right thing to do. 
                    return new ExpressionInfo();
                
                case AstConstants.Nodes.Scalar_LNumber:
                case AstConstants.Nodes.Scalar_DNumber:
                    return Node_LDNumbers(node);

                case AstConstants.Nodes.Expr_FuncCall:
                    return Node_FuncCall(node);
                case AstConstants.Nodes.Stmt_Echo:
                    return Node_Echo(node);
                case AstConstants.Nodes.Expr_New:
                    return Node_New(node);
                case AstConstants.Nodes.Stmt_InlineHTML:
                    return new ExpressionInfo();
                case AstConstants.Nodes.Expr_Ternary:
                    return Expr_Ternary(node);

                case AstConstants.Nodes.Stmt_Case:
                case AstConstants.Nodes.Stmt_Do:
                case AstConstants.Nodes.Stmt_ElseIf:
                case AstConstants.Nodes.Stmt_For:
                case AstConstants.Nodes.Stmt_If:
                case AstConstants.Nodes.Stmt_Switch:
                case AstConstants.Nodes.Stmt_While:
                    return Analyze(Conditional.GetCondNode(node));
                
                case AstConstants.Nodes.Expr_BinaryOp_BooleanAnd:
                case AstConstants.Nodes.Expr_BinaryOp_BooleanOr:
                    return Expr_BinaryOp_BooleanOperator(node);
                case AstConstants.Nodes.Expr_BinaryOp_ShiftLeft:
                case AstConstants.Nodes.Expr_BinaryOp_ShiftRight:
                    // Docs: "Both operands and the result for the << and >> operators are always treated as integers." - https://php.net/manual/en/language.operators.bitwise.php
                case AstConstants.Nodes.Expr_BinaryOp_Mod:
                case AstConstants.Nodes.Expr_BinaryOp_Div:
                    // Docs: "Operands of modulus are converted to integers.." & "The division operator ("/") returns a float value unless the two operands are integers.." - https://php.net/language.operators.arithmetic
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseAnd:
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseOr:
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseXor:
                    // POSSIBLY INCORRECT: Docs: "If both operands for the &, | and ^ operators are strings, then the operation will be performed on the ASCII values of the characters that make up the strings and the result will be a string. In all other cases, both operands will be converted to integers.." - https://php.net/manual/en/language.operators.bitwise.php
                case AstConstants.Nodes.Expr_BinaryOp_Minus:
                case AstConstants.Nodes.Expr_BinaryOp_Mul:
                case AstConstants.Nodes.Expr_BinaryOp_Plus:
                case AstConstants.Nodes.Expr_BinaryOp_Pow:
                case AstConstants.Nodes.Expr_BinaryOp_Equal:
                case AstConstants.Nodes.Expr_BinaryOp_Greater:
                case AstConstants.Nodes.Expr_BinaryOp_GreaterOrEqual:
                case AstConstants.Nodes.Expr_BinaryOp_Identical:
                case AstConstants.Nodes.Expr_BinaryOp_LogicalAnd:
                case AstConstants.Nodes.Expr_BinaryOp_LogicalOr:
                case AstConstants.Nodes.Expr_BinaryOp_LogicalXor:
                case AstConstants.Nodes.Expr_BinaryOp_NotEqual:
                case AstConstants.Nodes.Expr_BinaryOp_NotIdentical:
                case AstConstants.Nodes.Expr_BinaryOp_Smaller:
                case AstConstants.Nodes.Expr_BinaryOp_SmallerOrEqual:
                    return Expr_BinaryOp_NonSpecial_AlwaysSafe(node);

                case AstConstants.Nodes.Scalar_MagicConst_Class:
                case AstConstants.Nodes.Scalar_MagicConst_Dir:
                case AstConstants.Nodes.Scalar_MagicConst_File:
                case AstConstants.Nodes.Scalar_MagicConst_Function:
                case AstConstants.Nodes.Scalar_MagicConst_Line:
                case AstConstants.Nodes.Scalar_MagicConst_Method:
                case AstConstants.Nodes.Scalar_MagicConst_Namespace:
                case AstConstants.Nodes.Scalar_MagicConst_Trait:
                    return new ExpressionInfo(); // These are known values and should probably be resolved.
                case AstConstants.Nodes.Expr_Isset:
                    return new ExpressionInfo();
                
                case AstConstants.Nodes.Expr_BooleanNot:
                    return Expr_BooleanNot(node);

                case AstConstants.Nodes.Scalar_String:
                    return new ExpressionInfo { ValueInfo = { Value = ScalarNode.GetStringValue(node), Type = "string"} };
                case AstConstants.Nodes.Scalar_Encapsed:
                    return Scalar_Encapsed(node);

                case AstConstants.Nodes.Expr_Print:
                case AstConstants.Nodes.Expr_Exit:
                    return Expr_Exit(node);

                case AstConstants.Nodes.Expr_BitwiseNot:
                    // POSSIBLY INCORRECT: Docs: "If the operand for the ~ operator is a string, the operation will be performed on the ASCII values.."
                case AstConstants.Nodes.Expr_UnaryMinus:
                case AstConstants.Nodes.Expr_UnaryPlus:
                    return Expr_UnaryOp_AlwaysSafe(node);

                case AstConstants.Nodes.Expr_BinaryOp_Concat:
                    return Expr_BinaryOp_Concat(node);

                case AstConstants.Nodes.Expr_PostDec:
                case AstConstants.Nodes.Expr_PostInc:
                case AstConstants.Nodes.Expr_PreInc:
                case AstConstants.Nodes.Expr_PreDec:
                    return Expr_IncDec(node);
                case AstConstants.Nodes.Expr_Array:
                    return Expr_Array(node);
                case AstConstants.Nodes.Expr_Include:
                    return Expr_Include(node);
                case AstConstants.Nodes.Stmt_Return:
                    return Stmt_Return(node);

                case AstConstants.Nodes.Stmt_Foreach:
                    return Stmt_Foreach(node);

                case AstConstants.Nodes.Expr_MethodCall:
                    return Node_MethodCall(node);
                case AstConstants.Nodes.Stmt_Global:
                    return Stmt_Global(node);


                case AstConstants.Nodes.Expr_ArrayItem:

                case AstConstants.Nodes.Stmt_Break:
                case AstConstants.Nodes.Stmt_Catch:
                case AstConstants.Nodes.Stmt_ClassConst:
                case AstConstants.Nodes.Stmt_ClassLike:
                case AstConstants.Nodes.Stmt_Class:
                case AstConstants.Nodes.Stmt_ClassMethod:
                case AstConstants.Nodes.Stmt_Const:
                case AstConstants.Nodes.Stmt_Continue:
                case AstConstants.Nodes.Stmt_Declare:
                case AstConstants.Nodes.Stmt_DeclareDeclare:
                case AstConstants.Nodes.Stmt_Else:
                
                case AstConstants.Nodes.Stmt_Function:
                case AstConstants.Nodes.Stmt_Goto:
                case AstConstants.Nodes.Stmt_HaltCompiler:
                case AstConstants.Nodes.Stmt_Interface:
                case AstConstants.Nodes.Stmt_Label:
                case AstConstants.Nodes.Stmt_Namespace:
                case AstConstants.Nodes.Stmt_Property:
                case AstConstants.Nodes.Stmt_PropertyProperty:
                case AstConstants.Nodes.Stmt_StaticVar:
                case AstConstants.Nodes.Stmt_Static:
                case AstConstants.Nodes.Stmt_Throw:
                case AstConstants.Nodes.Stmt_Trait:
                case AstConstants.Nodes.Stmt_TraitUse:
                case AstConstants.Nodes.Stmt_TraitUseAdaption:
                case AstConstants.Nodes.Stmt_Unset:
                case AstConstants.Nodes.Stmt_Use:
                case AstConstants.Nodes.Stmt_UseUse:
                
                case AstConstants.Nodes.Expr_ClassConstFetch:
                case AstConstants.Nodes.Expr_Closure:
                case AstConstants.Nodes.Expr_ClosureUse:
                case AstConstants.Nodes.Expr_Empty:
                case AstConstants.Nodes.Expr_ErrorSuppress:
                case AstConstants.Nodes.Expr_Eval:
                case AstConstants.Nodes.Expr_Instanceof:
                case AstConstants.Nodes.Expr_List:
                case AstConstants.Nodes.Expr_ShellExec:
                case AstConstants.Nodes.Expr_StaticCall:
                case AstConstants.Nodes.Expr_StaticPropertyFetch:
                
                case AstConstants.Nodes.Expr_Yield:

                case AstConstants.Nodes.Arg:
                case AstConstants.Nodes.Param:
                case AstConstants.Nodes.Const:
                case AstConstants.Nodes.Name:
                    return new ExpressionInfo();
                default:
                    return new ExpressionInfo();
                    throw new NotImplementedException("Unknown AST node. Was " + node.Name);
            }
        }

        private ExpressionInfo Stmt_Global(XmlNode node)
        {
            this._varResolver.ResolveGlobalDeclaration(node);
            return new ExpressionInfo();
        }

        private ExpressionInfo AnalyzeSubnode(XmlNode node)
        {
            switch (node.LocalName)
            {
                case AstConstants.Subnodes.Cond:
                    return Subnode_Cond(node);
                case AstConstants.Subnodes.Else:
                case AstConstants.Subnodes.Var:
                    return Subnode_WithNode(node);
                case AstConstants.Subnodes.ValueVar:
                case AstConstants.Subnodes.KeyVar:
                case AstConstants.Subnodes.Expr:
                case AstConstants.Subnodes.If:
                    return Subnode_WithNodeOrScalar(node);
                case AstConstants.Subnodes.Exprs:
                    return Subnode_Exprs(node);
                case AstConstants.Subnodes.Value:
                    return Subnode_Value(node);
                case AstConstants.Subnodes.Key:
                    return Subnode_Key(node);
                case AstConstants.Subnodes.Loop:
                case AstConstants.Subnodes.Init:
                    return Subnode_Init(node);

                case AstConstants.Subnodes.Left:
                case AstConstants.Subnodes.Right:
                    return Analyze(node.GetSubNodesByPrefix(AstConstants.Node).Single());

                case AstConstants.Subnodes.Adaptions:
                case AstConstants.Subnodes.Args:
                case AstConstants.Subnodes.ByRef:
                case AstConstants.Subnodes.Cases:
                case AstConstants.Subnodes.Catches:
                case AstConstants.Subnodes.Class:
                case AstConstants.Subnodes.Consts:
                case AstConstants.Subnodes.Declares:
                case AstConstants.Subnodes.Default:
                case AstConstants.Subnodes.Dim:
                case AstConstants.Subnodes.Extends:
                case AstConstants.Subnodes.FinallyStmts:
                case AstConstants.Subnodes.Implements:
                case AstConstants.Subnodes.InsteadOf:
                case AstConstants.Subnodes.Items:
                case AstConstants.Subnodes.Method:
                case AstConstants.Subnodes.Name:
                case AstConstants.Subnodes.NewModifier:
                case AstConstants.Subnodes.NewName:
                case AstConstants.Subnodes.Num:
                case AstConstants.Subnodes.Params:
                case AstConstants.Subnodes.Parts:
                case AstConstants.Subnodes.Props:
                case AstConstants.Subnodes.Remaining:
                case AstConstants.Subnodes.ReturnType:
                case AstConstants.Subnodes.Static:
                case AstConstants.Subnodes.Stmts:
                case AstConstants.Subnodes.Trait:
                case AstConstants.Subnodes.Traits:
                case AstConstants.Subnodes.Type:
                case AstConstants.Subnodes.Unpack:
                case AstConstants.Subnodes.Uses:
                case AstConstants.Subnodes.Variadic:
                case AstConstants.Subnodes.Vars:
                
                    throw new NotImplementedException("Unknown AST subnode. Was " + node.LocalName);
                default:
                    throw new NotImplementedException("Unknown AST subnode. Was " + node.LocalName);
            }
        }

        private ExpressionInfo AnalyzeScalar(XmlNode node)
        {
            switch (node.LocalName)
            {
                case AstConstants.Scalars.Int:
                    return new ExpressionInfo() {ValueInfo = { Value = node.InnerText, Type = "int"} };
                case AstConstants.Scalars.String:
                    return new ExpressionInfo() { ValueInfo = { Value = node.InnerText, Type = "string" } };
                case AstConstants.Scalars.Float:
                    return new ExpressionInfo() { ValueInfo = { Value = node.InnerText, Type = "float" } };
                case AstConstants.Scalars.False:
                case AstConstants.Scalars.True:
                case AstConstants.Scalars.Null:
                    return new ExpressionInfo();
                case AstConstants.Scalars.Array:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private ExpressionInfo AnalyzeAttribute(XmlNode node)
        {
            throw new NotImplementedException();
        }

        private ExpressionInfo Stmt_Foreach(XmlNode node)
        {
            var expr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            var keyVar = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.KeyVar);
            var byRef = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.ByRef);
            var valueVar = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.ValueVar);

            var exprResult = Analyze(expr);
            var keyVarResult = Analyze(keyVar);
            var valueVarResult = Analyze(valueVar);

            var exprNode = expr.GetSubNodesByPrefix(AstConstants.Node).Single();
            Variable exprVariable = new Variable("$UNKNOWN var$ - Just here to prevent null checks.", _analysisScope.ToVariableScope());
            if (_varResolver.IsResolvableNode(exprNode))
            {
                exprVariable = _varResolver.ResolveVariable(exprNode).Variable;
            }

            var defaultKeyTaint = exprResult.ValueInfo.DefaultDimensionTaintFactory();
            var defaultValuePossibleTaint = exprResult.ValueInfo.NestedVariablePossibleStoredDefaultTaintFactory();
            var defaultValueTaint = exprResult.ValueInfo.NestedVariableDefaultTaintFactory();

            var keyNode = keyVar.GetSubNodesByPrefix(AstConstants.Node).SingleOrDefault(); // Might be a scalar!
            if (keyNode != null && this._varResolver.IsResolvableNode(keyNode))
            {
                var varResolveResult = _varResolver.ResolveVariable(keyNode);
                varResolveResult.Variable.Info.Taints = varResolveResult.Variable.Info.Taints.Merge(defaultKeyTaint);
            }

            var variableTaints = exprResult.ValueInfo.Variables.Select(x => x.Value.Info.Taints).ToList();
            TaintSets worstVariableTaint = variableTaints.Any() ? variableTaints.Aggregate((current, next) => current.Merge(next))
                                                                : defaultValueTaint;

            worstVariableTaint = worstVariableTaint.Merge(defaultValueTaint);

            var variablePossibleTaint = exprResult.ValueInfo.Variables.Select(x => x.Value.Info.PossibleStoredTaint.Taint).ToList();
            TaintSets worstVariablePossibleTaint = variablePossibleTaint.Any()
                ? variablePossibleTaint.Aggregate((current, next) => current.Merge(next))
                : defaultValuePossibleTaint;

            worstVariablePossibleTaint = worstVariablePossibleTaint.Merge(defaultValuePossibleTaint);

            var valueNode = valueVar.GetSubNodesByPrefix(AstConstants.Node).Single();
            if (this._varResolver.IsResolvableNode(valueNode))
            {
                var varResolveResult = _varResolver.ResolveVariable(valueNode).Variable;
                varResolveResult.Info.Taints = varResolveResult.Info.Taints.Merge(worstVariableTaint);
                varResolveResult.Info.Taints = varResolveResult.Info.Taints.Merge(exprVariable.Unknown.Info.Taints);
                varResolveResult.Info.PossibleStoredTaint = _varResolver.ClonePossibleStored(exprVariable.Info);
                varResolveResult.Info.PossibleStoredTaint.Taint = varResolveResult.Info.PossibleStoredTaint.Taint.Merge(worstVariablePossibleTaint);
                varResolveResult.Info.PossibleStoredTaint.Taint = varResolveResult.Info.PossibleStoredTaint.Taint.Merge(exprVariable.Unknown.Info.PossibleStoredTaint.Taint);
                varResolveResult.Info.NestedVariablePossibleStoredDefaultTaintFactory = exprResult.ValueInfo.NestedVariablePossibleStoredDefaultTaintFactory;
                varResolveResult.Info.NestedVariableDefaultTaintFactory = exprResult.ValueInfo.NestedVariableDefaultTaintFactory;
            }

            return new ExpressionInfo();
        }

        private ExpressionInfo Subnode_Init(XmlNode node)
        {
            var initExpressions = node
                .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array)
                .GetSubNodesByPrefix(AstConstants.Node);

            foreach (var initExpression in initExpressions)
            {
                Analyze(initExpression);
            }
            return new ExpressionInfo();
        }

        private ExpressionInfo Expr_Include(XmlNode node)
        {
            var inclExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            Analyze(inclExpr);

            File file;
            if (_inclusionResolver.TryResolveInclude(node, out file))
            {
                if (!_analysisStacks.IncludeStack.Contains(file))
                {
                    Console.WriteLine(">> Resolved " + file.Name + ". Starting analysis.");
                    _analysisStacks.IncludeStack.Push(file);
                    var result = _analyzer(ImmutableVariableStorage.CreateFromMutable(_variableStorage), _inclusionResolver, _analysisScope, _analysisStacks);
                    this._variableStorage = result.Merge(ImmutableVariableStorage.CreateFromMutable(this._variableStorage)).ToMutable();
                    _analysisStacks.IncludeStack.Pop();
                    Console.WriteLine(">> Finished " + file.Name + ". Continuing.");
                }
            }

            return new ExpressionInfo();
        }

        private ExpressionInfo Expr_Array(XmlNode node)
        {
            var arrayItems = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Items)
                .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array)
                .GetSubnodes(AstConstants.Node + ":" + AstConstants.Nodes.Expr_ArrayItem);

            var varInfo = new ValueInfo();

            foreach (var arrayItem in arrayItems)
            {
                var info = Handle_Expr_ArrayItem(arrayItem);
                var arrayVar = new Variable(info.Item1.ToString(), VariableScope.Instance) 
                               {
                                   Info = info.Item2
                               };
                varInfo.Variables[info.Item1] = arrayVar;
            }
            return new ExpressionInfo() { ExpressionTaint = new TaintSets(), ValueInfo = varInfo };
        }
        private Tuple<VariableTreeDimension, ValueInfo> Handle_Expr_ArrayItem(XmlNode node)
        {
            VariableTreeDimension arrayKey;
            var itemInfo = new ValueInfo();

            var valueResult = Analyze(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value));
            itemInfo.Taints = valueResult.ExpressionTaint;

            if (valueResult.ValueInfo != null)
            {
                itemInfo = valueResult.ValueInfo.AssignmentClone();
            }

            var dimension = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Key);
            Analyze(dimension);

            // Start dimension resolving - This should probably be refactored!

            var dimNode = dimension.GetSubNodesByPrefix(AstConstants.Node).SingleOrDefault();

            // Rules: 
            // Strings with valid integers are cast
            // Floats are cast to integer (fraction is truncated)
            // Bools are cast to integers (true = 1, false = 0)
            // Null = empty string
            // Arrays/Objects cannot be used as key.
            // 
            // Conflict - Last one wins. 

            if (dimNode == null)
            {
                arrayKey = new VariableTreeDimension()
                           {
                               Index = -1,
                               Key = "$UKENDT$"
                           };
            }
            else
            {
                if (dimNode.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_String)
                {
                    var stringValue = ScalarNode.GetStringValue(dimNode);

                    arrayKey = new VariableTreeDimension() { Key = stringValue };
                    double indexValue;
                    if (double.TryParse(stringValue, out indexValue))
                    {
                        arrayKey.Index = (int)indexValue;
                    }
                }
                else if (dimNode.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_LNumber)
                {
                    var index = ScalarNode.GetLValue(dimNode);
                    arrayKey = new VariableTreeDimension()
                               {
                                   Index = index,
                                   Key = index.ToString(CultureInfo.InvariantCulture)
                               };
                }
                else if (dimNode.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_DNumber)
                {
                    var index = (int)ScalarNode.GetDValue(dimNode);
                    arrayKey = new VariableTreeDimension()
                               {
                                   Index = index,
                                   Key = index.ToString(CultureInfo.InvariantCulture)
                               };
                }
                else
                {
                    // Default case. ie. Non resolvable dimension
                    arrayKey = new VariableTreeDimension()
                               {
                                   Index = -1,
                                   Key = "$UKENDT$"
                               };
                }
            }

            // End dimension resolving.

            return new Tuple<VariableTreeDimension, ValueInfo>(arrayKey, itemInfo);
        }

        private ExpressionInfo Subnode_Key(XmlNode node)
        {
            var subNode = node.GetSubNodesByPrefix(AstConstants.Node);
            if (subNode.Any())
            {
                return AnalyzeNode(subNode.Single());
            }
            return new ExpressionInfo();
        }
        private ExpressionInfo Subnode_Value(XmlNode node)
        {
            return Analyze(node.GetSubNodesByPrefix(AstConstants.Node).Single());
        }

        private ExpressionInfo Expr_Ternary(XmlNode node)
        {
            var condNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Cond);
            var ifTrueNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.If);
            var ifFalseNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Else);

            Analyze(condNode);
            var condAnalyzer = new ConditionTaintAnalyser(_analysisScope, this._inclusionResolver, _analysisStacks.IncludeStack);
            
            var condResult = condAnalyzer.AnalyzeCond(condNode, ImmutableVariableStorage.CreateFromMutable(_variableStorage));

            var currentResolver = this._varResolver;
            this._varResolver = new VariableResolver(condResult[EdgeType.True].ToMutable());
            var leftTaint = Analyze(ifTrueNode);
            this._varResolver = new VariableResolver(condResult[EdgeType.False].ToMutable());
            var rightTaint = Analyze(ifFalseNode);

            this._varResolver = currentResolver;

            return leftTaint.Merge(rightTaint);
        }
        private ExpressionInfo Expr_IncDec(XmlNode node)
        {
            var varNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            Analyze(varNode);
            varNode = varNode.GetSubNodesByPrefix(AstConstants.Node).Single();

            VariableResolveResult variabel = _varResolver.ResolveVariable(varNode);

            return new ExpressionInfo() { ExpressionTaint = variabel.Variable.Info.Taints };
        }

        private ExpressionInfo Expr_BooleanNot(XmlNode node)
        {
            var result = Analyze(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr));
            return result;
        }

        private ExpressionInfo Expr_BinaryOp_Concat(XmlNode node)
        {
            XmlNode leftExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Left)
                                   .GetSubNodesByPrefix(AstConstants.Node).Single();
            
            XmlNode rightExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Right)
                                    .GetSubNodesByPrefix(AstConstants.Node).Single();
            var leftResult = Analyze(leftExpr);
            var rightResult = Analyze(rightExpr);

            var result = leftResult.Merge(rightResult);

            if (leftResult.ValueInfo.Value != null && rightResult.ValueInfo.Value != null)
            {
                result.ValueInfo.Value = leftResult.ValueInfo.Value + rightResult.ValueInfo.Value;
            }

            return result;
        }
        private ExpressionInfo Expr_BinaryOp_BooleanOperator(XmlNode node)
        {
            var left = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Left);
            var right = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Right);

            Analyze(left);
            Analyze(right);

            return new ExpressionInfo();
        }

        private ExpressionInfo Node_Expr_Assign(XmlNode node)
        {
            var rhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            var rhsTaint = Analyze(rhsExpr);
            var lhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);

            Variable variable;
            if (_varResolver.IsResolvableNode(lhsExpr.FirstChild))
            {
                variable = _varResolver.ResolveVariable(lhsExpr.FirstChild).Variable;
            }
            else
            {
                // LHS of assignment is not necessarily something we support. E.g. list(..,..) = ...
                // In that case we use a "dummy" variable 
                variable = new Variable("$UNKNOWN$", _analysisScope.ToVariableScope());
            }
            
            var rhsExprNodes = rhsExpr.GetSubNodesByPrefix(AstConstants.Node);
            if (rhsExprNodes.Any())
            {
                var rhsExprNode = rhsExprNodes.Single();
                if (_varResolver.IsResolvableNode(rhsExprNode))
                {
                    // If simple var to var assignment, move relevant var info to lhs variabel
                    var rhsVariable = _varResolver.ResolveVariable(rhsExprNode).Variable;
                    variable.Info = rhsVariable.Info.AssignmentClone();
                    return rhsTaint;   
                }

                if (rhsTaint.ValueInfo.Value != null)
                {
                    variable.Info.Value = rhsTaint.ValueInfo.Value;
                    variable.Info.Type = rhsTaint.ValueInfo.Type;
                    variable.Info.ClassNames.AddRange(rhsTaint.ValueInfo.ClassNames);
                }
            }

            variable.Info = variable.Info.Merge(rhsTaint.ValueInfo);

            if (rhsTaint.ValueInfo.Equals(new ValueInfo())) // HACK - Hacky way of determining if it hasn't been changed.
            {
                variable.Info.Taints = rhsTaint.ExpressionTaint;
                variable.Info.PossibleStoredTaint = rhsTaint.ExpressionStoredTaint;
                variable.Info.Type = rhsTaint.ValueInfo.Type;
                variable.Info.ClassNames.AddRange(rhsTaint.ValueInfo.ClassNames);
                return rhsTaint;
            }
            variable.Info = rhsTaint.ValueInfo.AssignmentClone();
            return rhsTaint;
        }
        private ExpressionInfo Expr_AssignOp_Concat(XmlNode node)
        {
            var rhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            var rhsTaint = Analyze(rhsExpr);
            var lhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            var lhsVariable = _varResolver.ResolveVariable(lhsExpr.FirstChild).Variable;

            lhsVariable.Info.Taints = lhsVariable.Info.Taints.Merge(rhsTaint.ExpressionTaint);
            var result = new ExpressionInfo() { ExpressionTaint = lhsVariable.Info.Taints }; 

            if (lhsVariable.Info.Value != null && rhsTaint.ValueInfo.Value != null)
            {
                lhsVariable.Info.Value = lhsVariable.Info.Value + rhsTaint.ValueInfo.Value;
            }

            return result;
        }
        private ExpressionInfo Expr_AssignOp_NonSpecial_AlwaysSafe(XmlNode node)
        {
            var rhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            Analyze(rhsExpr);
            var lhsExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            var variable = _varResolver.ResolveVariable(lhsExpr.FirstChild);

            variable.Variable.Info.Taints = new TaintSets().ClearTaint();
            
            return new ExpressionInfo();
        }
        private ExpressionInfo Expr_BinaryOp_NonSpecial_AlwaysSafe(XmlNode node)
        {
            XmlNode leftExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Left);
            Analyze(leftExpr);
            XmlNode rightExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Right);
            Analyze(rightExpr);

            return new ExpressionInfo();
        }
        private ExpressionInfo Expr_UnaryOp_AlwaysSafe(XmlNode node)
        {
            var expr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            Analyze(expr);
            return new ExpressionInfo();
        }

        private ExpressionInfo Expr_Exit(XmlNode node)
        {
            var exprNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            var expressionTaint = Analyze(exprNode);

            CheckForXssVulnerabilities(expressionTaint, node);

            return new ExpressionInfo();
        }

        private ExpressionInfo Node_New(XmlNode node)
        {
            //ExpressionInfo info = Analyze(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Class));

            string className = "";
            var classNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Class);

            XmlNode nameNode = null;
            bool success = classNode.TryGetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Name, out nameNode);
            if (success)
            {
                className = nameNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Parts).InnerText;
            }
            //TODO: extract class names from vars. (Requires var tracking)
            //TODO: Analyze constructor here

            var exprInfo = new ExpressionInfo();
            if (className != "")
            {
                exprInfo.ValueInfo.Type = className;
                exprInfo.ValueInfo.ClassNames.Add(className);
            }
            return exprInfo;
        }

        private ExpressionInfo Node_Echo(XmlNode node)
        {
            var taintResult = Analyze(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Exprs));

            CheckForXssVulnerabilities(taintResult, node);
            
            return new ExpressionInfo();
        }

        private ExpressionInfo Node_Expr_ArrayDimFetch(XmlNode node)
        {
            var varSubnode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            Analyze(varSubnode);

            var variabel = _varResolver.ResolveVariable(node).Variable;

            variabel.Info.Taints.XssTaint.ForEach(x => x.InitialTaintedVariable = variabel.Name);
            variabel.Info.Taints.SqliTaint.ForEach(x => x.InitialTaintedVariable = variabel.Name);
            variabel.Info.PossibleStoredTaint.Taint.XssTaint.ForEach(x => x.InitialTaintedVariable = variabel.Name);
            variabel.Info.PossibleStoredTaint.Taint.SqliTaint.ForEach(x => x.InitialTaintedVariable = variabel.Name);
            return new ExpressionInfo()
                   {
                       ExpressionTaint = variabel.Info.Taints, 
                       ValueInfo = variabel.Info.AssignmentClone()
                   };
        }

        private ExpressionInfo Node_Expr_Variable(XmlNode node)
        {
            var variable = _varResolver.ResolveVariable(node).Variable;
            // Todo: Hacky way, needs proper init-tracking
            variable.Info.Taints.XssTaint.ForEach(x => x.InitialTaintedVariable = variable.Name);
            variable.Info.Taints.SqliTaint.ForEach(x => x.InitialTaintedVariable = variable.Name);
            variable.Info.PossibleStoredTaint.Taint.XssTaint.ForEach(x => x.InitialTaintedVariable = variable.Name);
            variable.Info.PossibleStoredTaint.Taint.SqliTaint.ForEach(x => x.InitialTaintedVariable = variable.Name);

            return new ExpressionInfo() 
            { 
                ExpressionTaint = variable.Info.Taints, 
                ValueInfo = variable.Info.AssignmentClone()
            };
        }

        private ExpressionInfo Node_FuncCall(XmlNode node)
        {
            var functionCallExtractor = new FunctionCallExtractor();
            var functionCall = functionCallExtractor.ExtractFunctionCall(node);

            bool isAlreadyInStack = _analysisStacks.CallStack.Any(x => x.Name == functionCall.Name);
            _analysisStacks.CallStack.Push(functionCall);

            var argInfos = new List<ExpressionInfo>();

            //Actually extract the arguments
            for (uint index = 1; index <= functionCall.Arguments.Count; index++)
            {
                var item = functionCall.Arguments.FirstOrDefault(x => x.Key == index);
                var exprInfo = this.Analyze(item.Value);
                if (_varResolver.IsResolvableNode(item.Value))
                {
                    var variableResolveResult = _varResolver.ResolveVariable(item.Value);
                    exprInfo.ValueInfo = variableResolveResult.Variable.Info;
                }
                argInfos.Add(exprInfo);
            }

            if (functionCall.Name == "")
            {
                var expr_info = new ExpressionInfo();
                _analysisStacks.CallStack.Pop();
                return argInfos.Aggregate(expr_info, (current, info) => current.Merge(info));
            }
            
            var customFunctionHandler = new CustomFunctionHandler(this._analyzer, _subroutineAnalyzerFactory);
            customFunctionHandler.AnalysisExtensions.AddRange(this.AnalysisExtensions);
            var immutableVariableStorage = ImmutableVariableStorage.CreateFromMutable(_variableStorage);
            var functionMethodAnalyzer = this._subroutineAnalyzerFactory.Create(immutableVariableStorage, _inclusionResolver,
                _analysisStacks, customFunctionHandler, _vulnerabilityStorage);

            var resultTaintSet = new ExpressionInfo();
            if (!isAlreadyInStack)
            {
                resultTaintSet = functionMethodAnalyzer.AnalyzeFunctionCall(functionCall, argInfos);
            }
            
            FunctionsHandler fh = FunctionsHandler.Instance;
            var sqlSaniFunc = fh.FindSQLSanitizerByName(functionCall.Name);
            var sqlSinkFunc = fh.FindSQLSinkByName(functionCall.Name);
            var xssSaniFunc = fh.FindXSSSanitizerByName(functionCall.Name);
            var xssSinkFunc = fh.FindXSSSinkByName(functionCall.Name);
            
            if(sqlSaniFunc != null && sqlSaniFunc.DefaultStatus == SQLITaint.None)
            {
                resultTaintSet.ExpressionTaint.SqliTaint.Clear();
                resultTaintSet.ValueInfo.Taints.SqliTaint.Clear();
                resultTaintSet.ExpressionStoredTaint.Taint.SqliTaint.Clear();
            }
            if (xssSaniFunc != null && xssSaniFunc.DefaultStatus == XSSTaint.None)
            {
                resultTaintSet.ExpressionTaint.XssTaint.Clear();
                resultTaintSet.ValueInfo.Taints.XssTaint.Clear();
                resultTaintSet.ExpressionStoredTaint.Taint.XssTaint.Clear();
            }
            if (sqlSinkFunc != null)
            {
                var vulnerableSqlParams = sqlSinkFunc.Parameters.Where(x => x.Value.IsSensitive).ToDictionary(pair => pair.Key);
                var param = functionCall.Arguments.Where(x => vulnerableSqlParams.Keys.Any(z => z.Item1 == x.Key));
                    
                foreach (var parameter in param)
                {
                    var argInfo = argInfos.ElementAt((int) (parameter.Key - 1));
                    CheckForSQLVulnerabilities(argInfo, parameter.Value);
                }
            }
            if (xssSinkFunc != null)
            {
                var vulnerableXssParams = xssSinkFunc.Parameters.Where(x => x.Value.IsSensitive).ToDictionary(pair => pair.Key);
                var param = functionCall.Arguments.Where(x => vulnerableXssParams.Keys.Any(z => z.Item1 == x.Key));

                foreach (var parameter in param)
                {
                    var argInfo = argInfos.ElementAt((int)(parameter.Key - 1));
                    CheckForXssVulnerabilities(argInfo, parameter.Value);
                }
            }
            resultTaintSet = StoredFuncHandler(resultTaintSet, node, argInfos);

            var argumentInfoWithIndex = argInfos.Select((a, i) => new { Info = a, Index = (uint)i + 1 })
                                                .ToDictionary(a => a.Index, a => a.Info);
            resultTaintSet = ApplyAnalysisExtensionsToFuncCall(node, resultTaintSet, argumentInfoWithIndex);

            _analysisStacks.CallStack.Pop();
            return resultTaintSet;
        }

        private ExpressionInfo Node_MethodCall(XmlNode node)
        {
            var functionCallExtractor = new FunctionCallExtractor();
            var methodCall = functionCallExtractor.ExtractMethodCall(node, this._variableStorage, this._analysisScope);

            bool isAlreadyInStack = _analysisStacks.CallStack.Any(x => x.Name == methodCall.Name);
            _analysisStacks.CallStack.Push(methodCall);

            var argInfos = new List<ExpressionInfo>();

            //Actually analyze the arguments
            for (uint index = 1; index <= methodCall.Arguments.Count; index++)
            {
                var item = methodCall.Arguments.FirstOrDefault(x => x.Key == index);
                var exprInfo = this.Analyze(item.Value);
                if (_varResolver.IsResolvableNode(item.Value))
                {
                    var @var = _varResolver.ResolveVariable(item.Value);
                    exprInfo.ValueInfo = @var.Variable.Info;
                }
                argInfos.Add(exprInfo);
            }

            if (methodCall.Name == "")
            {
                var exprInfo = new ExpressionInfo();
                _analysisStacks.CallStack.Pop();
                return argInfos.Aggregate(exprInfo, (current, info) => current.Merge(info));
            }
            
            var customFunctionHandler = new CustomFunctionHandler(this._analyzer, _subroutineAnalyzerFactory);
            customFunctionHandler.AnalysisExtensions.AddRange(this.AnalysisExtensions);
            var functionMethodAnalyzer = _subroutineAnalyzerFactory.Create(ImmutableVariableStorage.CreateFromMutable(_variableStorage),
                _inclusionResolver, _analysisStacks, customFunctionHandler, _vulnerabilityStorage);

            var methodCallTaintSet = new ExpressionInfo();
            if(!isAlreadyInStack)
            {
                methodCallTaintSet = functionMethodAnalyzer.AnalyzeMethodCall(methodCall, argInfos);
            }

            FunctionsHandler fh = FunctionsHandler.Instance;
            var resultTaintSet = new ExpressionInfo();
            foreach (var className in methodCall.ClassNames.Distinct())
            {
                var tempResultTaintSet = methodCallTaintSet.AssignmentClone();
                var sqlSaniFunc = fh.FindSQLSanitizerByName(methodCall.CreateFullMethodName(className));
                var sqlSinkFunc = fh.FindSQLSinkByName(methodCall.CreateFullMethodName(className));
                var xssSaniFunc = fh.FindXSSSanitizerByName(methodCall.CreateFullMethodName(className));
                var xssSinkFunc = fh.FindXSSSinkByName(methodCall.CreateFullMethodName(className));

                if (sqlSaniFunc != null && sqlSaniFunc.DefaultStatus == SQLITaint.None)
                {
                    resultTaintSet.ExpressionTaint.SqliTaint.Clear();
                }
                if (xssSaniFunc != null && xssSaniFunc.DefaultStatus == XSSTaint.None)
                {
                    resultTaintSet.ExpressionTaint.XssTaint.Clear();
                }
                if (sqlSinkFunc != null || xssSinkFunc != null)
                {
                    if (sqlSinkFunc != null)
                    {
                        var vulnerableSqlParams = sqlSinkFunc.Parameters.Where(x => x.Value.IsSensitive)
                                                                        .ToDictionary(pair => pair.Key);
                        var parameters = methodCall.Arguments.Where(x => vulnerableSqlParams.Keys.Any(z => z.Item1 == x.Key));

                        foreach (var parameter in parameters)
                        {
                            //var argInfo = Analyze(parameter.Value);
                            var argInfo = argInfos.ElementAt((int)(parameter.Key - 1));
                            CheckForSQLVulnerabilities(argInfo, parameter.Value);
                        }
                        if (sqlSinkFunc.ReturnType == "object" || sqlSinkFunc.ReturnType == "mix")
                        {
                            resultTaintSet.ValueInfo.ClassNames.AddRange(sqlSinkFunc.Classnames);
                        }
                    }
                    if (xssSinkFunc != null)
                    {
                        var vulnerableXssParams = xssSinkFunc.Parameters.Where(x => x.Value.IsSensitive).ToDictionary(pair => pair.Key);
                        var param = methodCall.Arguments.Where(x => vulnerableXssParams.Keys.Any(z => z.Item1 == x.Key));

                        foreach (var parameter in param)
                        {
                            var argInfo = argInfos.ElementAt((int)(parameter.Key - 1));
                            CheckForXssVulnerabilities(argInfo, parameter.Value);
                        }
                    }
                    // Assuming sinks does not return taint.
                    resultTaintSet.ExpressionTaint.ClearTaint();
                    resultTaintSet.ExpressionStoredTaint.Taint.ClearTaint();
                }
                tempResultTaintSet = StoredMethodHandler(tempResultTaintSet, node);
                resultTaintSet = resultTaintSet.Merge(tempResultTaintSet);

                var methodNameWithClass = methodCall.CreateFullMethodName(className);
                bool isStoredProvider = FunctionsHandler.Instance.FindStoredProviderMethods(methodNameWithClass).Any();
                if (isStoredProvider)
                {
                    resultTaintSet.ExpressionStoredTaint = resultTaintSet.ExpressionStoredTaint.Merge(methodCall.Var.Info.PossibleStoredTaint);
                    //TODO: The following is not true in all cases.
                    // What cases? 
                    var cloned = resultTaintSet.ExpressionStoredTaint.Taint.DeepClone();
                    resultTaintSet.ValueInfo.NestedVariablePossibleStoredDefaultTaintFactory = () => cloned;
                }
            }

            _analysisStacks.CallStack.Pop();
            return resultTaintSet;
        }

        private ExpressionInfo StoredMethodHandler(ExpressionInfo exprInfo, XmlNode node)
        {
            var functionCallExtractor = new FunctionCallExtractor();
            var methodCall = functionCallExtractor.ExtractMethodCall(node, this._variableStorage, this._analysisScope);
            var fh = FunctionsHandler.Instance;

            foreach (var className in methodCall.ClassNames.Distinct())
            {
                var sqlSinkFunc = fh.FindSQLSinkByName(methodCall.CreateFullMethodName(className));
                if (sqlSinkFunc == null)
                {
                    continue;
                }

                var vulnerableSqlParams = sqlSinkFunc.Parameters.Where(x => x.Value.IsSensitive).ToDictionary(pair => pair.Key);
                var param = methodCall.Arguments.Where(x => vulnerableSqlParams.Keys.Any(z => z.Item1 == x.Key));

                foreach (var parameter in param)
                {
                    ExpressionInfo customParameterAnalysis = Analyze(parameter.Value);
                    if (customParameterAnalysis.ValueInfo.Value == null)
                        continue;
                    if (StringAnalysis.IsSQLInsertionStmt(customParameterAnalysis.ValueInfo.Value))
                    {
                        customParameterAnalysis.ExpressionStoredTaint =
                            new StoredVulnInfo(StringAnalysis.RetrieveSQLTableName(customParameterAnalysis.ValueInfo.Value), 
                                AstNode.GetStartLine(node)) {
                                                                Taint = customParameterAnalysis.ExpressionTaint,
                                                                ICantFeelIt = IsItInYet.YesItsGoingIn
                                                            };
                        InsertIntoStoredLocation(customParameterAnalysis, node);
                        customParameterAnalysis = new ExpressionInfo();
                    }
                    else if (StringAnalysis.IsSQLRetrieveStmt(customParameterAnalysis.ValueInfo.Value))
                    {
                        customParameterAnalysis.ExpressionStoredTaint =
                            new StoredVulnInfo(StringAnalysis.RetrieveSQLTableName(customParameterAnalysis.ValueInfo.Value), 
                                AstNode.GetStartLine(node)) {
                                                                Taint = new DefaultTaintProvider().GetTaintedTaintSet(),
                                                                ICantFeelIt = IsItInYet.NoImPullingOut
                                                            };
                        exprInfo.ValueInfo.NestedVariablePossibleStoredDefaultTaintFactory = () => new DefaultTaintProvider().GetTaintedTaintSet();
                    }
                    exprInfo.ExpressionStoredTaint = exprInfo.ExpressionStoredTaint.Merge(customParameterAnalysis.ExpressionStoredTaint);
                }
            }
            return exprInfo;
        }

        private ExpressionInfo StoredFuncHandler(ExpressionInfo exprInfo, XmlNode node, List<ExpressionInfo> argInfos)
        {
            var resultExpr = new ExpressionInfo();
            var functionCallExtractor = new FunctionCallExtractor();
            var functionCall = functionCallExtractor.ExtractFunctionCall(node);
            var fh = FunctionsHandler.Instance;

            var sqlSinkFunc = fh.FindSQLSinkByName(functionCall.Name);

            if (sqlSinkFunc != null)
            {
                var vulnerableSqlParams = sqlSinkFunc.Parameters.Where(x => x.Value.IsSensitive).ToDictionary(pair => pair.Key);
                var param = functionCall.Arguments.Where(x => vulnerableSqlParams.Keys.Any(z => z.Item1 == x.Key));

                foreach (var arg in argInfos)
                {
                    exprInfo = arg;
                    if (exprInfo.ValueInfo.Value == null)
                    {
                        continue;
                    }
                    if (StringAnalysis.IsSQLInsertionStmt(exprInfo.ValueInfo.Value))
                    {
                        exprInfo.ExpressionStoredTaint = 
                            new StoredVulnInfo(StringAnalysis.RetrieveSQLTableName(exprInfo.ValueInfo.Value), AstNode.GetStartLine(node))
                            {
                                Taint = exprInfo.ExpressionTaint,
                                ICantFeelIt = IsItInYet.YesItsGoingIn
                            };
                        InsertIntoStoredLocation(exprInfo, node);
                        exprInfo = new ExpressionInfo();
                    }
                    else if(StringAnalysis.IsSQLRetrieveStmt(exprInfo.ValueInfo.Value))
                    {
                        exprInfo.ExpressionStoredTaint = 
                            new StoredVulnInfo(StringAnalysis.RetrieveSQLTableName(exprInfo.ValueInfo.Value), AstNode.GetStartLine(node))
                            {
                                Taint = new DefaultTaintProvider().GetTaintedTaintSet(),
                                ICantFeelIt = IsItInYet.NoImPullingOut
                            };
                        resultExpr.ValueInfo.NestedVariablePossibleStoredDefaultTaintFactory = () => new DefaultTaintProvider().GetTaintedTaintSet();
                    }
                    resultExpr.ExpressionStoredTaint = resultExpr.ExpressionStoredTaint.Merge(exprInfo.ExpressionStoredTaint);
                }
            }
            else
            {
                return exprInfo;
            }
            return resultExpr;
        }

        private ExpressionInfo Stmt_Return(XmlNode node)
        {
            var exprNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            ExpressionInfo exprInfo = Analyze(exprNode);
            this.ReturnInfos.Add(exprInfo);
            return new ExpressionInfo();
        }

        private ExpressionInfo Node_LDNumbers(XmlNode node)
        {
            // Numbers cannot be tainted - Return notaint for all sets
            return new ExpressionInfo();
        }
        private ExpressionInfo Node_Expr_Cast(XmlNode node)
        {
            var subExpr = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr);
            var subExprTaint = Analyze(subExpr);
            var sanitizationCasts = new[]
            {
                AstConstants.Nodes.Expr_Cast_Unset, AstConstants.Nodes.Expr_Cast_Bool,
                AstConstants.Nodes.Expr_Cast_Double, AstConstants.Nodes.Expr_Cast_Int,
            };
            if (sanitizationCasts.Contains(node.LocalName))
            {
                return new ExpressionInfo();
            }
            return subExprTaint;
        }

        private ExpressionInfo Subnode_Cond(XmlNode node)
        {
            var subNodes = node.GetSubNodesByPrefix(AstConstants.Node);
            
            return subNodes.Any() ? Analyze(subNodes.Single()) : new ExpressionInfo();
        }
        private ExpressionInfo Subnode_WithNode(XmlNode node)
        {
            return Analyze(node.GetSubNodesByPrefix(AstConstants.Node).Single());
        }
        private ExpressionInfo Subnode_WithNodeOrScalar(XmlNode node)
        {
            var subnodes = node.GetSubNodesByPrefix(AstConstants.Node);
            if (subnodes.Any())
            {
                return Analyze(subnodes.Single());
            }

            // Expr can contain a <scalar:null> rather than an expression.
            // in that case, there is no taint.
            return new ExpressionInfo();
        }

        private ExpressionInfo Subnode_Exprs(XmlNode node)
        {
            var arraynode = node.GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array);
            var subnodes = arraynode.GetSubNodesByPrefix(AstConstants.Node);

            var results = new ExpressionInfo();
            
            var temptaintsets = subnodes.Select(Analyze).ToList();

            return temptaintsets.Aggregate(results, (current, exprTaint) => current.Merge(exprTaint));
        }

        private ExpressionInfo Scalar_Encapsed(XmlNode node)
        {
            var partsArray = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Parts)
                                 .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array);

            var encapsedParts = partsArray.GetSubNodesByPrefixes(AstConstants.Node, AstConstants.Scalar);

            var result = new ExpressionInfo();

            foreach (var partResult in encapsedParts.Select(Analyze))
            {
                result.ExpressionTaint = result.ExpressionTaint.Merge(partResult.ExpressionTaint);
                result.ValueInfo.Value = result.ValueInfo.Value + partResult.ValueInfo.Value;
            }
            return result;
        }

        private void CheckForXssVulnerabilities(ExpressionInfo expressionInfo, XmlNode node)
        {
            foreach (var vuln in expressionInfo.ExpressionTaint.XssTaint.Where(taint => taint.TaintTag != XSSTaint.None))
            {
                string varName = (vuln.InitialTaintedVariable ?? "???");
                string message = "XSS vulnerability found on variable: " + varName + 
                                 " on line: " + AstNode.GetStartLine(node) + " in file: " + _analysisStacks.IncludeStack.Peek();
                _vulnerabilityStorage.AddVulnerability(new VulnerabilityInfo()
                                                       {
                                                           Message = message,
                                                           IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                                                           CallStack = _analysisStacks.CallStack.ToImmutableStack()
                                                       });
            }
            if (expressionInfo.ValueInfo.PossibleStoredTaint == null)
            {
                return;
            }

            var xssTaintSets = expressionInfo.ValueInfo.PossibleStoredTaint.Taint.XssTaint;
            foreach (var possibleVuln in xssTaintSets.Where(taint => taint.TaintTag != XSSTaint.None))
            {
                string varName = possibleVuln.InitialTaintedVariable ?? "???";
                var vulnMessage = "Tainted outgoing reaches sensitive sink: {0} on line: {1} in file: {2}";
                string message = string.Format(vulnMessage, varName, AstNode.GetStartLine(node), _analysisStacks.IncludeStack.Peek());
                var vulnInfo = new StoredVulnerabilityInfo() 
                               { 
                                   Message = message,
                                   PossibleStoredVuln = expressionInfo.ValueInfo.PossibleStoredTaint,
                                   IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                                   CallStack = _analysisStacks.CallStack.ToImmutableStack(),
                                   VulnerabilityType = VulnType.XSS
                               };
                _vulnerabilityStorage.AddPossibleStoredVulnerability(vulnInfo);
            }
        }
        private void CheckForSQLVulnerabilities(ExpressionInfo expressionInfo, XmlNode node)
        {
            foreach (var vuln in expressionInfo.ExpressionTaint.SqliTaint)
            {
                if (vuln.TaintTag != SQLITaint.None)
                {
                    string message = "SQL vulnerability found on variable: " + vuln.InitialTaintedVariable +
                                     " on line: " + AstNode.GetStartLine(node) + " in file: " + _analysisStacks.IncludeStack.Peek();
                    _vulnerabilityStorage.AddVulnerability(new VulnerabilityInfo()
                                                           {
                                                               Message = message,
                                                               IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                                                               CallStack = _analysisStacks.CallStack.ToImmutableStack(),
                                                           });
                }
            }
            if (expressionInfo.ValueInfo.PossibleStoredTaint == null)
            {
                return;
            }
            var sqliTaintSets = expressionInfo.ValueInfo.PossibleStoredTaint.Taint.SqliTaint;
            foreach (var possibleVuln in sqliTaintSets.Where(taint => taint.TaintTag != SQLITaint.None))
            {
                string varName = possibleVuln.InitialTaintedVariable ?? "???";
                var vulnMessage = "Tainted outgoing reaches sensitive sink: {0} on line: {1} in file: {2}";
                string message = string.Format(vulnMessage, varName, AstNode.GetStartLine(node), _analysisStacks.IncludeStack.Peek());
                var vulnInfo = new StoredVulnerabilityInfo()
                               {
                                   Message = message,
                                   PossibleStoredVuln = expressionInfo.ValueInfo.PossibleStoredTaint,
                                   IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                                   CallStack = _analysisStacks.CallStack.ToImmutableStack(),
                                   VulnerabilityType = VulnType.SQL
                               };
                _vulnerabilityStorage.AddPossibleStoredVulnerability(vulnInfo);
            }
        }

        private void InsertIntoStoredLocation(ExpressionInfo expressionInfo, XmlNode node)
        {
            if (expressionInfo.ExpressionStoredTaint != null)
            {
                if (expressionInfo.ExpressionStoredTaint.ICantFeelIt != IsItInYet.YesItsGoingIn)
                {
                    throw new NotSupportedException("Trying to insert a Stored taint into vulnerabilityStorage, " +
                                                    "but is not tagged with in-going store taint");
                }
                // SQLI
                foreach (var possibleVuln in expressionInfo.ExpressionStoredTaint.Taint.SqliTaint)
                {
                    if (possibleVuln.TaintTag != SQLITaint.None)
                    {
                        string varName = (possibleVuln.InitialTaintedVariable ?? "???");
                        string message = "Stored SQLI found - Ingoing: " + varName +
                                        " on line: " + AstNode.GetStartLine(node) + " in file: " + _analysisStacks.IncludeStack.Peek();
                        _vulnerabilityStorage.AddPossibleStoredVulnerability(new StoredVulnerabilityInfo()
                        {
                            Message = message,
                            PossibleStoredVuln = expressionInfo.ExpressionStoredTaint,
                            IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                            CallStack = _analysisStacks.CallStack.ToImmutableStack(),
                            VulnerabilityType = VulnType.SQL
                        });
                    }
                }
                // XSS
                foreach (var possibleVuln in expressionInfo.ExpressionStoredTaint.Taint.XssTaint)
                {
                    if (possibleVuln.TaintTag != XSSTaint.None)
                    {
                        string varName = (possibleVuln.InitialTaintedVariable ?? "???");
                        string message = "Stored XSS found - Ingoing: " + varName +
                                        " on line: " + AstNode.GetStartLine(node) + " in file: " + _analysisStacks.IncludeStack.Peek();
                        _vulnerabilityStorage.AddPossibleStoredVulnerability(new StoredVulnerabilityInfo()
                        {
                            Message = message,
                            PossibleStoredVuln = expressionInfo.ExpressionStoredTaint,
                            IncludeStack = _analysisStacks.IncludeStack.ToImmutableStack(),
                            CallStack = _analysisStacks.CallStack.ToImmutableStack(),
                            VulnerabilityType = VulnType.XSS
                        });
                    }
                }
            }
        }
    }
}