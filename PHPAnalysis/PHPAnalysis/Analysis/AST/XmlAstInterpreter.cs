using System;
using System.Xml;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.AST
{
    public interface IAstNodeAnalyzer<out T1, in T2>
    {
        T1 Analyze(XmlNode node, T2 context = default(T2));
    }

    public abstract class AstNodeAnalyzer<T1, T2> : IAstNodeAnalyzer<T1, T2>
    {
        public T1 Analyze(XmlNode node, T2 context = default(T2))
        {
            Preconditions.NotNull(node, "node");
            switch (node.Prefix)
            {
                case AstConstants.Node:
                    return AnalyzeNode(node, context);
                case AstConstants.Subnode:
                    return AnalyzeSubnode(node, context);
                case AstConstants.Scalar:
                    return AnalyzeScalar(node, context);
                case AstConstants.Attribute:
                    return AnalyzeAttribute(node, context);
                default:
                    throw new ArgumentException("Unknown nodetype. Was: " + node.Prefix, "node");
            }
        }

        private T1 AnalyzeNode(XmlNode node, T2 context)
        {
            switch (node.LocalName)
            {
                case AstConstants.Nodes.Stmt_Break:             return Node_Stmt_Break(node, context);
                case AstConstants.Nodes.Stmt_Case:              return Node_Stmt_Case(node, context);
                case AstConstants.Nodes.Stmt_Catch:             return Node_Stmt_Catch(node, context);
                case AstConstants.Nodes.Stmt_ClassConst:        return Node_Stmt_ClassConst(node, context);
                case AstConstants.Nodes.Stmt_ClassLike:         return Node_Stmt_ClassLike(node, context);
                case AstConstants.Nodes.Stmt_Class:             return Node_Stmt_Class(node, context);
                case AstConstants.Nodes.Stmt_ClassMethod:       return Node_Stmt_ClassMethod(node, context);
                case AstConstants.Nodes.Stmt_Const:             return Node_Stmt_Const(node, context);
                case AstConstants.Nodes.Stmt_Continue:          return Node_Stmt_Continue(node, context);
                case AstConstants.Nodes.Stmt_Declare:           return Node_Stmt_Declare(node, context);
                case AstConstants.Nodes.Stmt_DeclareDeclare:    return Node_Stmt_DeclareDeclare(node, context);
                case AstConstants.Nodes.Stmt_Do:                return Node_Stmt_Do(node, context);
                case AstConstants.Nodes.Stmt_Echo:              return Node_Stmt_Echo(node, context);
                case AstConstants.Nodes.Stmt_ElseIf:            return Node_Stmt_ElseIf(node, context);
                case AstConstants.Nodes.Stmt_Else:              return Node_Stmt_Else(node, context);
                case AstConstants.Nodes.Stmt_For:               return Node_Stmt_For(node, context);
                case AstConstants.Nodes.Stmt_Foreach:           return Node_Stmt_Foreach(node, context);
                case AstConstants.Nodes.Stmt_Function:          return Node_Stmt_Function(node, context);
                case AstConstants.Nodes.Stmt_Global:            return Node_Stmt_Global(node, context);
                case AstConstants.Nodes.Stmt_Goto:              return Node_Stmt_Goto(node, context);
                case AstConstants.Nodes.Stmt_HaltCompiler:      return Node_Stmt_HaltCompiler(node, context);
                case AstConstants.Nodes.Stmt_If:                return Node_Stmt_If(node, context);
                case AstConstants.Nodes.Stmt_InlineHTML:        return Node_Stmt_InlineHTML(node, context);
                case AstConstants.Nodes.Stmt_Interface:         return Node_Stmt_Interface(node, context);
                case AstConstants.Nodes.Stmt_Label:             return Node_Stmt_Label(node, context);
                case AstConstants.Nodes.Stmt_Namespace:         return Node_Stmt_Namespace(node, context);
                case AstConstants.Nodes.Stmt_Property:          return Node_Stmt_Property(node, context);
                case AstConstants.Nodes.Stmt_PropertyProperty:  return Node_Stmt_PropertyProperty(node, context);
                case AstConstants.Nodes.Stmt_Return:            return Node_Stmt_Return(node, context);
                case AstConstants.Nodes.Stmt_StaticVar:         return Node_Stmt_StaticVar(node, context);
                case AstConstants.Nodes.Stmt_Static:            return Node_Stmt_Static(node, context);
                case AstConstants.Nodes.Stmt_Switch:            return Node_Stmt_Switch(node, context);
                case AstConstants.Nodes.Stmt_Throw:             return Node_Stmt_Throw(node, context);
                case AstConstants.Nodes.Stmt_Trait:             return Node_Stmt_Trait(node, context);
                case AstConstants.Nodes.Stmt_TraitUse:          return Node_Stmt_TraitUse(node, context);
                case AstConstants.Nodes.Stmt_TraitUseAdaption:  return Node_Stmt_TraitUseAdaption(node, context);
                case AstConstants.Nodes.Stmt_Unset:             return Node_Stmt_Unset(node, context);
                case AstConstants.Nodes.Stmt_Use:               return Node_Stmt_Use(node, context);
                case AstConstants.Nodes.Stmt_UseUse:            return Node_Stmt_UseUse(node, context);
                case AstConstants.Nodes.Stmt_While:             return Node_Stmt_While(node, context);

                case AstConstants.Nodes.Expr_Array:                     return Node_Expr_Array(node, context);
                case AstConstants.Nodes.Expr_ArrayDimFetch:             return Node_Expr_ArrayDimFetch(node, context);
                case AstConstants.Nodes.Expr_ArrayItem:                 return Node_Expr_ArrayItem(node, context);
                case AstConstants.Nodes.Expr_Assign:                    return Node_Expr_Assign(node, context);
                case AstConstants.Nodes.Expr_AssignOp_BitwiseAnd:       return Node_Expr_AssignOp_BitwiseAnd(node, context);
                case AstConstants.Nodes.Expr_AssignOp_BitwiseOr:        return Node_Expr_AssignOp_BitwiseOr(node, context);
                case AstConstants.Nodes.Expr_AssignOp_BitwiseXor:       return Node_Expr_AssignOp_BitwiseXor(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Concat:           return Node_Expr_AssignOp_Concat(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Div:              return Node_Expr_AssignOp_Div(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Minus:            return Node_Expr_AssignOp_Minus(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Mod:              return Node_Expr_AssignOp_Mod(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Plus:             return Node_Expr_AssignOp_Plus(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Mul:              return Node_Expr_AssignOp_Mul(node, context);
                case AstConstants.Nodes.Expr_AssignOp_ShiftLeft:        return Node_Expr_AssignOp_ShiftLeft(node, context);
                case AstConstants.Nodes.Expr_AssignOp_ShiftRight:       return Node_Expr_AssignOp_ShiftRight(node, context);
                case AstConstants.Nodes.Expr_AssignOp_Pow:              return Node_Expr_AssignOp_Pow(node, context);
                case AstConstants.Nodes.Expr_AssignRef:                 return Node_Expr_AssignRef(node, context);
                case AstConstants.Nodes.Expr_BooleanNot:                return Node_Expr_BooleanNot(node, context);
                case AstConstants.Nodes.Expr_BitwiseNot:                return Node_Expr_BitwiseNot(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseAnd:       return Node_Expr_BinaryOp_BitwiseAnd(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseOr:        return Node_Expr_BinaryOp_BitwiseOr(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_BitwiseXor:       return Node_Expr_BinaryOp_BitwiseXor(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_BooleanAnd:       return Node_Expr_BinaryOp_BooleanAnd(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_BooleanOr:        return Node_Expr_BinaryOp_BooleanOr(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Concat:           return Node_Expr_BinaryOp_Concat(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Div:              return Node_Expr_BinaryOp_Div(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Equal:            return Node_Expr_BinaryOp_Equal(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Greater:          return Node_Expr_BinaryOp_Greater(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_GreaterOrEqual:   return Node_Expr_BinaryOp_GreaterOrEqual(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Identical:        return Node_Expr_BinaryOp_Identical(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_LogicalAnd:       return Node_Expr_BinaryOp_LogicalAnd(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_LogicalOr:        return Node_Expr_BinaryOp_LogicalOr(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_LogicalXor:       return Node_Expr_BinaryOp_LogicalXor(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Minus:            return Node_Expr_BinaryOp_Minus(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Mod:              return Node_Expr_BinaryOp_Mod(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Mul:              return Node_Expr_BinaryOp_Mul(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_NotEqual:         return Node_Expr_BinaryOp_NotEqual(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_NotIdentical:     return Node_Expr_BinaryOp_NotIdentical(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Plus:             return Node_Expr_BinaryOp_Plus(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Pow:              return Node_Expr_BinaryOp_Pow(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_ShiftLeft:        return Node_Expr_BinaryOp_ShiftLeft(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_ShiftRight:       return Node_Expr_BinaryOp_ShiftRight(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_Smaller:          return Node_Expr_BinaryOp_Smaller(node, context);
                case AstConstants.Nodes.Expr_BinaryOp_SmallerOrEqual:   return Node_Expr_BinaryOp_SmallerOrEqual(node, context);
                case AstConstants.Nodes.Expr_Cast_Array:                return Node_Expr_Cast_Array(node, context);
                case AstConstants.Nodes.Expr_Cast_Bool:                 return Node_Expr_Cast_Bool(node, context);
                case AstConstants.Nodes.Expr_Cast_Double:               return Node_Expr_Cast_Double(node, context);
                case AstConstants.Nodes.Expr_Cast_Int:                  return Node_Expr_Cast_Int(node, context);
                case AstConstants.Nodes.Expr_Cast_Object:               return Node_Expr_Cast_Object(node, context);
                case AstConstants.Nodes.Expr_Cast_String:               return Node_Expr_Cast_String(node, context);
                case AstConstants.Nodes.Expr_Cast_Unset:                return Node_Expr_Cast_Unset(node, context);
                case AstConstants.Nodes.Expr_ClassConstFetch:           return Node_Expr_ClassConstFetch(node, context);
                case AstConstants.Nodes.Expr_Closure:                   return Node_Expr_Closure(node, context);
                case AstConstants.Nodes.Expr_ClosureUse:                return Node_Expr_ClosureUse(node, context);
                case AstConstants.Nodes.Expr_ConstFetch:                return Node_Expr_ConstFetch(node, context);
                case AstConstants.Nodes.Expr_Empty:                     return Node_Expr_Empty(node, context);
                case AstConstants.Nodes.Expr_ErrorSuppress:             return Node_Expr_ErrorSuppress(node, context);
                case AstConstants.Nodes.Expr_Eval:                      return Node_Expr_Eval(node, context);
                case AstConstants.Nodes.Expr_Exit:                      return Node_Expr_Exit(node, context);
                case AstConstants.Nodes.Expr_FuncCall:                  return Node_Expr_FuncCall(node, context);
                case AstConstants.Nodes.Expr_Include:                   return Node_Expr_Include(node, context);
                case AstConstants.Nodes.Expr_Instanceof:                return Node_Expr_Instanceof(node, context);
                case AstConstants.Nodes.Expr_Isset:                     return Node_Expr_Isset(node, context);
                case AstConstants.Nodes.Expr_List:                      return Node_Expr_List(node, context);
                case AstConstants.Nodes.Expr_MethodCall:                return Node_Expr_MethodCall(node, context);
                case AstConstants.Nodes.Expr_New:                       return Node_Expr_New(node, context);
                case AstConstants.Nodes.Expr_PostDec:                   return Node_Expr_PostDec(node, context);
                case AstConstants.Nodes.Expr_PostInc:                   return Node_Expr_PostInc(node, context);
                case AstConstants.Nodes.Expr_PreInc:                    return Node_Expr_PreInc(node, context);
                case AstConstants.Nodes.Expr_PreDec:                    return Node_Expr_PreDec(node, context);
                case AstConstants.Nodes.Expr_Print:                     return Node_Expr_Print(node, context);
                case AstConstants.Nodes.Expr_PropertyFetch:             return Node_Expr_PropertyFetch(node, context);
                case AstConstants.Nodes.Expr_ShellExec:                 return Node_Expr_ShellExec(node, context);
                case AstConstants.Nodes.Expr_StaticCall:                return Node_Expr_StaticCall(node, context);
                case AstConstants.Nodes.Expr_StaticPropertyFetch:       return Node_Expr_StaticPropertyFetch(node, context);
                case AstConstants.Nodes.Expr_Ternary:                   return Node_Expr_Ternary(node, context);
                case AstConstants.Nodes.Expr_UnaryMinus:                return Node_Expr_UnaryMinus(node, context);
                case AstConstants.Nodes.Expr_UnaryPlus:                 return Node_Expr_UnaryPlus(node, context);
                case AstConstants.Nodes.Expr_Variable:                  return Node_Expr_Variable(node, context);
                case AstConstants.Nodes.Expr_Yield:                     return Node_Expr_Yield(node, context);

                case AstConstants.Nodes.Scalar_DNumber:                 return Node_Scalar_DNumber(node, context);
                case AstConstants.Nodes.Scalar_Encapsed:                return Node_Scalar_Encapsed(node, context);
                case AstConstants.Nodes.Scalar_LNumber:                 return Node_Scalar_LNumber(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Class:        return Node_Scalar_MagicConst_Class(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Dir:          return Node_Scalar_MagicConst_Dir(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_File:         return Node_Scalar_MagicConst_File(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Function:     return Node_Scalar_MagicConst_Function(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Line:         return Node_Scalar_MagicConst_Line(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Method:       return Node_Scalar_MagicConst_Method(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Namespace:    return Node_Scalar_MagicConst_Namespace(node, context);
                case AstConstants.Nodes.Scalar_MagicConst_Trait:        return Node_Scalar_MagicConst_Trait(node, context);
                case AstConstants.Nodes.Scalar_String:                  return Node_Scalar_String(node, context);

                case AstConstants.Nodes.Arg:    return Node_Arg(node, context);
                case AstConstants.Nodes.Param:  return Node_Param(node, context);
                case AstConstants.Nodes.Const:  return Node_Const(node, context);
                case AstConstants.Nodes.Name:   return Node_Name(node, context);

                default:
                    throw new ArgumentException("Unknown AST node. Was: " + node.LocalName, "node");
            }
        }
        private T1 AnalyzeSubnode(XmlNode node, T2 context)
        {
            switch (node.LocalName)
            {
                case AstConstants.Subnodes.Adaptions     : return Subnode_Adaptions(node, context);
                case AstConstants.Subnodes.Args          : return Subnode_Args(node, context);
                case AstConstants.Subnodes.ByRef         : return Subnode_ByRef(node, context);
                case AstConstants.Subnodes.Cases         : return Subnode_Cases(node, context);
                case AstConstants.Subnodes.Catches       : return Subnode_Catches(node, context);
                case AstConstants.Subnodes.Class         : return Subnode_Class(node, context);
                case AstConstants.Subnodes.Cond          : return Subnode_Cond(node, context);
                case AstConstants.Subnodes.Consts        : return Subnode_Consts(node, context);
                case AstConstants.Subnodes.Declares      : return Subnode_Declares(node, context);
                case AstConstants.Subnodes.Default       : return Subnode_Default(node, context);
                case AstConstants.Subnodes.Dim           : return Subnode_Dim(node, context);
                case AstConstants.Subnodes.Else          : return Subnode_Else(node, context);
                case AstConstants.Subnodes.Extends       : return Subnode_Extends(node, context);
                case AstConstants.Subnodes.Expr          : return Subnode_Expr(node, context);
                case AstConstants.Subnodes.Exprs         : return Subnode_Exprs(node, context);
                case AstConstants.Subnodes.FinallyStmts  : return Subnode_FinallyStmts(node, context);
                case AstConstants.Subnodes.If            : return Subnode_If(node, context);
                case AstConstants.Subnodes.Implements    : return Subnode_Implements(node, context);
                case AstConstants.Subnodes.Init          : return Subnode_Init(node, context);
                case AstConstants.Subnodes.InsteadOf     : return Subnode_InsteadOf(node, context);
                case AstConstants.Subnodes.Items         : return Subnode_Items(node, context);
                case AstConstants.Subnodes.Key           : return Subnode_Key(node, context);
                case AstConstants.Subnodes.KeyVar        : return Subnode_KeyVar(node, context);
                case AstConstants.Subnodes.Left          : return Subnode_Left(node, context);
                case AstConstants.Subnodes.Loop          : return Subnode_Loop(node, context);
                case AstConstants.Subnodes.Method        : return Subnode_Method(node, context);
                case AstConstants.Subnodes.Name          : return Subnode_Name(node, context);
                case AstConstants.Subnodes.NewModifier   : return Subnode_NewModifier(node, context);
                case AstConstants.Subnodes.NewName       : return Subnode_NewName(node, context);
                case AstConstants.Subnodes.Num           : return Subnode_Num(node, context);
                case AstConstants.Subnodes.Params        : return Subnode_Params(node, context);
                case AstConstants.Subnodes.Parts         : return Subnode_Parts(node, context);
                case AstConstants.Subnodes.Props         : return Subnode_Props(node, context);
                case AstConstants.Subnodes.Remaining     : return Subnode_Remaining(node, context);
                case AstConstants.Subnodes.ReturnType    : return Subnode_ReturnType(node, context);
                case AstConstants.Subnodes.Right         : return Subnode_Right(node, context);
                case AstConstants.Subnodes.Static        : return Subnode_Static(node, context);
                case AstConstants.Subnodes.Stmts         : return Subnode_Stmts(node, context);
                case AstConstants.Subnodes.Trait         : return Subnode_Trait(node, context);
                case AstConstants.Subnodes.Traits        : return Subnode_Traits(node, context);
                case AstConstants.Subnodes.Type          : return Subnode_Type(node, context);
                case AstConstants.Subnodes.Unpack        : return Subnode_Unpack(node, context);
                case AstConstants.Subnodes.Uses          : return Subnode_Uses(node, context);
                case AstConstants.Subnodes.Var           : return Subnode_Var(node, context);
                case AstConstants.Subnodes.Variadic      : return Subnode_Variadic(node, context);
                case AstConstants.Subnodes.Vars          : return Subnode_Vars(node, context);
                case AstConstants.Subnodes.Value         : return Subnode_Value(node, context);
                case AstConstants.Subnodes.ValueVar      : return Subnode_ValueVar(node, context);
                default:
                    throw new ArgumentException("Unknown subnode. Was: " + node.LocalName, "node");
            }
        }
        private T1 AnalyzeScalar(XmlNode node, T2 context)
        {
            switch (node.LocalName)
            {
                case AstConstants.Scalars.Int: return Scalar_Int(node, context);
                case AstConstants.Scalars.String: return Scalar_String(node, context);
                case AstConstants.Scalars.Float: return Scalar_Float(node, context);
                case AstConstants.Scalars.Array: return Scalar_Array(node, context);
                case AstConstants.Scalars.False: return Scalar_False(node, context);
                case AstConstants.Scalars.True: return Scalar_True(node, context);
                case AstConstants.Scalars.Null: return Scalar_Null(node, context);
                default:
                    throw new ArgumentException("Unknown Scalar node. Was: " + node.LocalName, "node");
            }
        }
        private T1 AnalyzeAttribute(XmlNode node, T2 context)
        {
            switch (node.LocalName)
            {
                case AstConstants.Attributes.StartLine:
                    return Attribute_StartLine(node, context);
                case AstConstants.Attributes.EndLine:
                    return Attribute_EndLine(node, context);
                case AstConstants.Attributes.Comments:
                    return Attribute_Comments(node, context);
                default:
                    throw new ArgumentException("Unknown AST subnode. Was: " + node.LocalName, "node");
            }
        }

        #region Node methods
        protected virtual T1 Node_Stmt_Break(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Case(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Catch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_ClassConst(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_ClassLike(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Class(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_ClassMethod(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Const(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Continue(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Declare(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_DeclareDeclare(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Do(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Echo(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_ElseIf(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Else(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_For(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Foreach(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Function(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Global(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Goto(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_HaltCompiler(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_If(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_InlineHTML(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Interface(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Label(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Namespace(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Property(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_PropertyProperty(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Return(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_StaticVar(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Static(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Switch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Throw(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Trait(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_TraitUse(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_TraitUseAdaption(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Unset(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_Use(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_UseUse(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Stmt_While(XmlNode node, T2 context) { throw new NotImplementedException(); }

        protected virtual T1 Node_Expr_Array(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ArrayDimFetch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ArrayItem(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Assign(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_BitwiseAnd(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_BitwiseOr(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_BitwiseXor(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Concat(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Div(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Minus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Mod(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Plus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Mul(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_ShiftLeft(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_ShiftRight(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignOp_Pow(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_AssignRef(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BooleanNot(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BitwiseNot(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_BitwiseAnd(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_BitwiseOr(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_BitwiseXor(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_BooleanAnd(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_BooleanOr(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Concat(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Div(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Equal(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Greater(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_GreaterOrEqual(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Identical(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_LogicalAnd(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_LogicalOr(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_LogicalXor(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Minus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Mod(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Mul(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_NotEqual(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_NotIdentical(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Plus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Pow(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_ShiftLeft(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_ShiftRight(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_Smaller(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_BinaryOp_SmallerOrEqual(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Array(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Bool(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Double(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Int(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Object(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_String(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Cast_Unset(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ClassConstFetch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Closure(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ClosureUse(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ConstFetch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Empty(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ErrorSuppress(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Eval(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Exit(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_FuncCall(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Include(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Instanceof(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Isset(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_List(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_MethodCall(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_New(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_PostDec(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_PostInc(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_PreInc(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_PreDec(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Print(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_PropertyFetch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_ShellExec(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_StaticCall(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_StaticPropertyFetch(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Ternary(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_UnaryMinus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_UnaryPlus(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Variable(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Expr_Yield(XmlNode node, T2 context) { throw new NotImplementedException(); }

        protected virtual T1 Node_Scalar_DNumber(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_Encapsed(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_LNumber(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Class(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Dir(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_File(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Function(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Line(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Method(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Namespace(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_MagicConst_Trait(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Scalar_String(XmlNode node, T2 context) { throw new NotImplementedException(); }

        protected virtual T1 Node_Arg(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Param(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Const(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Node_Name(XmlNode node, T2 context) { throw new NotImplementedException(); }
        #endregion

        #region Subnode methods
        protected virtual T1 Subnode_Adaptions(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Args(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_ByRef(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Cases(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Catches(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Class(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Cond(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Consts(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Declares(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Default(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Dim(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Else(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Extends(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Expr(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Exprs(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_FinallyStmts(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_If(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Implements(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Init(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_InsteadOf(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Items(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Key(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_KeyVar(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Left(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Loop(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Method(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Name(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_NewModifier(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_NewName(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Num(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Params(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Parts(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Props(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Remaining(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_ReturnType(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Right(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Static(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Stmts(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Trait(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Traits(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Type(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Unpack(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Uses(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Var(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Variadic(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Vars(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_Value(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Subnode_ValueVar(XmlNode node, T2 context) { throw new NotImplementedException(); }
        #endregion

        #region Attribute methods
        protected virtual T1 Attribute_StartLine(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Attribute_EndLine(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Attribute_Comments(XmlNode node, T2 context) { throw new NotImplementedException(); }
        #endregion

        #region Scalar methods
        protected virtual T1 Scalar_Int(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_String(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_Float(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_Array(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_False(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_True(XmlNode node, T2 context) { throw new NotImplementedException(); }
        protected virtual T1 Scalar_Null(XmlNode node, T2 context) { throw new NotImplementedException(); }
        #endregion
    }
}
