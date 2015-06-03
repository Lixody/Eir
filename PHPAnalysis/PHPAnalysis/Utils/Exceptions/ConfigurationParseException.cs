using System;

namespace PHPAnalysis.Utils.Exceptions
{
    public sealed class ConfigurationParseException : Exception
    {
        public ConfigurationParseException() { }
        public ConfigurationParseException(string message) : base(message) { }

        public ConfigurationParseException(string message, Exception inner) : base(message, inner) { }
    }
}