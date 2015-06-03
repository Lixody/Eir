using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Parsing.AstTraversing
{
    /// <summary>
    /// General purpose Xml structuer traverser. 
    /// </summary>
    public sealed class XmlTraverser : IXmlTraverser
    {
        /// <summary>
        /// Invoket right before the actual Xml traversal starts.
        /// </summary>
        public event EventHandler<XmlStartTraverseEventArgs> OnTraverseStart;
        /// <summary>
        /// Invoked when entering an Xml node.
        /// </summary>
        public event EventHandler<XmlTraverseEventArgs> OnEnteringNode;
        /// <summary>
        /// Invoked when leaving an Xml node.
        /// </summary>
        public event EventHandler<XmlTraverseEventArgs> OnLeavingNode;
        /// <summary>
        /// Invoked when the Xml traversal is done.
        /// </summary>
        public event EventHandler<XmlEndTraverseEventArgs> OnTraverseEnd;

        /// <summary>
        /// Adds XmlVisitor to the traverser.
        /// </summary>
        public void AddVisitor(IXmlVisitor visitor)
        {
            Preconditions.NotNull(visitor, "visitor");

            this.OnTraverseStart += visitor.TraverseStart;
            this.OnEnteringNode += visitor.EnteringNode;
            this.OnLeavingNode += visitor.LeavingNode;
            this.OnTraverseEnd += visitor.TraverseEnd;
        }

        public void AddVisitors(params IXmlVisitor[] visitors)
        {
            foreach (var xmlVisitor in visitors)
            {
                AddVisitor(xmlVisitor);
            }
        }

        /// <summary>
        /// Starts the traversal of the xml tree.
        /// </summary>
        public void Traverse(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            OnTraverseStart.RaiseEvent(this, XmlStartTraverseEventArgs.Empty);
            DepthFirstImpl(node);
            OnTraverseEnd.RaiseEvent(this, XmlEndTraverseEventArgs.Empty);
        }

        private void DepthFirstImpl(XmlNode node)
        {
            OnEnteringNode.RaiseEvent(this, new XmlTraverseEventArgs(node));

            foreach (XmlNode childNode in node.ChildNodes)
            {
                DepthFirstImpl(childNode);
            }

            OnLeavingNode.RaiseEvent(this, new XmlTraverseEventArgs(node));
        }

        private void BreadthFirst(XmlNode root)
        {
            Preconditions.NotNull(root, "root");

            var queue = new Queue<XmlNode>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var node = queue.Dequeue();
                OnEnteringNode.RaiseEvent(this, new XmlTraverseEventArgs(node));

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    queue.Enqueue(childNode);
                }
                
                OnLeavingNode.RaiseEvent(this, new XmlTraverseEventArgs(node));
            }
        }
    }
}
