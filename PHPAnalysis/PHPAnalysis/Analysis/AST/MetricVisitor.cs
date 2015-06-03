using System;
using System.Xml;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing.AstTraversing;

namespace PHPAnalysis.Analysis.AST
{
    public class MetricVisitor : IXmlVisitor
    {
        public int TotalNodes { get; private set; }
        public int EchoStatements { get; private set; }
        public int PotentialSQLQueries { get; private set; }
        public int IncludeStatements { get; private set; }
        public int Classes { get; private set; }
        public int Methods { get; private set; }
        public int Functions { get; private set; }
        public int Gotos { get; private set; }

        public void TraverseStart(object sender, XmlStartTraverseEventArgs e) { }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            XmlNode node = e.Node;
            TotalNodes++;
            switch (node.LocalName)
            {
                case "Stmt_Echo":
                    EchoStatements++;
                    break;
                case "#text":
                    StringLiteral(node);
                    break;
                case AstConstants.Nodes.Stmt_Function:
                    Functions++;
                    break;
                case AstConstants.Nodes.Stmt_ClassMethod:
                    Methods++;
                    break;
                case AstConstants.Nodes.Stmt_Class:
                    Classes++;
                    break;
                case "Expr_Include":
                    IncludeStatements++;
                    break;
                case AstConstants.Nodes.Stmt_Goto:
                    Gotos++;
                    break;
            }
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e) {  }

        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e) { }

        private void StringLiteral(XmlNode node)
        {
            // Poor query recognizer.. But whatevs
            if (node.InnerText.StartsWith("select ", StringComparison.OrdinalIgnoreCase) ||
                node.InnerText.StartsWith("update ", StringComparison.OrdinalIgnoreCase) ||
                node.InnerText.StartsWith("delete ", StringComparison.OrdinalIgnoreCase))
            {
                PotentialSQLQueries++;
            }
        }
    }
}