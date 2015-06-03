using System;
using System.Xml;

namespace PHPAnalysis.Parsing.AstTraversing
{
    /// <summary>
    /// Event args used when visiting an individual node in the Xml tree.
    /// </summary>
    public class XmlTraverseEventArgs : EventArgs
    {
        public XmlNode Node { get; private set; }
        public XmlTraverseEventArgs(XmlNode node)
        {
            this.Node = node;
        }
    }

    /// <summary>
    /// Event args used right before the Xml traversal starts.
    /// </summary>
    public class XmlStartTraverseEventArgs : EventArgs
    {
        new public static readonly XmlStartTraverseEventArgs Empty = new XmlStartTraverseEventArgs();
    }

    /// <summary>
    /// Event args used after the Xml traversal is finished.
    /// </summary>
    public class XmlEndTraverseEventArgs : EventArgs
    {
        new public static readonly XmlEndTraverseEventArgs Empty = new XmlEndTraverseEventArgs();

    }
}