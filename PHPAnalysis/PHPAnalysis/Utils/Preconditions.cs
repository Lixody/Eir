using System;
using System.Diagnostics;

namespace PHPAnalysis.Utils
{
    internal static class Preconditions
    {
        [DebuggerHidden]
        public static T NotNull<T>(T parameter, string parameterName) where T : class
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return parameter;
        }

        [DebuggerHidden]
        public static void IsTrue(bool condition, string message = "", string parameterName = "")
        {
            if ( !condition )
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerHidden]
        public static void IsFalse(bool condition, string message = "", string parameterName = "")
        {
            if (condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }
    }
}
