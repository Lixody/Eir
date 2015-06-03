using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;

namespace PHPAnalysis
{
    public class FunctionCall
    {
        public string Name { get; set; }
        public XmlNode ASTNode { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public IDictionary<uint,XmlNode> Arguments { get; set; }

        public FunctionCall(string name, XmlNode astNode, int start, int end)
        {
            this.Name = name;
            this.ASTNode = astNode;
            this.StartLine = start;
            this.EndLine = end;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Name, StartLine, EndLine);
        }
    }

    public class MethodCall : FunctionCall
    {
        public IList<string> ClassNames { get; set; }
        public Variable Var { get; set; }

        public MethodCall(string methodName, IList<string> className, XmlNode astNode, int start, int end, Variable variable = null)
            : base(methodName, astNode, start, end)
        {
            this.ClassNames = className;
            this.Var = variable;
        }

        public string CreateFullMethodName(string className)
        {
            // HACK - this.Name should NOT already have class name in it! 
            // It sometimes has because of the way we handle methodnames when extracting/putting into functionhandler.
            if (this.Name.Contains("->"))
            {
                return this.Name;
            }
            return className + "->" + this.Name;
        }

        public override string ToString()
        {
            return string.Format("[{0}] ->{1}", string.Join(",", ClassNames), base.ToString());
        }
    }
}

