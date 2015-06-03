using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using PHPAnalysis;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Analysis.CFG.Taint;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;

namespace WordPress.Plugin
{
    [Export(typeof (ITaintProvider))]
    public sealed class WpTaintProvider : ITaintProvider
    {
        public ImmutableVariableStorage GetTaint()
        {
            var defaultTaint = new DefaultTaintProvider().GetTaint();

            var variableStorage = defaultTaint.ToMutable();

            foreach (var superGlobal in variableStorage.SuperGlobals)
            {
                foreach (var var in superGlobal.Value.Info.Variables)
                {
                    var newTaint = var.Value.Info.Taints.DeepClone();
                    if (newTaint.SqliTaint.Single().TaintTag > SQLITaint.SQL_NoQ)
                    {
                        newTaint.SqliTaint.Clear();
                        newTaint.SqliTaint.Add(new SQLITaintSet( SQLITaint.SQL_NoQ));
                    }

                    var.Value.Info.Taints = newTaint;
                    var.Value.Info.DefaultDimensionTaintFactory = () => 
                        new TaintSets(new SQLITaintSet(SQLITaint.SQL_NoQ), new XSSTaintSet(XSSTaint.XSS_ALL));
                    var.Value.Info.NestedVariableDefaultTaintFactory = var.Value.Info.DefaultDimensionTaintFactory;
                }
            }

            var wpdbGlobal = new Variable("wpdb", VariableScope.File);
            var prefixVar = new Variable("prefix", VariableScope.Instance) { Info = { Value = "Eir_" } };
            wpdbGlobal.Info.Variables.Add(new VariableTreeDimension() { Key = "prefix" }, prefixVar);
            wpdbGlobal.Info.ClassNames.Add("wpdb");

            variableStorage.GlobalVariables.Add("wpdb", wpdbGlobal);

            return ImmutableVariableStorage.CreateFromMutable(variableStorage);
        }
    }
}