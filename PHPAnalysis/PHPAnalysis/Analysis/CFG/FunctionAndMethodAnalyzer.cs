using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG
{
    public class FunctionAndMethodAnalyzer
    {
        public bool UseSummaries { get; set; }

        private readonly ImmutableVariableStorage _varStorage;
        private readonly IIncludeResolver _incResolver;
        private readonly AnalysisStacks _stacks;
        private readonly CustomFunctionHandler _customFunctionHandler;
        private readonly IVulnerabilityStorage _vulnerabilityStorage;
        private readonly FunctionsHandler _funcHandler;

        public FunctionAndMethodAnalyzer(ImmutableVariableStorage variableStorage, IIncludeResolver incResolver,
            AnalysisStacks stacks, CustomFunctionHandler customFuncHandler,
            IVulnerabilityStorage vulnerabilityStorage, FunctionsHandler fh)
        {
            this._varStorage = variableStorage;
            this._incResolver = incResolver;
            this._stacks = stacks;
            this._customFunctionHandler = customFuncHandler;
            this._vulnerabilityStorage = vulnerabilityStorage;
            this._funcHandler = fh;
        }

        public FunctionsHandler FunctionsHandler => _funcHandler;

        /// <summary>
        /// Method to analyze a PHP funciton call. 
        /// </summary>
        /// <param name="functionCall">The function call to analyze</param>
        /// <param name="argInfos">The argument information to include in the analysis</param>
        /// <returns>The found Taintsets for the FunctionCall</returns>
        public ExpressionInfo AnalyzeFunctionCall(FunctionCall functionCall, IList<ExpressionInfo> argInfos)
        {
            //In most cases this should be either 0 or 1 but situations where functions are specified several places can happen.
            //Therefore, we support it and select the worst case.
            List<Function> functionList = _funcHandler.LookupFunction(functionCall.Name);
            return CreateCommonTaintSets(functionList, argInfos);
        }

        /// <summary>
        /// Method to analyze a PHP method call with the class name included
        /// </summary>
        /// <param name="methodCall">The method call to analyze</param>
        /// <param name="argInfos">The argument infos to include in the analysis</param>
        /// <returns>The common TaintSets found</returns>
        public ExpressionInfo AnalyzeMethodCall(MethodCall methodCall, IList<ExpressionInfo> argInfos)
        {
            //In most cases there should be either 0 or 1 classes, but situations where functions are specified several places can happen.
            //Therefore, we support it and select the worst case.
            var exprInfo = new ExpressionInfo();

            //Try to find all the possible method calls, and create the worst case scenario of taints.
            foreach (string className in methodCall.ClassNames)
            {
                IList<Function> funclist = _funcHandler.LookupFunction(methodCall.CreateFullMethodName(className));
                exprInfo = exprInfo.Merge(CreateCommonTaintSets(funclist, argInfos));
            }

            return exprInfo;
        }

        private static readonly HashSet<string> AnalyzedFunctions = new HashSet<string>(); 

        /// <summary>
        /// Method to create a common taint set from a list of functions
        /// </summary>
        /// <param name="functionList">The list of functions to analyze</param>
        /// <param name="argInfos">The argument ExpressionInfo to include in the analysis</param>
        /// <returns>A common worst-case TaintSets</returns>
        private ExpressionInfo CreateCommonTaintSets(IEnumerable<Function> functionList, IList<ExpressionInfo> argInfos)
        {
            //Create a common TaintSets that will be merged with the TaintSets from the found functions
            var exprInfo = new ExpressionInfo();
            if (functionList.Any())
            {
                AnalyzedFunctions.Add(functionList.First().Name);
                foreach (var func in functionList)
                {
                    var summary = MatchWithFunctionSummary(func, argInfos);

                    if (summary == null)
                    {
                        var taintSetsForFuncOrMethod = GetTaintSetsForFuncOrMethod(func, argInfos);
                        exprInfo = exprInfo.Merge(taintSetsForFuncOrMethod);

                        GenerateSummary(func, argInfos, taintSetsForFuncOrMethod);
                    }
                    else
                    {
                        exprInfo = exprInfo.Merge(summary.ReturnValue);
                    }
                }
            }
            else
            {
                // If function cannot be resolve, use the arguments taint values.
                exprInfo = argInfos.Aggregate(exprInfo, (current, arg) => current.Merge(arg));
            }
            return exprInfo;
        }

        private void GenerateSummary(Function func, IList<ExpressionInfo> argInfos, ExpressionInfo result)
        {
            if (!UseSummaries)
            {
                return;
            }

            var summary = new FunctionSummary(func.Name);
            summary.ArgInfos.AddRange(argInfos);
            summary.ReturnValue = result;

            List<FunctionSummary> existingSummaries;
            if (_funcHandler.FunctionSummaries.TryGetValue(func, out existingSummaries))
            {
                existingSummaries.Add(summary);
            }
            else
            {
                _funcHandler.FunctionSummaries.Add(func, new List<FunctionSummary>() {summary});
            }
        }

        private FunctionSummary MatchWithFunctionSummary(Function func, IList<ExpressionInfo> argInfos)
        {
            List<FunctionSummary> summaries;
            if (_funcHandler.FunctionSummaries.TryGetValue(func, out summaries))
            {
                var callArgInfo = argInfos.ToArray();
                foreach (var functionSummary in summaries)
                {
                    var summaryArgInfo = functionSummary.ArgInfos.ToArray();

                    if (callArgInfo.Count() != summaryArgInfo.Count()) { continue; }
                    if (!callArgInfo.Any())
                    {
                        return functionSummary;
                    }

                    for (int i = 0; i < callArgInfo.Length; i++)
                    {
                        if (callArgInfo[i].ExpressionStoredTaint.Equals(summaryArgInfo[i].ExpressionStoredTaint) &&
                            callArgInfo[i].ExpressionTaint.Equals(summaryArgInfo[i].ExpressionTaint))
                        {
                            return functionSummary;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Method to get the TaintSets for a single function.
        /// </summary>
        /// <param name="func">The function to find the TaintSets for</param>
        /// <param name="argInfos">The argument ExpressionInfo to include in the analysis</param>
        /// <returns>The found TaintSets for the function</returns>
        private ExpressionInfo GetTaintSetsForFuncOrMethod(Function func, IList<ExpressionInfo> argInfos)
        {
            //Start working from the bast case, and find worser cases.
            var tmp = new ExpressionInfo();

            //Try to cast the function to every possible function/method
            var xssSantizerFunc = CastToFunctionType<XSSSanitizer>(func);
            var sqlSantizerFunc = CastToFunctionType<SQLSanitizer>(func);

            //We have to find customFunctions explicitly as it can always be casted to a Function, as it is the base class.
            //However if we introduce a custom function class then it can be castet to this type, and work like the rest.
            Function customFunc = _funcHandler.FindCustomFunctionByName(func.Name);

            //If there exists a Source with the function name, then we select it.
            //But sources and be of several other types, like arrays etc.
            Source sourceFunc = _funcHandler.FindSourceByName(func.Name);

            //TODO: This should be changed to be dependent on parameters!!! 
            if (xssSantizerFunc != null)
            {
                SQLITaintSet sqliTaintSet = new SQLITaintSet();
                XSSTaintSet xssts = new XSSTaintSet();
                var returnParameters = xssSantizerFunc.Parameters.Where(x => x.Value.IsReturn);
                try
                {
                    ExpressionInfo returnParameter = new ExpressionInfo();
                    foreach(var item in returnParameters)
                    {
                        var actualParam = argInfos.ElementAt((int)item.Key.Item1 -1);
                        if(actualParam == null)
                        {
                            continue;
                        }
                        returnParameter = returnParameter.Merge(actualParam);
                    }
                    sqliTaintSet = returnParameter.ExpressionTaint.SqliTaint.Aggregate(sqliTaintSet, (current, taint) => current.Merge(taint));
                    xssts = returnParameter.ExpressionTaint.XssTaint.Aggregate(xssts, (current, taint) => current.Merge(taint));
                }
                catch(NullReferenceException)
                {
                    Debug.WriteLine("Could not map actual parameter with formal parameter, using default");
                    sqliTaintSet = new SQLITaintSet();
                    xssts = argInfos.Aggregate(xssts, (current1, info) => current1.Merge(info.ExpressionTaint.XssTaint.Aggregate(current1, (current, taint) => current.Merge(taint))));
                }
                tmp = new ExpressionInfo() {ExpressionTaint =  new TaintSets(
                    sqliTaintSet,
                    xssSantizerFunc.DefaultStatus < xssts.TaintTag ? new XSSTaintSet(xssSantizerFunc.DefaultStatus) : xssts.DeepClone()) };
            }
            if (sqlSantizerFunc != null)
            {
                XSSTaintSet xssts = new XSSTaintSet();
                SQLITaintSet sqlts = new SQLITaintSet();
                var returnParameters = sqlSantizerFunc.Parameters.Where(x => x.Value.IsReturn);
                try
                {
                    ExpressionInfo returnParameter = new ExpressionInfo();
                    foreach(var item in returnParameters)
                    {
                        var actualParam = argInfos.ElementAt((int)item.Key.Item1 - 1);
                        if(actualParam == null)
                        {
                            continue;
                        }
                        returnParameter = returnParameter.Merge(actualParam);
                    }
                    xssts = returnParameter.ExpressionTaint.XssTaint.Aggregate(xssts, (current, taint) => current.Merge(taint));
                    sqlts = returnParameter.ExpressionTaint.SqliTaint.Aggregate(sqlts, (current, taint) => current.Merge(taint));
                }
                catch(NullReferenceException)
                {
                    Debug.WriteLine("Could not map actual parameter with formal parameter, using default");
                    xssts = new XSSTaintSet();
                    sqlts = argInfos.Aggregate(sqlts, (current1, info) => current1.Merge(info.ExpressionTaint.SqliTaint.Aggregate(current1, (current, taint) => current.Merge(taint))));
                }
                tmp = new ExpressionInfo() { ExpressionTaint =  new TaintSets(
                    sqlSantizerFunc.DefaultStatus < sqlts.TaintTag ? new SQLITaintSet(sqlSantizerFunc.DefaultStatus) : sqlts.DeepClone(),
                    xssts) };
            }
            if (customFunc != null)
            {
                _funcHandler.ScannedFunctions.Add(customFunc);
                tmp = this._customFunctionHandler.AnalyseCustomFunction(customFunc, this._varStorage, _vulnerabilityStorage, argInfos, this._incResolver, this._stacks);
            }
            if (sourceFunc != null)
            {
                tmp = new ExpressionInfo() { ExpressionTaint =  new TaintSets(sourceFunc.SqliTaint, sourceFunc.XssTaint) };
            }

            //If no matches were found the function is unknown and therefore we return the worst case of them all!
            return tmp;
        }

        /// <summary>
        /// Method to cast a function to a subclass type.
        /// </summary>
        /// <returns>The Function as the given generic subtype, or null if not possible to cast!</returns>
        /// <param name="func">The function to try to cast to subtype</param>
        /// <typeparam name="T">The function subtype to cast the function to.</typeparam>
        private T CastToFunctionType<T>(Function func)
        {
            try
            {
                return (T)Convert.ChangeType(func, typeof(T));
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine("Could not cast function: {0} to given function type: {1}", func.Name, typeof(T).Name);
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }
    }
}
