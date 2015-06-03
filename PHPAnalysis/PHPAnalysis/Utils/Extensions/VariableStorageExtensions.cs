using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data;

namespace PHPAnalysis.Utils
{
    public static class VariableStorageExtensions
    {
        public static IVariableStorage ToMutable(this ImmutableVariableStorage immutableStorage)
        {
            return new VariableStorage(
                immutableStorage.SuperGlobals.ToDictionary(s => s.Key, s => s.Value.AssignmentClone()), 
                immutableStorage.Globals.ToDictionary(s => s.Key, s => s.Value.AssignmentClone()), 
                immutableStorage.Locals.ToDictionary(s => s.Key, s => s.Value.AssignmentClone()), 
                immutableStorage.ClassVariables.ToDictionary(s => s.Key, s => s.Value.AssignmentClone()),
                immutableStorage.LocalAccessibleGlobals.ToDictionary(s => s.Key, s => s.Value.AssignmentClone()));
        }
    }
}
