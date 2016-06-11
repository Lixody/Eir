using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using PHPAnalysis.Data;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Analysis.AST
{
    // TODO: Func/Method extract: Default values for parameters: Can be Arrays, NULL or any constant expression!
    //       Note: Default arguments have to be on the right side of any non-default arguments.
    // TODO: Method extract: Visibility modifiers (public, private, ..). Default is public.
    // TODO: Class extract: Detect superclass/interfaces.
    // TODO: Possibly rename this class.. 
    //
    // Let's ignore Traits for now..

    public sealed class ClassAndFunctionExtractor : IXmlVisitor
    {
        private enum FunctionClosure { Function, Closure }

        private readonly List<Function> _functions;
        public IReadOnlyCollection<Function> Functions
        {
            get { return new ReadOnlyCollection<Function>(_functions); }
        }

        private readonly List<Interface> _interfaces;
        public IReadOnlyCollection<Interface> Interfaces
        {
            get { return new ReadOnlyCollection<Interface>(_interfaces); }
        }  

        private readonly List<Class> _classes;
        public IReadOnlyCollection<Class> Classes
        {
            get { return new ReadOnlyCollection<Class>(_classes); }
        }

        private readonly List<Closure> _closures;

        public IReadOnlyCollection<Closure> Closures
        {
            get { return new ReadOnlyCollection<Closure>(_closures); }
        }  

        private Interface currentInterface;
        private Class currentClass;
        private readonly Stack<Function> currentFunctions = new Stack<Function>();
        private readonly Stack<ExtractorClosureInfo> currentClosures = new Stack<ExtractorClosureInfo>(); 
        private readonly Stack<FunctionClosure> functionClosureOrder = new Stack<FunctionClosure>(); 
        private bool inParamDeclarations = false;

        private Property currentProperty;
        private bool inPropertyDeclarations = false;
        private AstConstants.VisibilityModifiers propertyModifiers;

        private readonly Queue<Parameter> parameters = new Queue<Parameter>();
        private Parameter currentParameter;
        private bool parameterByRefIsSet = false;
        private bool parameteVariadicIsSet = false;
        private bool parameterDefaultIsSet = false;

        private ClosureUse currentClosureUse;

        public ClassAndFunctionExtractor()
        {
            _functions = new List<Function>();
            _classes = new List<Class>();
            _interfaces = new List<Interface>();
            _closures = new List<Closure>();
        }

        public void TraverseStart(object sender, XmlStartTraverseEventArgs e) { }
        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e) { }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            switch (node.LocalName)
            {
                // Nodes
                case AstConstants.Nodes.Stmt_Interface:
                    currentInterface = new Interface(node)
                    {
                        StartLine = AstNode.GetStartLine(node),
                        EndLine = AstNode.GetEndLine(node)
                    };
                    break;
                case AstConstants.Nodes.Stmt_Class:
                    currentClass = new Class(node)
                    {
                        StartLine = AstNode.GetStartLine(node),
                        EndLine = AstNode.GetEndLine(node)
                    };
                    break;
                case AstConstants.Nodes.Stmt_ClassMethod:
                    ClassMethodEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Property:
                    inPropertyDeclarations = true;
                    break;
                case AstConstants.Nodes.Stmt_PropertyProperty:
                    currentProperty = new Property(node)
                    {
                        StartLine = AstNode.GetStartLine(node),
                        EndLine = AstNode.GetEndLine(node),
                        VisibilityModifiers = propertyModifiers,
                    };
                    break;
                case AstConstants.Nodes.Stmt_Function:
                    currentFunctions.Push(new Function(node)
                    {
                        StartLine = AstNode.GetStartLine(node),
                        EndLine = AstNode.GetEndLine(node)
                    });
                    functionClosureOrder.Push(FunctionClosure.Function);
                    break;
                case AstConstants.Nodes.Expr_Closure:
                    currentClosures.Push(new ExtractorClosureInfo()
                    {
                        Closure = new Closure(node)
                                  {
                                      StartLine = AstNode.GetStartLine(node),
                                      EndLine = AstNode.GetEndLine(node)
                                  }
                    });
                    functionClosureOrder.Push(FunctionClosure.Closure);
                    break;
                case AstConstants.Nodes.Param:
                    currentParameter = new Parameter(node);
                    break;
                case AstConstants.Nodes.Expr_ClosureUse:
                    currentClosureUse = new ClosureUse(node);
                    break;
                // Subnodes
                case AstConstants.Subnodes.Name:
                    Name(node);
                    break;
                case AstConstants.Subnodes.Params:
                    inParamDeclarations = true;
                    break;
                case AstConstants.Subnodes.ByRef:
                    Subnode_ByRef_Enter(node);
                    break;
                case AstConstants.Subnodes.Variadic:
                    if (inParamDeclarations && !parameteVariadicIsSet)
                    {
                        currentParameter.IsVariadic = Convert.ToBoolean(node.FirstChild.LocalName);
                        parameteVariadicIsSet = true;
                    }
                    break;
                case AstConstants.Subnodes.Default:
                    if (inParamDeclarations && !parameterDefaultIsSet)
                    {
                        currentParameter.IsOptional = node.FirstChild.Prefix != AstConstants.Scalar;
                        parameterDefaultIsSet = true;
                    }
                    break;
                case AstConstants.Subnodes.Uses:
                    if (currentClosures.Any() && currentClosures.Peek().ClosureUses == null && !currentClosures.Peek().ClosureUsagesIsSet)
                    {
                        currentClosures.Peek().ClosureUses = new Queue<ClosureUse>();
                    }
                    break;
                case AstConstants.Subnodes.Var:
                    if (currentClosureUse != null)
                    {
                        currentClosureUse.Name = node.InnerText;
                    }
                    break;
                case AstConstants.Subnodes.Type:
                    if (inPropertyDeclarations && currentProperty == null)
                    {
                        var visibilityModifiers = (AstConstants.VisibilityModifiers) Enum.Parse(typeof (AstConstants.VisibilityModifiers), node.InnerText);
                        // No modifier in source code = 0, if no modifier is present the default visibility is public
                        propertyModifiers = visibilityModifiers == 0 ? AstConstants.VisibilityModifiers.Public : visibilityModifiers;
                    }
                    break;
            }
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            switch (node.LocalName)
            {
                // Nodes
                case AstConstants.Nodes.Stmt_Interface:
                    _interfaces.Add(currentInterface);
                    currentInterface = null;
                    break;
                case AstConstants.Nodes.Stmt_Class:
                    _classes.Add(currentClass);
                    currentClass = null;
                    break;
                case AstConstants.Nodes.Stmt_ClassMethod:
                    ClassMethodExit();
                    break;
                case AstConstants.Nodes.Stmt_Property:
                    inPropertyDeclarations = false;
                    propertyModifiers = 0;
                    break;
                case AstConstants.Nodes.Stmt_PropertyProperty:
                    currentClass.Properties.Add(currentProperty);
                    currentProperty = null;
                    break;
                case AstConstants.Nodes.Stmt_Function:
                    _functions.Add(currentFunctions.Pop());
                    functionClosureOrder.Pop();
                    break;
                case AstConstants.Nodes.Expr_ClosureUse:
                    currentClosures.Peek().ClosureUses.Enqueue(currentClosureUse);
                    currentClosureUse = null;
                    break;
                case AstConstants.Nodes.Expr_Closure:
                    _closures.Add(currentClosures.Pop().Closure);
                    functionClosureOrder.Pop();
                    break;
                case AstConstants.Nodes.Param:
                    parameters.Enqueue(currentParameter);
                    currentParameter = null;
                    parameteVariadicIsSet = false;
                    parameterByRefIsSet = false;
                    parameterDefaultIsSet = false;
                    break;
                // Subnodes
                case AstConstants.Subnodes.Params:
                    ParamsExit();
                    break;
                case AstConstants.Subnodes.Uses:
                    if (currentClosures.Any())
                    {
                        ExtractorClosureInfo closureInfo = currentClosures.Peek();
                        if (!closureInfo.ClosureUsagesIsSet && closureInfo.ClosureUses != null)
                        {
                            closureInfo.Closure.UseParameters = closureInfo.ClosureUses.ToArray();
                            closureInfo.ClosureUsagesIsSet = true;
                        }
                    }
                    break;
            }
        }

        private void ParamsExit()
        {
            if (functionClosureOrder.Peek() == FunctionClosure.Function && currentFunctions.Any())
            {
                for (int i = 1; i <= parameters.Count; i++)
                {
                    currentFunctions.Peek().Parameters.Add(new Tuple<uint, string>((uint)i, ""), parameters.ElementAt(i-1));
                }
                //currentFunctions.Peek().Parameters = parameters.ToArray();
                parameters.Clear();
            }
            else if (functionClosureOrder.Peek() == FunctionClosure.Closure && currentClosures.Any())
            {
                currentClosures.Peek().Closure.Parameters = parameters.ToArray();
                parameters.Clear();
            }
            else
            {
                throw new InvalidOperationException("Leaving params outside function, method or closure is not supported");
            }
            inParamDeclarations = false;
        }

        private void ClassMethodExit()
        {
            if (currentClass != null)
            {
                currentClass.Methods.Add(currentFunctions.Pop());
            }
            else if (currentInterface != null)
            {
                currentInterface.Methods.Add(currentFunctions.Pop());
            }
            else
            {
                throw new InvalidOperationException("Leaving method outside interface and class declaration is not supported.");
            }
            functionClosureOrder.Pop();
        }

        private void Name(XmlNode node)
        {
            string name = node.FirstChild.InnerText;
            if (currentClass != null && currentClass.Name == null)
            {
                currentClass.Name = name;
            }
            else if (currentInterface != null && currentInterface.Name == null)
            {
                currentInterface.Name = name;
            }
            else if (currentFunctions.Any() && currentFunctions.Peek().Name == null)
            {
                currentFunctions.Peek().Name = name;
            }
            else if (inParamDeclarations && currentParameter.Name == null)
            {
                currentParameter.Name = name;
            }
            else if (inPropertyDeclarations && currentProperty != null && currentProperty.Name == null)
            {
                currentProperty.Name = name;
            }
        }

        private void ClassMethodEnter(XmlNode node)
        {
            var subNodeForName = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);
            var methodName = subNodeForName.InnerText;
            var isMagicMethod = false;

            switch (methodName)
            {
                case "__construct":
                case "__destruct":
                case "__call":
                case "__callStatic":
                case "__get":
                case "__set":
                case "__isset":
                case "__unset":
                case "__sleep":
                case "__wakeup":
                case "__toString":
                case "__invoke":
                case "__set_state":
                case "__clone":
                case "__debugInfo":
                    isMagicMethod = true;
                    break;
            }

            currentFunctions.Push(new Function(node, isMagicMethod)
            {
                StartLine = AstNode.GetStartLine(node),
                EndLine = AstNode.GetEndLine(node)
            });
            functionClosureOrder.Push(FunctionClosure.Function);
        }

        private void Subnode_ByRef_Enter(XmlNode node)
        {
            if (inParamDeclarations && !parameterByRefIsSet)
            {
                currentParameter.ByReference = Convert.ToBoolean(node.FirstChild.LocalName);
                parameterByRefIsSet = true;
            }
            else if (currentClosureUse != null && !parameterByRefIsSet)
            {
                currentClosureUse.ByReference = Convert.ToBoolean(node.FirstChild.LocalName);
                parameterByRefIsSet = true;
            }
        }
    }

    internal class ExtractorClosureInfo
    {
        public Closure Closure { get; set; }
        public Queue<ClosureUse> ClosureUses { get; set; } 

        public bool ClosureUsagesIsSet { get; set; }
    }
}