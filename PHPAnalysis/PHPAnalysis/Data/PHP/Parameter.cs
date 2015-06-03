using System.Xml;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.PHP
{
    public class Parameter
    {
        public XmlNode AstNode { get; private set; }
        public string Name { get; set; }
        public bool ByReference { get; set; }
        public bool IsVariadic { get; set; }
        public bool IsOptional { get; set; }
        public bool IsReturn { get; set; }
        public string DefaultValue { get; set;}
        /// <summary>
        /// This property indicates whether this parameter can create a security issue through the input.
        /// An example is the mysqli_query($dblink, $query). The dblink cannot create an issue, however the $query can.
        /// </summary>
        /// <value><c>true</c> if this instance can create a security issue; otherwise <c>false</c>.</value>
        public bool IsSensitive { get; set; }

        //public string DefaultValue
        //{
        //    get
        //    {
        //        var defaultNodeName = AstConstants.SubNode + ":" + AstConstants.Subnodes.Default;
        //        var val = AstNode.GetSubNode(defaultNodeName);
        //        return val.Value;
        //    }
        //}

        public Parameter(bool optional = false, bool vulnerable = true,
            bool isVariadic = false, bool byRef = false, string defaultValue = "", bool isReturn = false)
        {
            this.ByReference = byRef;
            this.IsVariadic = isVariadic;
            this.IsOptional = optional;
            this.IsSensitive = vulnerable;
            this.AstNode = null;
            this.DefaultValue = defaultValue;
            this.IsReturn = isReturn;
        }

        public Parameter(string name, bool optional = false, bool vulnerable = true, bool variadic = false,
            bool byRef = false, string defaultValue = "", bool isReturn = false) : this(optional, vulnerable, variadic, byRef, defaultValue, isReturn)
        {
            this.Name = name;
        }

        public Parameter(string name, XmlNode node, bool optional = false, bool vulnerable = true, bool variadic = false,
            bool byRef = false, string defaultValue = "", bool isReturn = false) : this(name, optional, vulnerable, variadic, byRef, defaultValue, isReturn)
        {
            Preconditions.NotNull(node, "node");
            this.AstNode = node;
        }

        public Parameter(XmlNode node)
        {
            //TODO: Could extract stuff from the param, and set the properties from the ast node
            //-||-: or the getters could be implemented to check if the property is null or empty and if it is, then it should extract and return
            Preconditions.NotNull(node, "node");
            this.AstNode = node;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}", 
                ByReference ? "&" : "", 
                IsVariadic ? "..." : "", 
                Name,
                IsOptional ? " = ": "");
        }
    }
}