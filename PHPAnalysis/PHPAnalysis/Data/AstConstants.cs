using System;
using System.Diagnostics.CodeAnalysis;

namespace PHPAnalysis.Data
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class AstConstants
    {
        public const string Node = "node";
        public const string Subnode = "subNode";
        public const string Attribute = "attribute";
        public const string Scalar = "scalar";

        public static class Namespaces
        {
            public const string Node = "http://nikic.github.com/PHPParser/XML/node";
            public const string SubNode = "http://nikic.github.com/PHPParser/XML/subNode";
            public const string Scalar = "http://nikic.github.com/PHPParser/XML/scalar";
            public const string Attribute = "http://nikic.github.com/PHPParser/XML/attribute";
        }

        public static class Nodes
        {
            public const string Stmt_Break              = "Stmt_Break";
            public const string Stmt_Case               = "Stmt_Case";
            public const string Stmt_Catch              = "Stmt_Catch";
            public const string Stmt_ClassConst         = "Stmt_ClassConst";
            public const string Stmt_ClassLike          = "Stmt_ClassLike";
            public const string Stmt_Class              = "Stmt_Class";
            public const string Stmt_ClassMethod        = "Stmt_ClassMethod";
            public const string Stmt_Const              = "Stmt_Const";
            public const string Stmt_Continue           = "Stmt_Continue";
            public const string Stmt_Declare            = "Stmt_Declare";
            public const string Stmt_DeclareDeclare     = "Stmt_DeclareDeclare";
            public const string Stmt_Do                 = "Stmt_Do";
            public const string Stmt_Echo               = "Stmt_Echo";
            public const string Stmt_ElseIf             = "Stmt_ElseIf";
            public const string Stmt_Else               = "Stmt_Else";
            public const string Stmt_For                = "Stmt_For";
            public const string Stmt_Foreach            = "Stmt_Foreach";
            public const string Stmt_Function           = "Stmt_Function";
            public const string Stmt_Global             = "Stmt_Global";
            public const string Stmt_Goto               = "Stmt_Goto";
            public const string Stmt_HaltCompiler       = "Stmt_HaltCompiler";
            public const string Stmt_If                 = "Stmt_If";
            public const string Stmt_InlineHTML         = "Stmt_InlineHTML";
            public const string Stmt_Interface          = "Stmt_Interface";
            public const string Stmt_Label              = "Stmt_Label";
            public const string Stmt_Namespace          = "Stmt_Namespace";
            public const string Stmt_Property           = "Stmt_Property";
            public const string Stmt_PropertyProperty   = "Stmt_PropertyProperty";
            public const string Stmt_Return             = "Stmt_Return";
            public const string Stmt_StaticVar          = "Stmt_StaticVar";
            public const string Stmt_Static             = "Stmt_Static";
            public const string Stmt_Switch             = "Stmt_Switch";
            public const string Stmt_Throw              = "Stmt_Throw";
            public const string Stmt_Trait              = "Stmt_Trait";
            public const string Stmt_TraitUse           = "Stmt_TraitUse";
            public const string Stmt_TraitUseAdaption   = "Stmt_TraitUseAdaption";
            public const string Stmt_Unset              = "Stmt_Unset";
            public const string Stmt_Use                = "Stmt_Use";
            public const string Stmt_UseUse             = "Stmt_UseUse";
            public const string Stmt_While              = "Stmt_While";

            public const string Expr_Array = "Expr_Array";
            public const string Expr_ArrayDimFetch = "Expr_ArrayDimFetch";
            public const string Expr_ArrayItem = "Expr_ArrayItem";
            public const string Expr_Assign = "Expr_Assign";
            public const string Expr_AssignOp_BitwiseAnd = "Expr_AssignOp_BitwiseAnd";
            public const string Expr_AssignOp_BitwiseOr = "Expr_AssignOp_BitwiseOr";
            public const string Expr_AssignOp_BitwiseXor = "Expr_AssignOp_BitwiseXor";
            public const string Expr_AssignOp_Concat = "Expr_AssignOp_Concat";
            public const string Expr_AssignOp_Div = "Expr_AssignOp_Div";
            public const string Expr_AssignOp_Minus = "Expr_AssignOp_Minus";
            public const string Expr_AssignOp_Mod = "Expr_AssignOp_Mod";
            public const string Expr_AssignOp_Plus = "Expr_AssignOp_Plus";
            public const string Expr_AssignOp_Mul = "Expr_AssignOp_Mul";
            public const string Expr_AssignOp_ShiftLeft = "Expr_AssignOp_ShiftLeft";
            public const string Expr_AssignOp_ShiftRight = "Expr_AssignOp_ShiftRight";
            public const string Expr_AssignOp_Pow = "Expr_AssignOp_Pow";
            public const string Expr_AssignRef = "Expr_AssignRef";
            public const string Expr_BooleanNot = "Expr_BooleanNot";
            public const string Expr_BitwiseNot = "Expr_BitwiseNot";
            public const string Expr_BinaryOp_BitwiseAnd = "Expr_BinaryOp_BitwiseAnd";
            public const string Expr_BinaryOp_BitwiseOr = "Expr_BinaryOp_BitwiseOr";
            public const string Expr_BinaryOp_BitwiseXor = "Expr_BinaryOp_BitwiseXor";
            public const string Expr_BinaryOp_BooleanAnd = "Expr_BinaryOp_BooleanAnd";
            public const string Expr_BinaryOp_BooleanOr = "Expr_BinaryOp_BooleanOr";
            public const string Expr_BinaryOp_Concat = "Expr_BinaryOp_Concat";
            public const string Expr_BinaryOp_Div = "Expr_BinaryOp_Div";
            public const string Expr_BinaryOp_Equal = "Expr_BinaryOp_Equal";                    // ==
            public const string Expr_BinaryOp_Greater = "Expr_BinaryOp_Greater";
            public const string Expr_BinaryOp_GreaterOrEqual = "Expr_BinaryOp_GreaterOrEqual";
            public const string Expr_BinaryOp_Identical = "Expr_BinaryOp_Identical";            // ===
            public const string Expr_BinaryOp_LogicalAnd = "Expr_BinaryOp_LogicalAnd";
            public const string Expr_BinaryOp_LogicalOr = "Expr_BinaryOp_LogicalOr";
            public const string Expr_BinaryOp_LogicalXor = "Expr_BinaryOp_LogicalXor";
            public const string Expr_BinaryOp_Minus = "Expr_BinaryOp_Minus";
            public const string Expr_BinaryOp_Mod = "Expr_BinaryOp_Mod";
            public const string Expr_BinaryOp_Mul = "Expr_BinaryOp_Mul";
            public const string Expr_BinaryOp_NotEqual = "Expr_BinaryOp_NotEqual";
            public const string Expr_BinaryOp_NotIdentical = "Expr_BinaryOp_NotIdentical";
            public const string Expr_BinaryOp_Plus = "Expr_BinaryOp_Plus";
            public const string Expr_BinaryOp_Pow = "Expr_BinaryOp_Pow";
            public const string Expr_BinaryOp_ShiftLeft = "Expr_BinaryOp_ShiftLeft";
            public const string Expr_BinaryOp_ShiftRight = "Expr_BinaryOp_ShiftRight";
            public const string Expr_BinaryOp_Smaller = "Expr_BinaryOp_Smaller";
            public const string Expr_BinaryOp_SmallerOrEqual = "Expr_BinaryOp_SmallerOrEqual";
            public const string Expr_Cast_Array     = "Expr_Cast_Array";
            public const string Expr_Cast_Bool      = "Expr_Cast_Bool";
            public const string Expr_Cast_Double    = "Expr_Cast_Double";
            public const string Expr_Cast_Int       = "Expr_Cast_Int";
            public const string Expr_Cast_Object    = "Expr_Cast_Object";
            public const string Expr_Cast_String    = "Expr_Cast_String";
            public const string Expr_Cast_Unset     = "Expr_Cast_Unset";
            public const string Expr_ClassConstFetch = "Expr_ClassConstFetch";
            public const string Expr_Closure = "Expr_Closure";
            public const string Expr_ClosureUse = "Expr_ClosureUse";
            public const string Expr_ConstFetch = "Expr_ConstFetch";
            public const string Expr_Empty = "Expr_Empty";
            public const string Expr_ErrorSuppress = "Expr_ErrorSuppress";
            public const string Expr_Eval = "Expr_Eval";
            public const string Expr_Exit = "Expr_Exit";
            public const string Expr_FuncCall = "Expr_FuncCall";
            public const string Expr_Include = "Expr_Include";
            public const string Expr_Instanceof = "Expr_Instanceof";
            public const string Expr_Isset = "Expr_Isset";
            public const string Expr_List = "Expr_List";
            public const string Expr_MethodCall = "Expr_MethodCall";
            public const string Expr_New = "Expr_New";
            public const string Expr_PostDec = "Expr_PostDec";
            public const string Expr_PostInc = "Expr_PostInc";
            public const string Expr_PreInc = "Expr_PreInc";
            public const string Expr_PreDec = "Expr_PreDec";
            public const string Expr_Print = "Expr_Print";
            public const string Expr_PropertyFetch = "Expr_PropertyFetch";
            public const string Expr_ShellExec = "Expr_ShellExec";
            public const string Expr_StaticCall = "Expr_StaticCall";
            public const string Expr_StaticPropertyFetch = "Expr_StaticPropertyFetch";
            public const string Expr_Ternary = "Expr_Ternary";
            public const string Expr_UnaryMinus = "Expr_UnaryMinus";
            public const string Expr_UnaryPlus = "Expr_UnaryPlus";
            public const string Expr_Variable = "Expr_Variable";
            public const string Expr_Yield = "Expr_Yield";

            public const string Scalar_DNumber              = "Scalar_DNumber";
            public const string Scalar_Encapsed             = "Scalar_Encapsed";
            public const string Scalar_LNumber              = "Scalar_LNumber";
            public const string Scalar_MagicConst_Class     = "Scalar_MagicConst_Class";
            public const string Scalar_MagicConst_Dir       = "Scalar_MagicConst_Dir";
            public const string Scalar_MagicConst_File      = "Scalar_MagicConst_File";
            public const string Scalar_MagicConst_Function  = "Scalar_MagicConst_Function";
            public const string Scalar_MagicConst_Line      = "Scalar_MagicConst_Line";
            public const string Scalar_MagicConst_Method    = "Scalar_MagicConst_Method";
            public const string Scalar_MagicConst_Namespace = "Scalar_MagicConst_Namespace";
            public const string Scalar_MagicConst_Trait     = "Scalar_MagicConst_Trait";
            public const string Scalar_String               = "Scalar_String";

            public const string Arg     = "Arg";              // Function call argument
            public const string Param   = "Param";            // Function/Method parameter
            public const string Const   = "Const";
            public const string Name    = "Name";
        }

        public static class Attributes
        {
            public const string StartLine   = "startLine";
            public const string EndLine     = "endLine";
            public const string Comments    = "comments";
        }

        public static class Subnodes
        {
            public const string Adaptions    = "adaptions";     // Trait usage
            public const string Args         = "args";             
            public const string ByRef        = "byRef";         // Parameter byref
            public const string Cases        = "cases";           
            public const string Catches      = "catches";       
            public const string Class        = "class";         // instanceof, new *class*, *class*::, ..
            public const string Cond         = "cond";             
            public const string Consts       = "consts";         
            public const string Declares     = "declares";     
            public const string Default      = "default";       
            public const string Dim          = "dim";           // Array dimension
            public const string Else         = "else";          // Ternary op
            public const string Extends      = "extends";
            public const string Expr         = "expr";
            public const string Exprs        = "exprs";
            public const string FinallyStmts = "finallyStmts";
            public const string If           = "if";            // Ternary op
            public const string Implements   = "implements";
            public const string Init         = "init";
            public const string InsteadOf    = "insteadof"; 
            public const string Items        = "items";         // Array items
            public const string Key          = "key";           // Yield
            public const string KeyVar       = "keyVar";
            public const string Left         = "left";          // Binary expression left operand
            public const string Loop         = "loop";          // Used in for-loops
            public const string Method       = "method";         
            public const string Name         = "name";          // Names (functions, classes, variables, ..)
            public const string NewModifier  = "newModifier";
            public const string NewName      = "newName"; 
            public const string Num          = "num";           // Break, Continue
            public const string Params       = "params";
            public const string Parts        = "parts";
            public const string Props        = "props";
            public const string Remaining    = "remaining";     // HaltCompiler
            public const string ReturnType   = "returnType";    // Closures
            public const string Right        = "right";         // Binary expression right element
            public const string Static       = "static";         
            public const string Stmts        = "stmts";           
            public const string Trait        = "trait";           
            public const string Traits       = "traits";         
            public const string Type         = "type";             
            public const string Unpack       = "unpack";         
            public const string Uses         = "uses";           
            public const string Var          = "var";           // ++, --, etc
            public const string Variadic     = "variadic";      // Parameter variadic
            public const string Vars         = "vars";          // Isset
            public const string Value        = "value";           
            public const string ValueVar     = "valueVar";      // foreach

        }

        public static class Scalars
        {
            public const string Int     = "int";
            public const string String  = "string";
            public const string Float   = "float";
            public const string Array   = "array";
            public const string False   = "false";
            public const string True    = "true";
            public const string Null    = "null";
        }

        public enum IncludeTypes
        {
            // https://github.com/nikic/PHP-Parser/blob/a2d7e8977a406232b3a3396cae5925d8f26eadbe/lib/PhpParser/Node/Expr/Include_.php
            Include = 1, IncludeOnce = 2, Require = 3, RequireOnce = 4
        }

        [Flags]
        public enum VisibilityModifiers
        {
            // https://github.com/nikic/PHP-Parser/blob/d56ff5a351d43cb4d26589f2aff878ae7cf01ff1/lib/PhpParser/Node/Stmt/Class_.php
            Public = 1, Protected = 2, Private = 4, Static = 8, Abstract = 16, Final = 32
        }
    }
}
