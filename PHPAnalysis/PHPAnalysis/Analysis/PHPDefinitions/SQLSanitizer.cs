using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis
{
    public sealed class SQLSanitizer : Function
    {
        public SQLITaint DefaultStatus { get; set; }

        public SQLSanitizer(string json) : this(JToken.Parse(json))
        {
            //Nothing. Just practical to provide both string and JToken version of ctor
        }

        public SQLSanitizer(JToken JSON) : base(JSON)
        {
            Parameters = new Dictionary<Tuple<uint,string>, Parameter>();

            SQLITaint tmp;
            bool success = Enum.TryParse((string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.DefaultStatusCode), out tmp);
            DefaultStatus = success ? tmp : SQLITaint.SQL_ALL;

            var paramsArray = (JArray) JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Parameters);
            foreach (JObject param in paramsArray)
            {
                var paramValues = (JArray)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues);
                var type = (string)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterType);
                var paramNumber = (uint)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterNumber);
                var isOptional = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsOptional);
                var variadic = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsVariadic);
                var isReturn = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsReturnValue);

                if (paramValues == null)
                {
                    var objectParam = new Parameter(isOptional ?? false, false, variadic ?? false, false, "", isReturn ?? false);
                    Parameters.Add(new Tuple<uint, string>(paramNumber, type), objectParam);
                    continue;
                }

                switch (type)
                {
                    case "flag":
                        var flag = FlagParameterFactory.CreateFlagParameter<SQLITaint>(paramValues, DefaultStatus,
                            isOptional: isOptional, isVaridic: variadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), flag);
                        break;
                    case "bool":
                    case "boolean":
                        var boolparam = BooleanParameterFactory.CreateBooleanParameter<SQLITaint>(paramValues, DefaultStatus,
                            isOptional: isOptional, isVariadic: variadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), boolparam);
                        break;
                    case "int":
                    case "integer":
                        var intParam = IntegerParameterFactory.CreateIntParameter<SQLITaint>(paramValues, DefaultStatus,
                            isOptional: isOptional, isVariadic: variadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint, string>(paramNumber, type), intParam);
                        break;
                    case "str":
                    case "string":
                        var strParam = StringParameterFactory.CreateStringParameter<SQLITaint>(paramValues, DefaultStatus,
                            isOptional: isOptional, isVariadic: variadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint, string>(paramNumber, type), strParam);
                        break;
                    case "array":
                        break;
                    case "object":
                        break;
                    default:
                        string s = String.Format("Unknown parameter type. Parameter number: {0} had the type {1}", paramNumber, type).ToString();
                        throw new NotSupportedException(s);
                }
            }
        }

        public SQLITaint GetSQLStatus(uint paramNumber, string paramValue = "")
        {
            var param = Parameters.FirstOrDefault(x => x.Key.Item1 == paramNumber);

            try
            {
                switch (param.Key.Item2)
                {
                    case "flag":
                        var flagVal = Int32.Parse(paramValue);
                        var flagParam = (FlagParameter<SQLITaint>)param.Value;
                        return flagParam.GetStatus(flagVal);
                    case "bool":
                    case "boolean":
                        var boolVal = Boolean.Parse(paramValue);
                        var boolParam = (BooleanParameter<SQLITaint>)param.Value;
                        return boolParam.GetStatus(boolVal);
                    case "int":
                    case "integer":
                        var intVal = Int32.Parse(paramValue);
                        var intParam = (IntegerParameter<SQLITaint>)param.Value;
                        return intParam.GetStatus(intVal);
                    case "str":
                    case "string":
                        var stringParam = (StringParameter<SQLITaint>)param.Value;
                        return stringParam.GetStatus(paramValue);
                    case "array":
                        break;
                    case "object":
                        break;
                    default:
                        return DefaultStatus;
                }
                return DefaultStatus;
            }
            catch(NullReferenceException e)
            {
                Debug.WriteLine("Could not fetch value, returning default. Exception was: {0}", e);
                return this.DefaultStatus;
            }
        }
    }
}