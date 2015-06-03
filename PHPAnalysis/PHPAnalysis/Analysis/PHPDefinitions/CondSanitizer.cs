using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis.Analysis.PHPDefinitions
{
    public sealed class CondSanitizer : Function
    {
        public MixedStatus DefaultStatus { get; private set; }

        public CondSanitizer(string json) : this(JToken.Parse(json))
        {
            //Nothing here, just practical to have both string and jtoken version of ctor
        }

        public CondSanitizer(JToken JSON) : base(JSON)
        {
            Parameters = new Dictionary<Tuple<uint,string>, Parameter>();

            MixedStatus tmp;
            bool defaultStatusCode = Enum.TryParse((string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.DefaultStatusCode), out tmp);
            DefaultStatus = defaultStatusCode ? tmp : MixedStatus.XSSSQL_UNSAFE;   

            var paramsArray = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Parameters);

            foreach (JObject param in paramsArray)
            {
                if (param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues) == null) continue;

                var paramNumber = (uint)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterNumber);
                var type = (string)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterType);
                var isOptional = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsOptional);
                var isVariadic = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsVariadic);
                var paramValues = (JArray)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues);

                switch (type)
                {
                    case "flag":
                        var flag = FlagParameterFactory.CreateFlagParameter<MixedStatus>(paramValues, MixedStatus.XSSSQL_UNSAFE,
                                                                                       isOptional: isOptional, isVaridic: isVariadic);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), flag);
                        break;
                    case "bool":
                    case "boolean":
                        var boolparam = BooleanParameterFactory.CreateBooleanParameter(paramValues, MixedStatus.XSSSQL_UNSAFE,
                                                                                       isOptional: isOptional, isVariadic: isVariadic);
                        Parameters.Add(new Tuple<uint,string>(paramNumber, type), boolparam);
                        break;
                    case "int":
                    case "integer":
                        var intParam = IntegerParameterFactory.CreateIntParameter(paramValues, MixedStatus.XSSSQL_UNSAFE,
                                                                                  isOptional: isOptional, isVariadic: isVariadic);
                        Parameters.Add(new Tuple<uint, string>(paramNumber, type), intParam);
                        break;
                    case "str":
                    case "string":
                        var strParam = StringParameterFactory.CreateStringParameter(paramValues, MixedStatus.XSSSQL_UNSAFE,
                                                                                    isOptional: isOptional, isVariadic: isVariadic);
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
        /// Method to find the XSSStatus for a parameter number and the currently known value of the parameter
        /// </summary>
        /// <returns>The most fitting XSSStatus for the parameter</returns>
        /// <param name="parameterNumber">The number of the parameter</param>
        /// <param name="value">The currently known parameter value</param>
        public MixedStatus GetReturnStatus(uint parameterNumber, string value = "")
        {
            var param = Parameters.FirstOrDefault(x => x.Key.Item1 == parameterNumber);

            try
            {
                switch (param.Key.Item2)
                {
                    case "flag":
                        var flagVal = Int32.Parse(value);
                        var flagParameter = (FlagParameter<MixedStatus>)param.Value;
                        return (MixedStatus)flagParameter.GetStatus(flagVal);
                    case "bool":
                    case "boolean":
                        var boolVal = Boolean.Parse(value);
                        var booleanParam = (BooleanParameter<MixedStatus>)param.Value;
                        return (MixedStatus)booleanParam.GetStatus(boolVal);
                    case "int":
                    case "integer":
                        var intVal = Int32.Parse(value);
                        var intParam = (IntegerParameter<MixedStatus>)param.Value;
                        return (MixedStatus)intParam.GetStatus(intVal);
                    case "str":
                    case "string":
                        var strParam = (StringParameter<MixedStatus>)param.Value;
                        return (MixedStatus)strParam.GetStatus(value);
                    case "array":
                    case "object":
                    default:
                        return this.DefaultStatus;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine("Could not fetch value, returning default status. Exception was: {0}", e);
                return this.DefaultStatus;
            }
        }
    }
}
