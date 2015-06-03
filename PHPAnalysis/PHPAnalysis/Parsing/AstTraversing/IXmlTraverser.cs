using System;
using System.Xml;

namespace PHPAnalysis.Parsing.AstTraversing
{
    public interface IXmlTraverser
    {
        event EventHandler<XmlStartTraverseEventArgs> OnTraverseStart;
        event EventHandler<XmlTraverseEventArgs> OnEnteringNode;
        event EventHandler<XmlTraverseEventArgs> OnLeavingNode;
        event EventHandler<XmlEndTraverseEventArgs> OnTraverseEnd;

        void AddVisitor(IXmlVisitor visitor);

        void Traverse(XmlNode node);
    }
}