namespace PHPAnalysis.Parsing.AstTraversing
{
    public interface IXmlVisitor
    {
        void TraverseStart(object sender, XmlStartTraverseEventArgs e);
        void EnteringNode(object sender, XmlTraverseEventArgs e);
        void LeavingNode(object sender, XmlTraverseEventArgs e);
        void TraverseEnd(object sender, XmlEndTraverseEventArgs e);
    }
}