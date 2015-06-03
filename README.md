#Eir #
PHP vulnerability scanner written in C#.  Why C#? Because! That's why!

### Requirements ###
* .NET 4.5 or Mono 3.12.0-1
* PHP >= 5.3
* Good intentions

### Getting it running ###
1. Download project  
2. Install [Composer](https://getcomposer.org/) (for the PHP parser)  
    2.1. From the PHPAnalysis-parser folder run composer update in terminal
    2.2. Make sure all dependencies are correctly installed.
3. Fix the config.yml 
4. Run the application

### Errors ###

A collection of possible errors you might experience while setting up/using this application and possible ways to mitigate them. 

-----------------------
####"Parser error: Syntax error, unexpected EOF on line xx"  ####
(PHP project throws this error)

Make sure that there is no syntactical errors in your PHP code. This scanner does not handle syntax errors very well.

If the syntax is correct, try updating to the newest version of PHP.

-----------------------
#### CONFIGURATION ERROR ####
Make sure the configuration file (named Config.yml) is present and has the correct format. It should look like the config-template file.
Standard rules for Yaml files apply.

Common error:
Tabs instead of spaces. Indentation should be done using spaces, since Yaml does not accept tabs.
-----------------------
### External dependencies ###

- CommandLineParser 1.9.71
- Newtonsoft.Json 6.0.0
- PHP-Parser 1.1.0 
- QuickGraph 3.6.61119.7
- System.Collections.Immutable 1.0.34
- YamlDotNet 3.5.1
 
- Moq 4.2.1502
- NUnit 2.6.4
 