using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis
{
    public sealed class XSSSink : Function
    {
        public XSSTaint DefaultStatus { get; private set; }
        public bool Outputs { get; set; }

        public XSSSink(string json) : this(JToken.Parse(json))
        {
            //Nothing here, just practical to have both the string and JToken version of ctor
        }

        public XSSSink(JToken JSON) : base(JSON)
        {
            Parameters = new Dictionary<Tuple<uint, string>, Parameter>();

            XSSTaint tmp;
            bool success = Enum.TryParse((string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.DefaultStatusCode), out tmp);
            DefaultStatus = success ? tmp : XSSTaint.XSS_ALL;

            Outputs = (bool)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.OutputsPerDefault);

            var parameters = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Parameters);
            foreach (JObject parameterObject in parameters)
            {
                var parameterNumber = (uint)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterNumber);
                var optional = (bool?)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsOptional);
                var type = (string)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterType);
                var paramValues = (JArray)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues);
                var vulnerable = (bool?)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterCanCreateHole);
                var isVariadic = (bool?)parameterObject.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsVariadic);
                //TODO: Add support for return parameters

                switch (type)
                {
                    case "flag":
                        var flagParam = FlagParameterFactory.CreateFlagParameter<XSSTaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                 isVulnerable:vulnerable, isVaridic: isVariadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), flagParam);
                        break;
                    case "bool":
                    case "boolean":
                        var boolParam = BooleanParameterFactory.CreateBooleanParameter<XSSTaint>(paramValues, DefaultStatus, isOptional: optional, 
                                                                                       isVulnerable: vulnerable, isVariadic: isVariadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), boolParam);
                        break;
                    case "int":
                    case "integer":
                        var intParam = IntegerParameterFactory.CreateIntParameter<XSSTaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                  isVulnerable: vulnerable, isVariadic: isVariadic);
                        Parameters.Add(new Tuple<uint, string>(parameterNumber, type), intParam);
                        break;
                    case "str":
                    case "string":
                        var strParam = StringParameterFactory.CreateStringParameter<XSSTaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                    isVulnerable: vulnerable, isVariadic: isVariadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), strParam);
                        break;
                    case "array":
                    case "object":
                    case "mix":
                        var tuple = new Tuple<uint,string>(parameterNumber, type);
                        Parameters.Add(tuple, new Parameter(optional ?? false, vulnerable ?? true, isVariadic ?? false, false, "", false));
                        break;
                    default:
                        string s = String.Format("Unknown parameter type. Parameter number: {0} had the type {1}", parameterNumber, type).ToString();
                        throw new NotSupportedException(s);
                }
            }
        }

        public XSSTaint GetTaintStatus(Dictionary<uint,string> arguments)
        {
            XSSTaint returnValue = XSSTaint.XSS_ALL;

            foreach (var arg in arguments)
            {
                var param = Parameters.FirstOrDefault(x => x.Key.Item1 == arg.Key);
                XSSTaint tmp;

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
                    if(tmp < returnValue)
                        returnValue = tmp;
                }
                catch (NullReferenceException e)
                {
                    return this.DefaultStatus;
                }
            }

            return returnValue;
        }
    }
}