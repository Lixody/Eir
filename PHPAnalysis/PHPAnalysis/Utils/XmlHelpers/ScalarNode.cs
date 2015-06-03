using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils.XmlHelpers
{
    public static class ScalarNode
    {
        public static string GetStringValue(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_String, "String value retrieval only supported for " + AstConstants.Nodes.Scalar_String + " nodes. Was " + node.Name, "node");

            return node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value)
                       .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String).InnerText;
        }

        public static int GetLValue(XmlNode node)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_LNumber,  "LValue retrieval only supported for " + AstConstants.Nodes.Scalar_LNumber + " nodes. Was " + node.Name, "node");

            var innerText = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value)
                .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Int).InnerText;

            return int.Parse(innerText);
        }

        public static double GetDValue(XmlNode node)
        {
            Preconditions.NotNull(node, "Node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Scalar_DNumber, "DValue retrieval only supported for " + AstConstants.Nodes.Scalar_DNumber + " nodes. Was " + node.Name, "node");

            var innerText = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value)
                                .GetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Float).InnerText;

            return double.Parse(innerText, CultureInfo.InvariantCulture);
        }
    }
}
