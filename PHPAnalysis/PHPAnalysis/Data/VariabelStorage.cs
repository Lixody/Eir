using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public interface IVariableStorage
    {
        IDictionary<string, Variable> SuperGlobals { get; }

        /// <summary>
        /// File level variables.
        /// </summary>
        IDictionary<string, Variable> GlobalVariables { get; }

        /// <summary>
        /// Function/Method/Closure variables.
        /// </summary>
        IDictionary<string, Variable> LocalVariables { get; }

        /// <summary>
        /// Static class level variables. 
        /// </summary>
        IDictionary<string, Variable> ClassVariables { get; }

        /// <summary>
        /// Global variables that has been included into the local scope.
        /// </summary>
        IDictionary<string, Variable> LocalAccessibleGlobals { get; }
        VariableStorage AssignmentClone();
    }

    public sealed class VariableStorage : IVariableStorage
    {
        public IDictionary<string, Variable> SuperGlobals { get; private set; }

        /// <summary>
        /// File level variables.
        /// </summary>
        public IDictionary<string, Variable> GlobalVariables { get; private set; }

        /// <summary>
        /// Function/Method/Closure variables.
        /// </summary>
        public IDictionary<string, Variable> LocalVariables { get; private set; }

        /// <summary>
        /// Static class level variables. 
        /// </summary>
        public IDictionary<string, Variable> ClassVariables { get; private set; }

        public IDictionary<string, Variable> LocalAccessibleGlobals { get; private set; }

        public VariableStorage() : this(new Dictionary<string, Variable>(),
                                        new Dictionary<string, Variable>(),
                                        new Dictionary<string, Variable>(),
                                        new Dictionary<string, Variable>(),
                                        new Dictionary<string, Variable>())
        {
        }

        public VariableStorage(IDictionary<string, Variable> superGlobals, 
                               IDictionary<string, Variable> globals, 
                               IDictionary<string, Variable> locals, 
                               IDictionary<string, Variable> classVariables,
                               IDictionary<string, Variable> localAccessibleGlobals)
        {
            Preconditions.NotNull(superGlobals, "superGlobals");
            Preconditions.NotNull(globals, "globals");
            Preconditions.NotNull(locals, "locals");
            Preconditions.NotNull(classVariables, "classVariables");
            Preconditions.NotNull(localAccessibleGlobals, "localAccessibleGlobals");

            SuperGlobals = superGlobals;
            GlobalVariables = globals;
            LocalVariables = locals;
            ClassVariables = classVariables;
            LocalAccessibleGlobals = localAccessibleGlobals;
        }

        public VariableStorage AssignmentClone()
        {
            var result = new VariableStorage();

            foreach (var sg in SuperGlobals)
            {
                result.SuperGlobals.Add(sg.Key, sg.Value.AssignmentClone());
            }
            foreach (var cv in ClassVariables)
            {
                result.ClassVariables.Add(cv.Key, cv.Value.AssignmentClone());
            }
            foreach (var lv in LocalVariables)
            {
                result.LocalVariables.Add(lv.Key, lv.Value.AssignmentClone());
            }
            foreach (var gv in GlobalVariables)
            {
                result.GlobalVariables.Add(gv.Key, gv.Value.AssignmentClone());
            }
            foreach (var localAccessibleGlobal in LocalAccessibleGlobals)
            {
                result.LocalAccessibleGlobals.Add(localAccessibleGlobal.Key, localAccessibleGlobal.Value.AssignmentClone());
            }
            return result;
        }
    }

    public sealed class ImmutableVariableStorage : IEquatable<ImmutableVariableStorage>, IMergeable<ImmutableVariableStorage>
    {
        public static readonly ImmutableVariableStorage Empty = new ImmutableVariableStorage();

        public IImmutableDictionary<string, Variable> SuperGlobals { get; private set; }
        public IImmutableDictionary<string, Variable> Globals { get; private set; }
        public IImmutableDictionary<string, Variable> Locals { get; private set; }
        public IImmutableDictionary<string, Variable> ClassVariables { get; private set; }
        public IImmutableDictionary<string, Variable> LocalAccessibleGlobals { get; private set; }

        private ImmutableVariableStorage()
        {
            SuperGlobals = ImmutableDictionary<string, Variable>.Empty;
            Globals = ImmutableDictionary<string, Variable>.Empty;
            Locals = ImmutableDictionary<string, Variable>.Empty;
            ClassVariables = ImmutableDictionary<string, Variable>.Empty;
            LocalAccessibleGlobals = ImmutableDictionary<string, Variable>.Empty;
        }

        public static ImmutableVariableStorage CreateFromMutable(IVariableStorage variableStorage)
        {
            var result = new ImmutableVariableStorage();
            result.SuperGlobals = result.SuperGlobals.AddRange(variableStorage.SuperGlobals);
            result.Globals = result.Globals.AddRange(variableStorage.GlobalVariables);
            result.Locals = result.Locals.AddRange(variableStorage.LocalVariables);
            result.ClassVariables = result.ClassVariables.AddRange(variableStorage.ClassVariables);
            result.LocalAccessibleGlobals = result.LocalAccessibleGlobals.AddRange(variableStorage.LocalAccessibleGlobals);
            return result;
        }

        public ImmutableVariableStorage Merge(ImmutableVariableStorage other)
        {
            Preconditions.NotNull(other, "other");

            var result = new ImmutableVariableStorage 
                         {
                             SuperGlobals = this.SuperGlobals.Merge(other.SuperGlobals),
                             Globals = this.Globals.Merge(other.Globals), 
                             Locals = this.Locals.Merge(other.Locals),
                             ClassVariables = this.ClassVariables.Merge(other.ClassVariables),
                             LocalAccessibleGlobals = this.LocalAccessibleGlobals.Merge(other.LocalAccessibleGlobals)
                         };
            return result;
        }

        public bool Equals(ImmutableVariableStorage other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var dictionaryComparer = new ImmutableDictionaryComparer<string, Variable>();
            return dictionaryComparer.Equals(SuperGlobals, other.SuperGlobals) &&
                   dictionaryComparer.Equals(Globals, other.Globals) &&
                   dictionaryComparer.Equals(Locals, other.Locals) &&
                   dictionaryComparer.Equals(ClassVariables, other.ClassVariables) &&
                   dictionaryComparer.Equals(LocalAccessibleGlobals, other.LocalAccessibleGlobals);

            return SuperGlobals.OrderBy(x => x.Key).SequenceEqual(other.SuperGlobals.OrderBy(x => x.Key)) &&
                   Globals.OrderBy(x => x.Key).SequenceEqual(other.Globals.OrderBy(x => x.Key)) &&
                   Locals.OrderBy(x => x.Key).SequenceEqual(other.Locals.OrderBy(x => x.Key)) &&
                   ClassVariables.OrderBy(x => x.Key).SequenceEqual(other.ClassVariables.OrderBy(x => x.Key)) &&
                   LocalAccessibleGlobals.OrderBy(x => x.Key).SequenceEqual(other.LocalAccessibleGlobals.OrderBy(x => x.Key));

            return SuperGlobals.SequenceEqual(other.SuperGlobals) &&
                   Globals.SequenceEqual(other.Globals) &&
                   Locals.SequenceEqual(other.Locals) &&
                   ClassVariables.SequenceEqual(other.ClassVariables) &&
                   LocalAccessibleGlobals.SequenceEqual(other.LocalAccessibleGlobals);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ImmutableVariableStorage);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SuperGlobals.GetHashCode();
                hashCode = (hashCode * 397) ^ Globals.GetHashCode();
                hashCode = (hashCode * 397) ^ Locals.GetHashCode();
                hashCode = (hashCode * 397) ^ ClassVariables.GetHashCode();
                hashCode = (hashCode * 397) ^ LocalAccessibleGlobals.GetHashCode();
                return hashCode;
            }
        }
    }
}