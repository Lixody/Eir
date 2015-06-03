# PHP JSON specification

## General explanation

The format for specifying functions from PHP is JSON. As JSON is key-value pair, some general keys had to be chosen for functions, sources and parameters. These are explained individually later on in this document. The keys can be found in the PHPAnalysis/PHPAnalysis/Provider/keys.cs file.

## Functions

The following will be set in the FunctionDef constructor and therefore can be specified in the JSON definitions:

*** Required key-value pairs: ***

* Key: _name_ (The function name)
* Key: _totalParameters_ (The total number of parameters (set 0 if none))
* Key: _returnType_ (The type that this function returns (set 'none' if the function does not return))

*** Optional key-value pairs: ***

* Key: _formats_ (string array to define call formats of the function)
* Key: _aliases_ (String array to define function aliases of the function)

*** JSON Examples: ***

## Parameters

The following will be set in the ParameterDef constructor and therefore can be specified in the JSON Definitions:

*** Required key-value pairs: ***

* Key: _name_ (the parameter name (mostly for debugging and to make it human readable))
* Key: _type_ (Sting to define the parameter type (such as int, string, object etc.))
* Key: _number_ (The parameter index)

*** Optional key-value pairs ***

* Key: _optional_ (bool to indicate whether this parameter is optional)
* Key: _IsVariadic_ (bool to indicate whether this parameter is variadic)
* Key: _can\_be\_vulnerable (bool to indicate whether this parameter can create a security issue)

### Derived classes from ParameterDef

When the type is defined the function specification will try to create an instance of the sub-class of ParameterDef, and therefore a values array can be specified for a parameter.

*** Optional key-value pairs: ***

* Key: _values_ (array to specify status codes from parameter values consult [XSS Sanitizers](#XSS-Sanitizers) to see example usages)

## Value object

As stated parameters can contain a values array. Therefore some key-value pair are chosen to specify what values affect status codes. The key-value pairs are as follows:

*** Required key-valeu pairs ***

* Key: _value_ (The value (should obey the parameter type of the parameter))

*** Optional key-value pairs ***

* Key: _outputs_ (bool to indicate whether the parameter controls screen output)
* Key: _outputStatus_ (Status code to indicate the output status (mostly return value) if the parameter is changing the ouput status code)

## Status Codes

The status codes are the safety codes that is used throughout the analysis. These are defined in the PHPAnalysis/PHPAnalysis/Analysis/PHPDefinitions/SafetyTypes.cs

The JSON specified status codes should be using these status codes. If the status code given in JSON cannot be parsed, the unsafe status code will be used.

## XSS Sanitizers

XSS sanitizers are assumed to be all functions, and therefore the XSS sanitizer class is a derived class from the FunctionDef class. The FunctionDef required key-value pairs should therefore be fulfilled. An XSS sanitizer can specify parameters and thereby have value arrays. These are used for specifying status codes depending on variables given to the function.

    {
        "name":"htmlentities",
        "formats": [ "htmlentities(string, int, string, bool)" ],
        "totalParameters":4,
        "parameters": [
            { "number":1, "name":"string", "type":"object" },
            { "number":2, "name":"flags", "type":"flag", "values": [
                    {"value":2, "outputStatus":"XSS_ESCAPE_DOUBLEQOUTES" },
                    {"value":3, "outputStatus":"XSS_ESCAPE_ALLQOUTES" },
                    {"value":0, "outputStatus":"XSS_ESCAPE_NOQUOTES" },
                    {"value":4, "outputStatus":"XSS_UNSAFE" },
                    {"value":8, "outputStatus":"XSS_INVALID_TO_UNICODE" },
                    {"value":128, "outputStatus":"XSS_INVALID_TO_UNICODE" }
                ]
            },
            { "number":3, "name":"encoding", "type":"string" },
            { "number":4, "name":"double_encode", "type":"bool" }
        ],
        "returnType":"string",
        "defaultStatus":"XSS_SAFE"
    }

In this JSON the second parameter is a flag parameter therefore each value that the flag can have has to be set together with an outputStatus. Also the XSS sanitizer includes two more required key-value pairs as given here:

*** Required key-value pairs: ***

* Key: _returnType_ (the type that the function returns)
* Key: _defaultStatus_ (The default status code to use if no fitting value can be found)

## XSS Sink

The XSS Sinks is derived from the FunctionDef

    {
        "name":"print_r",
        "formats":[ "print_r(object, bool)", "print_r(object)" ],
        "totalParameters":2,
        "parameters": [
            { "number":1, "type":"object", "name":"expression" },
            { "number":2, "type":"bool", "name":"return", "optional":true, "values": [
                    {"value":true, "outputs":false },
                    {"value":false, "outputs":true }
                ]
            },
        ],
        "defaultOutput":true,
        "returnType":"string",
        "defaultStatus":"XSS_UNSAFE"
    }

The XSS Sink introduces some new key-value pairs

*** Required key-value pairs: ***

* Key: _defaultOutput_ (bool to indicate whether this function prints as default)
* Key: _returnType_ (string to indicate type (should be none if the functions does not output))
* Key: _defaultStatus_ (Status code to indicate how safe output is as default)

## SQL Sanitizer

Is defined the same way as XSS sanitizers, with the exception of XSS status codes should be SQL status codes instead.

## SQL Sink

The SQL Sink works nearly the same way as the XSS Sink does. It is also a derived class from FunctionDef, and therefore has the same required key-value pairs.

    {
        "name":"mysqli_query",
        "formats":[ "mysqli_query(mixed, string)" ],
        "totalParameters":2,
        "parameters":[
            { "number":1, "type":"object", "name":"dbLink" },
            { "number":2, "type":"string", "name":"query", "can\_be\_vulnerable":true }
        ],
        "returnType":"array"
    }

The SQL sink introduces the following additional key-value pairs:

*** Required key-value pairs: ***

* Key: _returnType_ (string to specify the type of the output of the function)
