using System;
using System.Collections.Generic;
using System.Linq;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis
{
    public sealed class DefaultTaintProvider : ITaintProvider
    {
        private readonly Func<TaintSets> _taintedTaintFactory = () =>
            new TaintSets(new SQLITaintSet(SQLITaint.SQL_ALL), new XSSTaintSet(XSSTaint.XSS_ALL));

        private readonly Func<TaintSets> _untaintedTaintFactory = () => 
            new TaintSets(new SQLITaintSet(), new XSSTaintSet());

        public TaintSets GetTaintedTaintSet()
        {
            return _taintedTaintFactory.Invoke();
        }

        public ImmutableVariableStorage GetTaint()
        {
            var superglobals = new List<Variable> {
                                                      new Variable("_GET", VariableScope.SuperGlobal),
                                                      new Variable("_POST", VariableScope.SuperGlobal),
                                                      new Variable("_REQUEST", VariableScope.SuperGlobal),
                                                      new Variable("_COOKIE", VariableScope.SuperGlobal),
                                                      DefaultServerVariable()
                                                  };
            var globals = new List<Variable> {
                                                 new Variable("HTTP_GET_VARS", VariableScope.File),
                                                 new Variable("HTTP_POST_VARS", VariableScope.File),
                                                 new Variable("HTTP_SERVER_VARS", VariableScope.File),
                                                 new Variable("HTTP_COOKIE_VARS", VariableScope.File),
                                             };
            Action<Variable> setDefaultTaint = x =>
            {
                x.Info.NestedVariableDefaultTaintFactory = _taintedTaintFactory;
                x.Info.DefaultDimensionTaintFactory = _taintedTaintFactory;
                x.Info.NestedVariablePossibleStoredDefaultTaintFactory = _untaintedTaintFactory;
            };
            
            globals.ForEach(setDefaultTaint);
            superglobals.ForEach(setDefaultTaint);

            superglobals.AddRange(new[]
                                  {
                                      new Variable("GLOBALS", VariableScope.SuperGlobal),
                                      new Variable("_FILES", VariableScope.SuperGlobal),
                                      new Variable("_SESSION", VariableScope.SuperGlobal),
                                      new Variable("_ENV", VariableScope.SuperGlobal),     
                                  });

            

            var rawPost = new Variable("HTTP_RAW_POST_DATA", VariableScope.File) { Info = { Taints = _taintedTaintFactory() } };
            var argv = new Variable("argv", VariableScope.File);
            // Docs: "The first argument $argv[0] is always the name that was used to run the script." - goo.gl/hrek2V
            argv.Info.Variables.Add(new VariableTreeDimension() { Index = 0, Key = "0" }, new Variable("0", VariableScope.Instance));
            globals.AddRange(new[] {rawPost, argv});

            var varStorage = new VariableStorage();
            varStorage.SuperGlobals.AddRange(superglobals.ToDictionary(s => s.Name, s => s));
            varStorage.GlobalVariables.AddRange(globals.ToDictionary(g => g.Name, g => g));

            return ImmutableVariableStorage.CreateFromMutable(varStorage);
        }

        private Variable DefaultServerVariable()
        {
            var server = new Variable("_SERVER", VariableScope.SuperGlobal) 
                         { 
                             Info = {
                                      NestedVariableDefaultTaintFactory = _taintedTaintFactory, 
                                      DefaultDimensionTaintFactory = _taintedTaintFactory,
                                      NestedVariablePossibleStoredDefaultTaintFactory = _untaintedTaintFactory
                                    } 
                         };

            var safeServerVars = new[]
                                 {
                                     // IDEA - These could easily be defined in an external file, to allow for changes without recompiling.
                                     new Variable("GATEWAY_INTERFACE", VariableScope.Instance),
                                     new Variable("HTTPS", VariableScope.Instance),
                                     new Variable("REMOTE_ADDR", VariableScope.Instance),
                                     new Variable("REMOTE_HOST", VariableScope.Instance),
                                     new Variable("REMOTE_PORT", VariableScope.Instance),
                                     new Variable("REQUEST_TIME", VariableScope.Instance),
                                     new Variable("SCRIPT_FILENAME", VariableScope.Instance),
                                     new Variable("SCRIPT_NAME", VariableScope.Instance),
                                     new Variable("SERVER_ADDR", VariableScope.Instance),
                                     new Variable("SERVER_ADMIN", VariableScope.Instance),
                                     new Variable("SERVER_PROTOCOL", VariableScope.Instance),
                                     new Variable("SERVER_PORT", VariableScope.Instance),
                                     new Variable("SERVER_SIGNATURE", VariableScope.Instance),
                                     new Variable("SERVER_SOFTWARE", VariableScope.Instance),
                                 };
            foreach (var safeServerVar in safeServerVars)
            {
                safeServerVar.Info.Taints = _untaintedTaintFactory();
                safeServerVar.Info.DefaultDimensionTaintFactory = _untaintedTaintFactory;
                safeServerVar.Info.NestedVariableDefaultTaintFactory = _untaintedTaintFactory;
                safeServerVar.Info.NestedVariablePossibleStoredDefaultTaintFactory = _untaintedTaintFactory;

                server.Info.Variables.Add(new VariableTreeDimension() { Key = safeServerVar.Name }, safeServerVar);
            }

            var serverName = new Variable("SERVER_NAME", VariableScope.Instance)
                             {
                                 // SERVER_NAME seems to be XSS safe, but not necessarily SQLi safe: http://shiflett.org/blog/2006/mar/server-name-versus-http-host
                                 Info =
                                 {
                                     Taints = new TaintSets(new SQLITaintSet(SQLITaint.SQL_ALL), new XSSTaintSet()),
                                     DefaultDimensionTaintFactory = _untaintedTaintFactory,
                                     NestedVariableDefaultTaintFactory = _untaintedTaintFactory,
                                     NestedVariablePossibleStoredDefaultTaintFactory = _untaintedTaintFactory
                                 }
                             };
            server.Info.Variables.Add(new VariableTreeDimension() { Key = serverName.Name }, serverName );

            return server;
        }
    }
}