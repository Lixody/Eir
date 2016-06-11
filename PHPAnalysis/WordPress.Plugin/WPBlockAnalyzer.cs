using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using PHPAnalysis;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Analysis.PHPDefinitions;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;

namespace WordPress.Plugin
{
    [Export(typeof(IBlockAnalyzerComponent))]
    public sealed class WPBlockAnalyzer : IBlockAnalyzerComponent
    {
        public Func<IVariableStorage, FunctionAndMethodAnalyzer> FunctionMethodAnalyzerFactory { get; set; }

        private readonly string[] _getOptionsFunctions = {
                                                             "get_option",
                                                             "get_site_option",
                                                         };

        //private readonly string[] _deleteOptionFunctions = {
        //                                                       "delete_option",
        //                                                       "delete_site_option",
        //                                                   };

        private readonly string[] _addOptionFunctions = {
                                                            "add_option",
                                                            "add_site_option",
                                                        };

        private readonly string[] _updateOptionFunctions = {
                                                               "update_site_option",
                                                               "update_option",
                                                           };

        private readonly string[] _hookFunctions = 
        {
            "add_action",
            "add_filter",
            "add_menu_page",
            "add_submenu_page",
            "add_dashboard_page",
            "add_posts_page",
            "add_media_page",
            "add_links_page",
            "add_pages_page",
            "add_comments_page",
            "add_theme_page",
            "add_plugins_page",
            "add_users_page",
            "add_management_page",
            "add_options_page",
            "register_sidebar_widget",    //deprecated sidebar_widget register
            "wp_register_sidebar_widget", //new sidebar_widget register
        };

        public ExpressionInfo Analyze(XmlNode node, ExpressionInfo exprInfo, IVariableStorage currentStorage, IVulnerabilityStorage storage)
        {
            return exprInfo;
        }

        public ExpressionInfo AnalyzeFunctionCall(XmlNode node, ExpressionInfo exprInfo, IVariableStorage varStorage, 
                                                  IVulnerabilityStorage vulnStorage, IDictionary<uint, ExpressionInfo> argumentInfos, AnalysisStacks analysisStacks)
        {
            var funcCall = new FunctionCallExtractor().ExtractFunctionCall(node);
            if (_getOptionsFunctions.Contains(funcCall.Name) ||
                _addOptionFunctions.Contains(funcCall.Name) ||
                _updateOptionFunctions.Contains(funcCall.Name))
            {
                return HandleOptionsCall(funcCall, node, exprInfo, varStorage, vulnStorage, argumentInfos, analysisStacks);
            }
            else if (_hookFunctions.Contains(funcCall.Name))
            {
                return HandleHookCall(node, exprInfo, varStorage, analysisStacks);
            }
            return exprInfo;
        }

        /// <summary>
        /// Make sure that hardcoded callback functions are analyzed.
        /// </summary>
        private ExpressionInfo HandleHookCall(XmlNode node, ExpressionInfo exprInfo, IVariableStorage currentStorage, AnalysisStacks analysisStacks)
        {
            var functionCall = new FunctionCallExtractor().ExtractFunctionCall(node);
            var result = new ExpressionInfo();

            foreach (var argument in functionCall.Arguments.Where(a => a.Value.LocalName == AstConstants.Nodes.Scalar_String))
            {
                var stringValue = ScalarNode.GetStringValue(argument.Value);
                var functionAnalyzer = FunctionMethodAnalyzerFactory(currentStorage);
                var functions = functionAnalyzer.FunctionsHandler.LookupFunction(stringValue);
                if (functions.Any())
                {
                    //Console.WriteLine("FOUND " + functions.Count() + " functions with name: " + stringValue);
                    var call = new FunctionCall(stringValue, null, AstNode.GetStartLine(node), AstNode.GetEndLine(node));

                    if (analysisStacks.CallStack.Any(c => c.Name == call.Name))
                    {
                        // Avoid recursive registrations. 
                        continue;
                    }
                    analysisStacks.CallStack.Push(call);
                    var funcCallResult = functionAnalyzer.AnalyzeFunctionCall(call, new ExpressionInfo[0]);
                    analysisStacks.CallStack.Pop();

                    result = result.Merge(funcCallResult);
                }

                // https://codex.wordpress.org/Function_Reference/add_submenu_page
                // If a method is called, it is called with: array( $this, 'function_name' ) or array( __CLASS__, 'function_name' )

               

            }
            return result;
        }

        private ExpressionInfo HandleOptionsCall(FunctionCall call, XmlNode node, ExpressionInfo exprInfo, IVariableStorage currentStorage, 
                                                 IVulnerabilityStorage storage, IDictionary<uint, ExpressionInfo> argumentInfos, AnalysisStacks analysisStacks)
        {
            if (_getOptionsFunctions.Contains(call.Name))
            {
                return HandleGetOptions(call, argumentInfos, exprInfo);
            }
            else if (_updateOptionFunctions.Contains(call.Name) ||
                     _addOptionFunctions.Contains(call.Name))
            {
                return HandleUpdateAddOptions(call, exprInfo, storage, argumentInfos, analysisStacks);
            }

            return exprInfo;
        }

        private ExpressionInfo HandleUpdateAddOptions(FunctionCall call, ExpressionInfo exprInfo, IVulnerabilityStorage storage, 
                                                      IDictionary<uint, ExpressionInfo> argumentInfos, AnalysisStacks analysisStacks)
        {
            XmlNode firstArgument;
            XmlNode secondArgument;

            string optionKeyValue;

            if (call.Arguments.TryGetValue(1, out firstArgument) && 
                call.Arguments.TryGetValue(2, out secondArgument) && 
                TryGetOptionKeyValue(firstArgument, argumentInfos[1], out optionKeyValue))
            {
                foreach (var sqliTaintSet in argumentInfos.ElementAt(1).Value.ExpressionTaint.SqliTaint)
                {
                    if (sqliTaintSet.TaintTag == SQLITaint.None)
                    {
                        continue;
                    }
                    string varName = (sqliTaintSet.InitialTaintedVariable ?? "???");
                    string message = "Stored SQLI found - Ingoing: " + varName +
                                    " on line: " + call.StartLine + " in file: " + analysisStacks.IncludeStack.Peek();

                    storage.AddPossibleStoredVulnerability(new StoredVulnerabilityInfo()
                    {
                        IncludeStack = analysisStacks.IncludeStack.ToImmutableStack(), 
                        CallStack = analysisStacks.CallStack.ToImmutableStack(),
                        Message = message,
                        VulnerabilityType = VulnType.SQL,
                        PossibleStoredVuln = new StoredVulnInfo()
                                             {
                                                 ICantFeelIt = IsItInYet.YesItsGoingIn,
                                                 StorageName = optionKeyValue,
                                                 StorageOrigin = "Options",
                                                 Taint = new TaintSets(sqliTaintSet, new XSSTaintSet())
                                             }
                    });
                }
                foreach (var xssTaintSet in argumentInfos.ElementAt(1).Value.ExpressionTaint.XssTaint)
                {
                    if (xssTaintSet.TaintTag == XSSTaint.None)
                    {
                        continue;
                    }
                    string varName = (xssTaintSet.InitialTaintedVariable ?? "???");
                    string message = "Stored XSS found - Ingoing: " + varName +
                                    " on line: " + call.StartLine + " in file: " + analysisStacks.IncludeStack.Peek();

                    storage.AddPossibleStoredVulnerability(new StoredVulnerabilityInfo()
                    {
                        IncludeStack = analysisStacks.IncludeStack.ToImmutableStack(), 
                        CallStack = analysisStacks.CallStack.ToImmutableStack(),
                        Message = message,
                        VulnerabilityType = VulnType.XSS,
                        PossibleStoredVuln = new StoredVulnInfo()
                                            {
                                                ICantFeelIt = IsItInYet.YesItsGoingIn,
                                                StorageName = optionKeyValue,
                                                StorageOrigin = "Options",
                                                Taint = new TaintSets(new SQLITaintSet(), xssTaintSet)
                                            }
                    });
                }
            }

            return exprInfo;
        }

        private bool TryGetOptionKeyValue(XmlNode keyParam, ExpressionInfo analysisResult, out string key)
        {
            if (keyParam.LocalName == AstConstants.Nodes.Scalar_String)
            {
                key = ScalarNode.GetStringValue(keyParam);
                return true;
            }

            if (analysisResult.ValueInfo.Value != null)
            {
                key = analysisResult.ValueInfo.Value;
                return true;
            }

            key = null;
            return false;
        }

        private ExpressionInfo HandleGetOptions(FunctionCall call, IDictionary<uint, ExpressionInfo> argumentInfos, ExpressionInfo exprInfo)
        {
            XmlNode firstArgument;
            string optionsKeyValue;
            if (call.Arguments.TryGetValue(1, out firstArgument) && TryGetOptionKeyValue(firstArgument, argumentInfos[1], out optionsKeyValue))
            {
                Func<TaintSets> taintFactory = () => new TaintSets(new SQLITaintSet(SQLITaint.SQL_ALL), new XSSTaintSet(XSSTaint.XSS_ALL));
                var possibleStoredTaint = new StoredVulnInfo()
                                            {
                                                StorageOrigin = "Options",
                                                StorageName = optionsKeyValue,
                                                Taint = taintFactory(),
                                                ICantFeelIt = IsItInYet.NoImPullingOut
                                            };
                var getOptionResult = new ExpressionInfo
                                      {
                                          ExpressionStoredTaint = possibleStoredTaint,
                                          ValueInfo =
                                          {
                                              PossibleStoredTaint = possibleStoredTaint,
                                              NestedVariablePossibleStoredDefaultTaintFactory = taintFactory,
                                          },
                                      };
                return getOptionResult;
            }
            return exprInfo;
        }
    }
}
