// TODO: Major todo list here!
//
// AST:
//   Class/Function/Interface/Closure/Method/.. extractor:
//     ✔ Magic methods should be identified as magic methods.
//     - Func/Method extract: Default values for parameters: Can be Arrays, NULL or any constant expression!
//     - Method extract: Visibility modifiers (public, private, ..). Default is public.
//     - Class extract: Detect superclass/interfaces.
//     ✔ Class extract: Detect fields.
//   
// CFG:
//   Creator:
//     ✔ It should be possible to create CFG's for functions, methods and closures.
//
//   Pruner:
//     ✔ Pruning of unreachable CFG blocks
//     ✔ Removal of all/most of the empty nodes in the CFG (So far I think it works, haven't found a case that can fuck it up)
//
// Data/Internal storage:
//   Variabel Info:
//     ✔ Data structure for storing info about variables
//     - An array index COULD be an unknown variabel, in which case we still might be able to retrieve the taint info, if the same var is used as key later (without having been changed).
//     - Array indexes can be tainted!
//   Variable storage:
//     ✔ A place to store all variables and which ones are superglobals, locals, etc.
//   Vulnerability data:
//     - Data containing information about vulnerabilities
//       - Sink/Sanitization/Desanitization/Source information about each kind of vulnerability.
//         NOTE: Should be usable in the analysis. Should be extensible.
//   Vulnerability storage:
//     - When we have found a vuln, we need to store it somewhere.
//
// Analysis:
//   - SQL analysis -> DB Model
//
//   Variabel/Data analysis:
//     - Analysis of variables (Reaching def, type analysis, value analysis) 
//  
//   Taint analysis:
//     ✔ In/Out taint tracking on CFG blocks
//
//   Stored vuln analysis:
//     ✔ From the vulnerability storage, we need to see if there is any stored vulnerabilities.
//   
//   Summaries:
//     - Function/Method/Closure summaries
//     - File summaries (?)
//
// ✔ Basic inclusion resolving
//     NOTE: I'm thinking: Basic string concatenation, magic variables (__FILE__, etc).
//
//   Framework modelling:
//    - Working examples of what we want to support!
//    ✔ Implementation of models
//      NOTE: I'm thinking: Make our analysis extensible! e.g. Users can create a few C# classes that hook into our analysis and provide info.
//
//   Other:
//    - Dead code analysis (?) - Identify possible dead code (e.g. if(false))
//    - File "type" identification (?) - e.g. distinguish code files from "just function defs"-files
//    - FP detection - After analysis, we could try to detect possible FPs
//    
// Output:
//   Vulnerability reports:
//     NOTE: Should be extensible (i.e. some IVulnerabilityReporter interface)
//     - Console
//     ✔ File
//     - HTML (?)
//     - SQL (?)    <- Could be useful if we want to scan thousands of plugins
//     - ...
//
// 
// 
// 
// 
// 
// 
// 
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//