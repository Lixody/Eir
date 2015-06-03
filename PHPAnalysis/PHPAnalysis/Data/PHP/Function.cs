using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Data.PHP
{
    public class Function
    {
        public string Name { get; set; }
        public XmlNode AstNode { get; private set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string File { get; set; }
        public bool IsMagicMethod { get; private set; }

        public Dictionary<Tuple<uint,string>, Parameter> Parameters { get; set; }
        public List<string> Formats { get; set; }
        public int ParameterCount { get; private set; }
        public List<string> Aliases { get; set; }
        public string ReturnType { get; set; }

        private Function(bool magicMethod = false)
        {
            this.Parameters = new Dictionary<Tuple<uint, string>, Parameter>();
            this.IsMagicMethod = magicMethod;
            this.Aliases = new List<string>();
        }

        public Function(XmlNode node, bool magicMethod = false) : this(magicMethod)
        {
            Preconditions.NotNull(node, "node");
            this.AstNode = node;
        }

        public Function(JToken JSON, bool magicMethod = false) : this(magicMethod)
        {
            this.StartLine = Int32.MinValue;
            this.EndLine = Int32.MinValue;
            this.Formats = new List<string>();

            this.Name = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Name);
            this.ParameterCount = (int)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.ParameterCount);
            this.ReturnType = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.ReturnType);

            var formats = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Formats);
            if (formats != null)
            {
                foreach (string format in formats)
                {
                    this.Formats.Add(format);
                }
            }

            var aliasArray = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Aliases);
            if(aliasArray != null)
            {
                foreach (string alias in aliasArray)
                {
                    Aliases.Add(alias);
                }
            }
        }

        public Function(string json, bool magicMethod = false) : this(JObject.Parse(json), magicMethod)
        {
            //Should not do anything.
            //It's just practical to include both the string and the JToken ctor
        }

        public Function(string name, int startLine, int endLine, Dictionary<Tuple<uint,string>,Parameter> parameters, XmlNode ast)
        {
            this.Name = name;
            this.StartLine = startLine;
            this.EndLine = endLine;
            this.Parameters = parameters;
            this.AstNode = ast;
        }

        public XmlNode ExtractStatements()
        {
            return this.AstNode != null ? this.AstNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Stmts).FirstChild 
                                        : null;
        }

        public override string ToString()
        {
            const string indent = "    ";
            var sb = new StringBuilder("Function: " + this.Name);
            if (this.StartLine != Int32.MinValue && this.EndLine != Int32.MinValue)
            {
                sb.AppendLine(indent + "Start line: " + this.StartLine + " End line: " + this.EndLine);
            }
            sb.AppendLine(indent + "Magic method: " + this.IsMagicMethod);
            if (this.Parameters != null && this.Parameters.Any())
            {
                sb.AppendLine(indent + "Parameters defined: " + this.Parameters.Count);
            }
            sb.AppendLine(indent + "Total parameters: " + this.ParameterCount);
            if (this.Formats != null && this.Formats.Any())
            {
                sb.AppendLine(indent + "Known formats: " + this.Formats.Count);
            }
            if (this.Aliases != null && this.Aliases.Any())
            {
                sb.AppendLine(indent + "Known aliases: " + this.Aliases.Count);
            }
            sb.AppendLine(indent + "Return type: " + this.ReturnType);
            return sb.ToString();
        }
    }
}
