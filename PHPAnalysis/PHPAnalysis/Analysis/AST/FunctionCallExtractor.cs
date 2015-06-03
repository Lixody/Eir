using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Utils.XmlHelpers;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.AST
{
    public class FunctionCallExtractor
    {
        public FunctionCall ExtractFunctionCall(XmlNode node)
        {
            int startLine = AstNode.GetStartLine(node);
            int endLine = AstNode.GetEndLine(node);
            string funcName = "";
            XmlNode nameSubNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);
            XmlNode nameNode = null;

            bool success = nameSubNode.TryGetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Name, out nameNode);
            if (success)
            {
                funcName = nameNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Parts).InnerText;
            }
            
            return new FunctionCall(funcName, node, startLine, endLine) { Arguments = ExtractArgumentNodes(node) };
        }

        public MethodCall ExtractMethodCall(XmlNode node, IVariableStorage varStorage, AnalysisScope scope = AnalysisScope.File)
        {
            int startLine = AstNode.GetStartLine(node);
            int endLine = AstNode.GetEndLine(node);
            string methodName = "";

            //Get the varNode which includes either (new ClassName())->MethodName or $var->MethodName
            var varNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            var classNames = new List<string>();

            if (varNode.FirstChild.LocalName == AstConstants.Nodes.Expr_New)
            {
                //PHP: (new ClassName(args))->MethodName(args);
                //Extract the ClassName directly, in this case there can be only one ClassName!
                var className = varNode.FirstChild
                    .GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Class)
                    .GetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Name)
                    .GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Parts).FirstChild.FirstChild.InnerText;
                classNames.Add(className);

                methodName = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name).InnerText;
                return new MethodCall(methodName, classNames, node, startLine, endLine) { Arguments = ExtractArgumentNodes(node) };
            }
            else
            {
                //PHP: $var->MethodName(args);
                //Resolve the variable, and get all the possible class names!
                VariableResolver vr = new VariableResolver(varStorage, scope);
                VariableResolveResult variableResult = null;
                if (vr.IsResolvableNode(varNode.FirstChild))
                {
                    variableResult = vr.ResolveVariable(varNode.FirstChild);
                    classNames.AddRange(variableResult.Variable.Info.ClassNames.Where(className => !classNames.Contains(className)));
                }

                var nameSubnode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);
                XmlNode nameNode = null;
                bool success = nameSubnode.TryGetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Name, out nameNode);
                if (success)
                {
                    methodName = nameNode.InnerText;
                }
                else
                {
                    if (nameSubnode.FirstChild.LocalName == AstConstants.Scalars.String)
                    {
                        methodName = nameSubnode.FirstChild.InnerText;
                    }
                }

                return variableResult == null ?
                    new MethodCall(methodName, classNames, node, startLine, endLine) { Arguments = ExtractArgumentNodes(node) } :
                    new MethodCall(methodName, classNames, node, startLine, endLine, variableResult.Variable) { Arguments = ExtractArgumentNodes(node) };
            }
        }

        /// <summary>
        /// Method to extract each XmlNode argument node and include it in the common arguments dictionary with index
        /// </summary>
        /// <param name="node">The Function call or Method call to extract arguments from</param>
        /// <returns>The dictionary of arguments</returns>
        private IDictionary<uint, XmlNode> ExtractArgumentNodes(XmlNode node)
        {
            //TODO - HACK: Find out if there is a good way to include the arguments!
            // ----||----: Right now we are analyzing them here and in the taintblockanalyzer, which is stupid!
            var argumentNodes = new Dictionary<uint, XmlNode>();

            const string XpathSelector = "./node()[local-name()='" + AstConstants.Nodes.Arg + "']";
            XmlNodeList arguments = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Args).FirstChild.SelectNodes(XpathSelector);

            //Actually extract the arguments
            for (uint index = 1; index <= arguments.Count; index++)
            {
                var item = arguments[(int)index - 1];
                var valueNode = item.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value).FirstChild;
                argumentNodes.Add(index, valueNode);
            }

            return argumentNodes;
        }
    }
}