using System.ComponentModel.Composition;
using PHPAnalysis.Data;
using PHPAnalysis.Parsing.AstTraversing;

namespace WordPress.Plugin
{
    [Export(typeof(IXmlVisitor))]
    public sealed class GotoCounter : IXmlVisitor
    {
        private int _gotos = 0;
        public void TraverseStart(object sender, XmlStartTraverseEventArgs e)
        {
        }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            if (e.Node.LocalName == AstConstants.Nodes.Stmt_Goto)
            {
                _gotos++;
            }
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e)
        {
        }

        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e)
        {
            //System.IO.File.AppendAllText("john.txt", "GOTOs: " + _gotos);

        }
    }
}