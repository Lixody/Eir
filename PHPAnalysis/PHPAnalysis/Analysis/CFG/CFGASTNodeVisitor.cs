using System;
using System.Collections.Generic;
using System.Xml;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing.AstTraversing;

namespace PHPAnalysis.Analysis.CFG
{
    class CFGASTNodeVisitor : IXmlVisitor
    {
        public List<XmlNode> NodesOfInterest;
        public void TraverseStart(object sender, XmlStartTraverseEventArgs e)
        {
            NodesOfInterest = new List<XmlNode>();
        }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            switch (node.LocalName)
            {
                case AstConstants.Nodes.Expr_Assign:
                    NodesOfInterest.Add(node);
                    break;
                case AstConstants.Nodes.Expr_AssignOp_BitwiseAnd:
                case AstConstants.Nodes.Expr_AssignOp_BitwiseOr:
                case AstConstants.Nodes.Expr_AssignOp_BitwiseXor:
                case AstConstants.Nodes.Expr_AssignOp_Concat:
                case AstConstants.Nodes.Expr_AssignOp_Div:
                case AstConstants.Nodes.Expr_AssignOp_Minus:
                case AstConstants.Nodes.Expr_AssignOp_Mod:
                case AstConstants.Nodes.Expr_AssignOp_Mul:
                case AstConstants.Nodes.Expr_AssignOp_Plus:
                case AstConstants.Nodes.Expr_AssignOp_Pow:
                case AstConstants.Nodes.Expr_AssignOp_ShiftLeft:
                case AstConstants.Nodes.Expr_AssignOp_ShiftRight:
                case AstConstants.Nodes.Expr_AssignRef:
                    break;
                case AstConstants.Nodes.Stmt_Echo:
                    break;
                case AstConstants.Nodes.Expr_ShellExec:
                    break;
                case AstConstants.Nodes.Expr_Eval:
                    break;
            }
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e)
        {

        }

        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e)
        {
        }
    }
}
