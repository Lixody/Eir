using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis
{
    public sealed class XSSSanitizer : Function
    {
        public XSSTaint DefaultStatus { get; private set; }

        public XSSSanitizer(string json) : this(JToken.Parse(json))
        {
            //Nothing here, just practical to have both string and jtoken version of ctor
        }

        public XSSSanitizer(JToken JSON) : base(JSON)
        {
            Parameters = new Dictionary<Tuple<uint,string>, Parameter>();

            XSSTaint tmp;
            bool success = Enum.TryParse((string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.DefaultStatusCode), out tmp);
            DefaultStatus = success ? tmp : XSSTaint.XSS_ALL;   

            var paramsArray = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Parameters);

            foreach (JObject param in paramsArray)
            {
                var paramNumber = (uint)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterNumber);
                var type = (string)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterType);
                var isOptional = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsOptional);
                var isVariadic = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsVariadic);
                var paramValues = (JArray)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues);
                var isReturn = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsReturnValue);

                if (param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues) == null)
                {
                    var parameter = new Parameter(isOptional ?? false, false, isVariadic ?? false, false, "", isReturn ?? false);
                    Parameters.Add(new Tuple<uint, string>(paramNumber, type), parameter);
                    continue;
                }

                switch (type)
                {
                    case "flag":
                        var flag = FlagParameterFactory.CreateFlagParameter<XSSTaint>(paramValues, this.DefaultStatus,
                            isOptional: isOptional, isVaridic: isVariadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), flag);
                        break;
                    case "bool":
                    case "boolean":
                        var boolparam = BooleanParameterFactory.CreateBooleanParameter<XSSTaint>(paramValues, this.DefaultStatus,
                            isOptional: isOptional, isVariadic: isVariadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), boolparam);
                        break;
                    case "int":
                    case "integer":
                        var intParam = IntegerParameterFactory.CreateIntParameter<XSSTaint>(paramValues, this.DefaultStatus,
                            isOptional: isOptional, isVariadic: isVariadic, isReturn: isReturn);
                        Parameters.Add(new Tuple<uint, string>(paramNumber, type), intParam);
                        break;
                    case "str":
                    case "string":
                        var strParam = StringParameterFactory.CreateStringParameter<XSSTaint>(paramValues, this.DefaultStatus,
                            isOptional: isOptional, isVariadic: isVariadic, isReturn: isReturn);
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

        public override string ToString()
        {
            var str = base.ToString();
            var sb = new StringBuilder();
            sb.Append(str);
            sb.AppendLine("Parameters:");
            foreach (var param in this.Parameters)
            {
                sb.AppendLine(string.Format("    Parameternumber: {0}, Parametertype: {1}", param.Key.Item1, param.Key.Item2));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the return value from a dictionary of the parameter index and the value.
        /// If no higher is found the default status i returned.
        /// </summary>
        /// <returns>The status code found</returns>
        /// <param name="arguments">The dictionary of argument values</param>
        public XSSTaint GetTaintStatus(Dictionary<uint, string> arguments)
        {
            XSSTaint returnValue = this.DefaultStatus;

            foreach (var arg in arguments)
            {
                var param = Parameters.FirstOrDefault(x => x.Key.Item1 == arg.Key);
                XSSTaint tmp = this.DefaultStatus;
                try
                {
                    switch (param.Key.Item2)
                    {
                        case "flag":
                            var flagVal = Int32.Parse(arg.Value);
                            var flagParameter = (FlagParameter<XSSTaint>)param.Value;
                            tmp = (XSSTaint)flagParameter.GetStatus(flagVal);
                            break;
                        case "bool":
                        case "boolean":
                            var boolVal = Boolean.Parse(arg.Value);
                            var booleanParam = (BooleanParameter<XSSTaint>)param.Value;
                            tmp = (XSSTaint)booleanParam.GetStatus(boolVal);
                            break;
                        case "int":
                        case "integer":
                            var intVal = Int32.Parse(arg.Value);
                            var intParam = (IntegerParameter<XSSTaint>)param.Value;
                            tmp = (XSSTaint)intParam.GetStatus(intVal);
                            break;
                        case "str":
                        case "string":
                            var strParam = (StringParameter<XSSTaint>)param.Value;
                            tmp = (XSSTaint)strParam.GetStatus(arg.Value);
                            break;
                        case "array":
                        case "object":
                        default:
                            continue;
                    }
                }
                catch (NullReferenceException e)
                {
                    return this.DefaultStatus;
                }
                if (tmp < returnValue)
                {
                    returnValue = tmp;
                }
            }
            return returnValue;
        }
    }
}