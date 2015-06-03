using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using PHPAnalysis.Data;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Tests.Utils.XmlHelpers
{
    [TestFixture]
    public class ScalarNodeTests
    {
        [TestCase(1.1),TestCase(123.1)]
        public void DValueResolving(double valueToTest)
        {
            var node = CreateDValueNodeWithValue(valueToTest);

            double result = ScalarNode.GetDValue(node);

            Assert.AreEqual(valueToTest, result);
        }

        private static XmlNode CreateDValueNodeWithValue(double value)
        {
            var doc = new XmlDocument();
            var node = doc.CreateNode(XmlNodeType.Element, AstConstants.Node, AstConstants.Nodes.Scalar_DNumber, "");
            var valueNode = doc.CreateNode(XmlNodeType.Element, AstConstants.Subnode, AstConstants.Subnodes.Value, "");
            var floatNode = doc.CreateNode(XmlNodeType.Element, AstConstants.Scalar, AstConstants.Scalars.Float, "");
            floatNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
            valueNode.AppendChild(floatNode);
            node.AppendChild(valueNode);
            return node;
        }
    }
}
