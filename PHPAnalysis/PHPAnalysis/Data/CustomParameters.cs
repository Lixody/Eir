using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.PHP;

namespace PHPAnalysis
{
    //TODO: this should actually handle flags the correct way
    public class FlagParameter<T> : Parameter where T : struct, IComparable, IConvertible, IFormattable
    {
        //The key field is the possible flag values.
        //The Value field is the status code that match the key field.
        public Dictionary<int, T> FlagValues { get; set; }

        public FlagParameter(Dictionary<int, T> values)
        {
            FlagValues = values;
        }

        public FlagParameter(Dictionary<int, T> values, bool optional, bool vulnerable, bool variadic, bool isReturn) : this(values)
        {
            this.IsOptional = optional;
            this.IsSensitive = vulnerable;
            this.IsVariadic = variadic;
            this.IsReturn = isReturn;
        }

        /// <summary>
        /// Method to get the status code from the currently known flag value.
        /// </summary>
        /// <returns>The status code for the flag value</returns>
        /// <param name="codeVal">The currently known flag value</param>
        public T GetStatus(int codeVal)
        {
            //TODO: This should actually be handled.
            return FlagValues.First(x => x.Key == codeVal).Value;
        }
    }

    public static class FlagParameterFactory
    {
        /// <summary>
        /// Method to create a new instance of flag parameter with the Enum status codes as the generic type.
        /// </summary>
        /// <returns>A new instance of flag parameter built from the JSON values data provided</returns>
        /// <param name="parameterValues">JSON formated values array</param>
        /// <param name="defaultStatus">The default status code to use if parsing of it fails</param>
        /// <param name="isOptional">Optional parameter to indicate whether this parameter is optional (default is false)</param>
        /// <param name="isVulnerable">Optional parameter to indicate if this parameter can create a security issue (default is true, only useful in case of sinks)</param>
        /// <param name="isVaridic">Optional parameter to indicate whether this parameter is variadic</param>
        /// <typeparam name="T">Should be type of Enum and should hold status codes</typeparam>
        public static FlagParameter<T> CreateFlagParameter<T>(JArray parameterValues, T defaultStatus,
            bool? isOptional = false, bool? isVulnerable = true, bool? isVaridic = false, bool? isReturn = false)
            where T: struct, IComparable, IConvertible, IFormattable
        {
            var dict = new Dictionary<int, T>();
            foreach(JObject valueObj in parameterValues)
            {
                var value = (int)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.Value);
                if (dict.Keys.Any(x => x == value))
                    continue;
                var outputStatusStr = (string)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.StatusCode);
                var outputStatus = defaultStatus;
                Enum.TryParse(outputStatusStr, out outputStatus);
                dict.Add(value, outputStatus);
            }

            return new FlagParameter<T>(dict, isOptional ?? false, isVulnerable ?? true, isVaridic ?? false, isReturn ?? false);
        }
    }


    public class BooleanParameter<T> : Parameter where T : struct, IComparable, IConvertible, IFormattable
    {
        //The status output when the bool is true
        public T TrueStatus { get; set; }
        //The status output when the bool is false
        public T FalseStatus { get; set; }

        public BooleanParameter(T trueStatus, T falseStatus)
        {
            TrueStatus = trueStatus;
            FalseStatus = falseStatus;
        }

        public BooleanParameter(T trueStatus, T falseStatus, bool optional, bool isVulnerable, bool isVariadic, bool isReturn) : this(trueStatus, falseStatus)
        {
            this.IsOptional = optional;
            this.IsSensitive = isVulnerable;
            this.IsVariadic = isVariadic;
            this.IsReturn = isReturn;
        }

        /// <summary>
        /// Mehtod to get the status from a known value.
        /// </summary>
        /// <returns>The statuscode (should be parsed to the correct enum)</returns>
        /// <param name="currentValue">If which is <c>true</c> then the TrueStatus is returned. otherwise falsevalue is returned</param>
        public T GetStatus(bool currentValue)
        {
            return currentValue ? TrueStatus : FalseStatus;
        }
    }

    public static class BooleanParameterFactory
    {
        /// <summary>
        /// Method to create a new instance of the boolean parameter from the JSON specification.
        /// </summary>
        /// <returns>A new instance of the BooleanParameter</returns>
        /// <param name="parameterValues">The JSON array specifying the values for the boolean</param>
        /// <param name="defaultStatusCode">Default status code to use if parsing fails</param>
        /// <param name="isOptional">Optional parameter to indicate whether this parameter is optional (default is false)</param>
        /// <param name="isVulnerable">Optional parameter to indicate whether this parameter can create a security issue (default is true, only useful in case of sink)</param>
        /// <param name="isVariadic">Indicates whether this parameter is variadic</param>
        /// <typeparam name="T">The status code type to use, (Should be type of Enum)</typeparam>
        public static BooleanParameter<T> CreateBooleanParameter<T>(JArray parameterValues, T defaultStatusCode,
            bool? isOptional = false, bool? isVulnerable = true, bool? isVariadic = false, bool? isReturn = false)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            T trueStatus = defaultStatusCode;
            T falseStatus = defaultStatusCode;

            if (parameterValues != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var currentVal = (bool)parameterValues[i].SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.Value);
                    var outputStr = (string)parameterValues[i].SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.StatusCode);
                    var outputStatus = defaultStatusCode;
                    Enum.TryParse(outputStr, out outputStatus);

                    if (currentVal)
                        trueStatus = outputStatus;
                    else
                        falseStatus = outputStatus;
                }
            }

            var boolParam = new BooleanParameter<T>(trueStatus, falseStatus, isOptional ?? false, isVulnerable ?? true, isVariadic ?? false, isReturn ?? false);
            return boolParam;
        }
    }

    public class IntegerParameter<T> : Parameter where T : struct, IComparable, IFormattable, IConvertible
    {
        //Key field is the value of the integer, Value field is the matching status code.
        public Dictionary<int,T> IntValues { get; set; }

        public IntegerParameter()
        {
            IntValues = new Dictionary<int,T>();
        }

        public IntegerParameter(Dictionary<int,T> values)
        {
            IntValues = values;
        }

        public IntegerParameter(Dictionary<int, T> values, bool optional, bool isVulenerable, bool isVariadic, bool isReturn) : this(values)
        {
            this.IsOptional = optional;
            this.IsSensitive = isVulenerable;
            this.IsVariadic = isVariadic;
            this.IsReturn = isReturn;
        }

        /// <summary>
        /// Method to get the status code from the currently known value
        /// </summary>
        /// <returns>The status for the known int value</returns>
        /// <param name="currentValue">Currently known value</param>
        public T GetStatus(int currentValue)
        {
            //TODO: Should probably be changed to return a defualt value in case the item does not exists.
            var entry = IntValues.First(x => x.Key == currentValue);
            return entry.Value;
        }

        public void AddValueStatus(int value, T statusCode)
        {
            IntValues.Add(value, statusCode);
        }
    }

    public static class IntegerParameterFactory
    {
        // <summary>
        // Create a new IntParameter from the JSON specification
        // </summary>
        // <returns>A new IntParameter from the values array given</returns>
        // <param name="paramValues">The possible candidates given in the values array</param>
        // <param name="isOptional">Indicates whether or not this parameter is optional</param>

        /// <summary>
        /// Method to create a new instance of the IntegerParameter from the JSON specification
        /// </summary>
        /// <returns>A new instance of the IntegerParameter class</returns>
        /// <param name="parameterValues">The JSON specified values</param>
        /// <param name="defaultStatusCode">The default status code to use if parsing fails</param>
        /// <param name="isOptional">Optional parameter to indicate whether this parameter is optional (Default is false)</param>
        /// <param name="isVulnerable">Optional parameter to indicate whether this parameter can create a security issue (defualt is true, mostly useful in case of sink)</param>
        /// <param name="isVariadic">Indicates whether this parameter is variadic or not</param>
        /// <typeparam name="T">The type of the status codes (Should be the Enum status codes)</typeparam>
        public static IntegerParameter<T> CreateIntParameter<T>(JArray parameterValues, T defaultStatusCode,
            bool? isOptional = false, bool? isVulnerable = true, bool? isVariadic = false, bool? isReturn = false)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            var dict = new Dictionary<int, T>();
            if (parameterValues != null)
            {
                foreach (JObject valueObj in parameterValues)
                {
                    var possibleValue = (int)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.Value);
                    if (dict.Keys.Any(x => x == possibleValue))
                        continue;

                    var outputStr = (string)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.StatusCode);
                    var outputStatus = defaultStatusCode;
                    Enum.TryParse(outputStr, out outputStatus);
                    dict.Add(possibleValue, outputStatus);
                }
            }
            var intParameter = new IntegerParameter<T>(dict, isOptional ?? false, isVulnerable ?? true, isVariadic ?? false, isReturn ?? false);
            return intParameter;
        }
    }

    public class StringParameter<T> : Parameter where T : struct, IConvertible, IComparable, IFormattable
    {
        //Status code dictionary. The key field is the string value, and the value field is the status code for the string.
        public Dictionary<string,T> StringValues { get; set; }

        public StringParameter()
        {
            StringValues = new Dictionary<string, T>();
        }

        public StringParameter(Dictionary<string,T> values)
        {
            StringValues = values;
        }

        public StringParameter(Dictionary<string,T> values, bool optional, bool isVulnerable, bool isVariadic, bool isReturn) : this(values)
        {
            this.IsOptional = optional;
            this.IsSensitive = isVulnerable;
            this.IsVariadic = isVariadic;
            this.IsReturn = isReturn;
        }

        /// <summary>
        /// Method to get the status code from a known string value.
        /// </summary>
        /// <returns>The status code for the known string value</returns>
        /// <param name="value">The currently known string value</param>
        public T GetStatus(string value)
        {
            //TODO: Should be changed to return a default when the string value does not exists.
            return StringValues.First(x => x.Key == value).Value;
        }

        public void AddStringValue(string value, T statusCode)
        {
            StringValues.Add(value, statusCode);
        }
    }

    public static class StringParameterFactory
    {
        // <summary>
        // 
        // </summary>
        // <returns></returns>
        // <param name="paramValues"></param>
        // <param name="isOptional">Indicates whether or not this parameter is optional</param>

        /// <summary>
        /// Creates a new StringParamter from the JSON specification.
        /// </summary>
        /// <returns>A new StringParameter created from the JSON array given</returns>
        /// <param name="parameterValues">Candidate values given in the values array</param>
        /// <param name="defaultStatusCode">Default status code to use when parsing fails</param>
        /// <param name="isOptional">Optional parameter to indicate whether this parameter is optional (default is false)</param>
        /// <param name="isVulnerable">Optional parameter to indicate whether this parameter can create a security issue (default is true, most useful in case of sink)</param>
        /// <param name="isVariadic">Indicates whether this parameter is variadic</param>
        /// <typeparam name="T">The status code type (Should be of type Enum)</typeparam>
        public static StringParameter<T> CreateStringParameter<T>(JArray parameterValues, T defaultStatusCode,
            bool? isOptional = false, bool? isVulnerable = true, bool? isVariadic = false, bool? isReturn = false)
            where T : struct, IComparable, IConvertible, IFormattable
        {
            var dict = new Dictionary<string, T>();
            if (parameterValues != null)
            {
                foreach (JObject valueObj in parameterValues)
                {
                    var possibleValue = (string)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.Value);
                    if (dict.Keys.Any(x => x == possibleValue))
                        continue;

                    var outputStr = (string)valueObj.SelectToken(Keys.PHPDefinitionJSONKeys.ValuesJSONKeys.StatusCode);
                    var outputStatus = defaultStatusCode;
                    Enum.TryParse(outputStr, out outputStatus);
                    dict.Add(possibleValue, outputStatus);
                }
            }

            var stringParam = new StringParameter<T>(dict, isOptional ?? false, isVulnerable ?? true, isVariadic ?? false, isReturn ?? false);
            return stringParam;
        }
    }

    public class ArrayParameter : Parameter
    {
        //TODO: Dunno what i should do here? Does it make sense to implement this ?
    }

    public class ObjectParameter : Parameter
    {
        //TODO: Dunno what i should do here? Does it make sense to implement this ? 
    }

    public class FloatParameter : Parameter
    {
        //TODO: Dunno what i should do here? Does it make sense to implement this ? 
    }
}