using System;

namespace PHPAnalysis
{
    public static class Keys
    {
        public static class PHPDefinitionJSONKeys
        {
            public static class FunctionSpecificationArrays
            {
                public const string Sources = "SourceFuncs";
                public const string XssSanitizer = "XSSSanitizersFuncs";
                public const string SqlSanitizer = "SQLSanitizersFuncs";
                public const string XssSinks = "XssSinkFuncs";
                public const string SqlSinks = "SqlSinksFuncs";
                public const string CondSinks = "ConditionSanitizerFuncs";
                public const string StoredVulnProviders = "StoredVulnerabilityProviders";
            }

            public static class GeneralKeys
            {
                public const string Name                        = "name";
                public const string Type                        = "type";
                public const string Formats                     = "formats";
                public const string Aliases                     = "aliases";
                public const string ParameterCount              = "totalParameters";
                public const string Parameters                  = "parameters";
                public const string DefaultStatusCode           = "defaultStatus";
                public const string OutputsPerDefault           = "defaultOutput";
                public const string ReturnType                  = "returnType";
                public const string XssTaint                    = "XSSTaint";
                public const string SqlTaint                    = "SQLTaint";
                public const string Classnames                  = "classnames";
            }

            public static class ParameterJSONKeys
            {
                public const string ParameterNumber             = "number";
                public const string ParameterType               = "type";
                public const string ParameterName               = "name";
                public const string ParameterIsOptional         = "optional";
                public const string ParameterValues             = "values";
                public const string ParameterCanCreateHole      = "can_be_vulnerable";
                public const string ParameterIsVariadic         = "IsVariadic";
                public const string ParameterIsReturnValue      = "isReturnValue";
            }

            public static class ValuesJSONKeys
            {
                public const string Value                       = "value";
                public const string IsOutputing                 = "outputs";
                public const string StatusCode                  = "outputStatus";
            }
        }
    }
}