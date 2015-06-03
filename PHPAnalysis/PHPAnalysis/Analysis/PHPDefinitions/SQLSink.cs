using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis
{
    public sealed class SQLSink : Function
    {
        public SQLITaint DefaultStatus { get; private set;}
        public List<string> Classnames { get; set; }

        public SQLSink(string json) : this(JToken.Parse(json))
        {
            //Nothing! Just practical to have both string and JToken ctor
        }

        public SQLSink(JToken JSON) : base(JSON)
        {
            SQLITaint tmp;
            bool success = Enum.TryParse((string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.DefaultStatusCode), out tmp);
            DefaultStatus = success ? tmp : SQLITaint.SQL_ALL;

            this.Classnames = new List<string>();
            var classnames = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Classnames);
            if (classnames != null)
            {
                foreach (string format in classnames)
                {
                    this.Classnames.Add(format);
                }
            }

            var paramArray = (JArray) JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Parameters);
            foreach (JObject param in paramArray)
            {
                //if (param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues) == null) continue;

                var parameterNumber = (uint)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterNumber);
                var type = (string)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterType);
                var optional = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsOptional);
                var vulnerable = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterCanCreateHole);
                var paramValues = (JArray)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterValues);
                var variadic = (bool?)param.SelectToken(Keys.PHPDefinitionJSONKeys.ParameterJSONKeys.ParameterIsVariadic);

                switch (type)
                {
                    case "flag":
                        var flagParam = FlagParameterFactory.CreateFlagParameter<SQLITaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                 isVulnerable: vulnerable, isVaridic: variadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), flagParam);
                        break;
                    case "bool":
                    case "boolean":
                        var boolParam = BooleanParameterFactory.CreateBooleanParameter<SQLITaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                       isVulnerable: vulnerable, isVariadic: variadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), boolParam);
                        break;
                    case "int":
                    case "integer":
                        var intParam = IntegerParameterFactory.CreateIntParameter<SQLITaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                  isVulnerable: vulnerable, isVariadic: variadic);
                        Parameters.Add(new Tuple<uint, string>(parameterNumber, type), intParam);
                        break;
                    case "str":
                    case "string":
                        var strParam = StringParameterFactory.CreateStringParameter<SQLITaint>(paramValues, DefaultStatus, isOptional: optional,
                                                                                    isVulnerable: vulnerable, isVariadic: variadic);
                        Parameters.Add(new Tuple<uint,string>(parameterNumber, type), strParam);
                        break;
                    case "array":
                    case "object":
                    default: 
                        Parameters.Add(new Tuple<uint, string>(parameterNumber, type), new Parameter(optional ?? false, vulnerable ?? false));
                        break;
                }
            }
        }


        public SQLITaint GetTaintStatus(Dictionary<uint,string> arguments)
        {
            SQLITaint returnValue = SQLITaint.SQL_ALL;

            foreach (var arg in arguments)
            {
                SQLITaint tmp;

                var param = Parameters.FirstOrDefault(x => x.Key.Item1 == arg.Key);
                try
                {
                    switch (param.Key.Item2)
                    {
                        case "flag":
                            var flagVal = Int32.Parse(arg.Value);
                            var flagParam = (FlagParameter<SQLITaint>)param.Value;
                            tmp = (SQLITaint)flagParam.GetStatus(flagVal);
                            break;
                        case "bool":
                        case "boolean":
                            var boolVal = Boolean.Parse(arg.Value);
                            var boolParam = (BooleanParameter<SQLITaint>)param.Value;
                            tmp = (SQLITaint)boolParam.GetStatus(boolVal);
                            break;
                        case "int":
                        case "integer":
                            var intVal = Int32.Parse(arg.Value);
                            var intParam = (IntegerParameter<SQLITaint>)param.Value;
                            tmp = (SQLITaint)intParam.GetStatus(intVal);
                            break;
                        case "str":
                        case "string":
                            var stringParam = (StringParameter<SQLITaint>)param.Value;
                            tmp = (SQLITaint)stringParam.GetStatus(arg.Value);
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
                    Debug.WriteLine("Could not find value, returning default: Exception was: {0}", e);
                    return this.DefaultStatus;
                }
            }

            return returnValue;
        }
    }
}