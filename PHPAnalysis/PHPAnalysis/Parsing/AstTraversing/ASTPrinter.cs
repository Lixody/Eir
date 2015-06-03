using System.IO;
using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Parsing.AstTraversing
{
    public sealed class ASTPrinter : IXmlVisitor
    {
        private readonly TextWriter writer;

        private int indentation;

        public ASTPrinter(TextWriter output)
        {
            Preconditions.NotNull(output, "output");

            this.writer = output;
        }


        public void TraverseStart(object sender, XmlStartTraverseEventArgs e)
        {
        }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            var toPrint = string.Format("{0}{1}:{2}:{3} - {4}", new string(' ', indentation), node.NodeType, node.Prefix, node.LocalName, node.Value);
            writer.WriteLine(toPrint);
            indentation++;
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            indentation--;
            var toPrint = string.Format("{0}{1}:{2}:{3} - {4}", new string(' ', indentation), node.NodeType, node.Prefix, node.LocalName, node.Value);
            writer.WriteLine(toPrint);
        }

        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e)
        {
        }
    }
}
