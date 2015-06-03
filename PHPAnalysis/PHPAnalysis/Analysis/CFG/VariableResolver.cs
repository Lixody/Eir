using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;

namespace PHPAnalysis.Analysis.CFG
{
    public sealed class VariableResolveResult
    {
        public readonly Variable Variable;
        public readonly bool IsNew;

        public VariableResolveResult(Variable var, bool isNew = false)
        {
            this.Variable = var;
            this.IsNew = isNew;
        }
    }
    public sealed class VariableResolver
    {
        private static readonly string[] ResolvableNodes =
        {
            AstConstants.Node + ":" + AstConstants.Nodes.Expr_Variable,
            AstConstants.Node + ":" + AstConstants.Nodes.Expr_StaticPropertyFetch,
            AstConstants.Node + ":" + AstConstants.Nodes.Expr_PropertyFetch,
            AstConstants.Node + ":" + AstConstants.Nodes.Expr_ArrayDimFetch,
            //AstConstants.Node + ":" + AstConstants.Nodes.Expr_ClassConstFetch,
        };

        private static readonly string[] GlobalDeclarations =
        {
            AstConstants.Node + ":" + AstConstants.Nodes.Stmt_Global,
        };

        private readonly IVariableStorage _variableStorage;
        private readonly AnalysisScope _scope;
        private readonly VariableScope variableScope;

        public VariableResolver(IVariableStorage varStorage, AnalysisScope scope = AnalysisScope.File)
        {
            Preconditions.NotNull(varStorage, "varStorage");
            this._variableStorage = varStorage;
            this._scope = scope;

            variableScope = scope == AnalysisScope.File ? VariableScope.File : VariableScope.Function;
        }

        public void ResolveGlobalDeclaration(XmlNode node)
        {
            Preconditions.IsTrue(GlobalDeclarations.Contains(node.Name), "Node was not a global declaration. It was: " + node.Name, "node");

            var variables = GlobalNode.GetVariables(node);

            foreach (var variableNode in variables)
            {
                string variableName;
                if (ExprVarNode.TryGetVariableName(variableNode, out variableName))
                {
                    Variable variable;
                    if (_variableStorage.GlobalVariables.TryGetValue(variableName, out variable) && 
                        !_variableStorage.LocalAccessibleGlobals.ContainsKey(variableName))
                    {
                        _variableStorage.LocalAccessibleGlobals.Add(variableName, variable);
                    }
                }
            }
        }

        public VariableResolveResult ResolveVariable(XmlNode node)
        {
            Preconditions.IsTrue(ResolvableNodes.Contains(node.Name), "Cannot resolve variable of " + node.Name, "node");

            return GetVariable(node);
            
            // Case1: Standard var
            // Case2: ArrayType of some kind
            // Case3: Lokal vs Global vars

        }

        private VariableResolveResult GetVariable(XmlNode node)
        {
            switch (node.Name)
            {
                case AstConstants.Node + ":" + AstConstants.Nodes.Expr_Variable:
                    return Node_Expr_Variable(node);
                case AstConstants.Node + ":" + AstConstants.Nodes.Expr_StaticPropertyFetch:
                    return Node_Expr_StaticPropertyFetch(node);
                case AstConstants.Node + ":" + AstConstants.Nodes.Expr_PropertyFetch:
                    return Node_Expr_PropertyFetch(node);
                case AstConstants.Node + ":" + AstConstants.Nodes.Expr_ArrayDimFetch:
                    return Node_Expr_ArrayDimFetch(node);
                    
                // Subnodes
                case AstConstants.Subnode + ":" + AstConstants.Subnodes.Var:
                    return GetVariable(node.GetSubNodesByPrefix(AstConstants.Node).Single());
            }
            return new VariableResolveResult(new Variable("$UNKNOWN$", VariableScope.Unknown), true);
        }

        private VariableResolveResult Node_Expr_Variable(XmlNode node)
        {
            var nameNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);

            if (nameNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String, out nameNode))
            {
                string varName = nameNode.InnerText;
                Variable variable;

                var localVariables = GetLocalScope(_scope);
                if (_variableStorage.SuperGlobals.TryGetValue(varName, out variable) ||
                    localVariables.TryGetValue(varName, out variable) ||
                    _variableStorage.LocalAccessibleGlobals.TryGetValue(varName, out variable))
                {
                    return new VariableResolveResult(variable);
                }

                var newVariable = new Variable(varName, variableScope);
                localVariables.Add(varName, newVariable);

                return new VariableResolveResult(newVariable, true);
            }

            // We do not support variable variables.
            return new VariableResolveResult(new Variable("$UNKNOWN$", variableScope), true);
        }

        private VariableResolveResult Node_Expr_StaticPropertyFetch(XmlNode node)
        {
            var classNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Class);
            var nameNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);

            if (classNode.TryGetSubNode(AstConstants.Node + ":" + AstConstants.Nodes.Name, out classNode) &&
                classNode.TryGetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Parts, out classNode) &&
                classNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.Array, out classNode) &&
                classNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String, out classNode))
            {
                string className = classNode.InnerText;
                if (className == "self" || className == "parent")
                {
                    return new VariableResolveResult(new Variable("$UNKNOWN$", VariableScope.Class), true);
                    //throw new NotImplementedException("We need a way to handle class scopes before this can be effectively handled");
                }

                XmlNode nameStringNode;
                if (nameNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String, out nameStringNode))
                {
                    Variable field;

                    if (!_variableStorage.ClassVariables.TryGetValue(className, out field))
                    {
                        field = new Variable(className, VariableScope.Class);
                        _variableStorage.ClassVariables.Add(className, field);
                    }
                    if (field.Info.TryGetVariableByString(nameStringNode.InnerText, out field))
                    {
                        return new VariableResolveResult(field);
                    }

                    field = new Variable(nameStringNode.InnerText, VariableScope.Class);
                    _variableStorage.ClassVariables[className].Info.Variables.Add(new VariableTreeDimension() { Key = nameStringNode.InnerText }, field);
                    return new VariableResolveResult(field, true);
                }
            }
            
            return new VariableResolveResult(new Variable("$UNKNOWN$", VariableScope.Class), true);
        }

        private VariableResolveResult Node_Expr_PropertyFetch(XmlNode node)
        {
            var varNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var);
            var variable = GetVariable(varNode.GetSubNodesByPrefix(AstConstants.Node).Single());
            var nameNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Name);
            if (nameNode.TryGetSubNode(AstConstants.Scalar + ":" + AstConstants.Scalars.String, out nameNode))
            {
                Variable property;
                if (variable.Variable.Info.TryGetVariableByString(nameNode.InnerText, out property))
                {
                    return new VariableResolveResult(property);
                }

                property = new Variable(nameNode.InnerText, VariableScope.Instance);
                property.Info.NestedVariableDefaultTaintFactory = variable.Variable.Info.NestedVariableDefaultTaintFactory;
                property.Info.Taints = variable.Variable.Info.NestedVariableDefaultTaintFactory();
                property.Info.NestedVariablePossibleStoredDefaultTaintFactory = variable.Variable.Info.NestedVariablePossibleStoredDefaultTaintFactory;
                property.Info.PossibleStoredTaint = ClonePossibleStored(variable.Variable.Info);
                variable.Variable.Info.Variables.Add(new VariableTreeDimension() { Key = property.Name }, property);
                return new VariableResolveResult(property, true);
            }

            return new VariableResolveResult(new Variable("$UNKNOWN$", VariableScope.Instance), true);
        }

        private VariableResolveResult Node_Expr_ArrayDimFetch(XmlNode node)
        {
            var arrayVarResult = GetVariable(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Var));
            var dimValueNode = node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Dim)
                                   .GetSubNodesByPrefix(AstConstants.Node).SingleOrDefault();

            if (dimValueNode == null)
            {
                // This can happen with e.g. $myArray[] = *something*;
                
                return new VariableResolveResult(arrayVarResult.Variable.Unknown, true);
            }

            if (dimValueNode.LocalName == AstConstants.Nodes.Scalar_String)
            {
                var keyValue = dimValueNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value).InnerText;
                Variable fetchedVar;
                if (arrayVarResult.Variable.Info.TryGetVariableByString(keyValue, out fetchedVar))
                {
                    return new VariableResolveResult(fetchedVar);
                }
                var newVar = new Variable(keyValue, VariableScope.Instance);
                var arrayDimension = new VariableTreeDimension() {Key = keyValue};
                double numericKeyValue;
                if (double.TryParse(keyValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out numericKeyValue))
                {
                    arrayDimension.Index = (int) numericKeyValue;
                }
                newVar.Info.Taints = arrayVarResult.Variable.Info.NestedVariableDefaultTaintFactory();
                newVar.Info.NestedVariableDefaultTaintFactory = arrayVarResult.Variable.Info.NestedVariableDefaultTaintFactory;
                newVar.Info.PossibleStoredTaint = ClonePossibleStored(arrayVarResult.Variable.Info);
                newVar.Info.NestedVariablePossibleStoredDefaultTaintFactory = arrayVarResult.Variable.Info.NestedVariablePossibleStoredDefaultTaintFactory;
                arrayVarResult.Variable.Info.Variables.Add(arrayDimension, newVar);
                return new VariableResolveResult(newVar, true);
            }
            if (dimValueNode.LocalName == AstConstants.Nodes.Scalar_LNumber ||
                dimValueNode.LocalName == AstConstants.Nodes.Scalar_DNumber)
            {
                var valueNode = dimValueNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value);
                int index = (int) Convert.ToDouble(valueNode.InnerText, CultureInfo.InvariantCulture);

                Variable fetchedVar;
                if (arrayVarResult.Variable.Info.TryGetVariableByIndex(index, out fetchedVar))
                {
                    return new VariableResolveResult(fetchedVar);
                }

                var newVar = new Variable(index.ToString(CultureInfo.InvariantCulture), VariableScope.Instance) 
                {
                    Info = {
                               Taints = arrayVarResult.Variable.Info.NestedVariableDefaultTaintFactory(),
                               NestedVariableDefaultTaintFactory = arrayVarResult.Variable.Info.NestedVariableDefaultTaintFactory,
                               NestedVariablePossibleStoredDefaultTaintFactory = arrayVarResult.Variable.Info.NestedVariablePossibleStoredDefaultTaintFactory,
                               PossibleStoredTaint = ClonePossibleStored(arrayVarResult.Variable.Info)
                           }
                };
                arrayVarResult.Variable.Info.Variables.Add(new VariableTreeDimension() {
                                                                                           Index = index, 
                                                                                           Key = index.ToString(CultureInfo.InvariantCulture)
                                                                                       }, newVar);
                return new VariableResolveResult(newVar, true);
            }

            // Couldn't resolve array element.
            return new VariableResolveResult(arrayVarResult.Variable.Unknown, true);
        }

        public StoredVulnInfo ClonePossibleStored(ValueInfo valInfo)
        {
            return new StoredVulnInfo() {
                ICantFeelIt = valInfo.PossibleStoredTaint.ICantFeelIt,
                StorageName = valInfo.PossibleStoredTaint.StorageName,
                StorageOrigin = valInfo.PossibleStoredTaint.StorageOrigin,
                Taint = valInfo.NestedVariablePossibleStoredDefaultTaintFactory()
            };
        }

        public bool IsResolvableNode(XmlNode node)
        {
            return node != null && ResolvableNodes.Contains(node.Name);
        }

        private IDictionary<string, Variable> GetLocalScope(AnalysisScope scope)
        {
            if (scope == AnalysisScope.File)
            {
                return _variableStorage.GlobalVariables;
            }
            return _variableStorage.LocalVariables;
        }
    }
}
