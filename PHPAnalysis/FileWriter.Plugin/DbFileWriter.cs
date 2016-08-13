using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Analysis;
using PHPAnalysis.Analysis.AST;
using PHPAnalysis.Analysis.PHPDefinitions;

namespace FileWriter.Plugin
{
    public class DbFileWriter
    {
        private string vulnDBFile = "ScanResultForDB.txt";
        private readonly string _stackSeperator = Environment.NewLine + " → ";
        private FunctionsHandler _funcHandler;

        public void RegisterFunctionsHandler(FunctionsHandler functionsHandler)
        {
            _funcHandler = functionsHandler;
        }

        public void WriteStart(string target)
        {
            WriteInfoLine(Environment.MachineName + ";" + Environment.UserName + ";");
            WriteInfo(target + ";");
        }

        public void WriteEnd(TimeSpan time)
        {
            WriteInfo("TIME;" + time.TotalSeconds);
        }

        public void WriteVulnerability(IVulnerabilityInfo vuln)
        {
            string vulnType = GetVulnType(vuln.Message);

            WriteInfo(vulnType + ";");

            WriteInfoLine("Message: " + vuln.Message);
            WriteInfoLine("Include stack:" + String.Join(_stackSeperator, vuln.IncludeStack));
            WriteInfo("Call stack: " + String.Join(_stackSeperator, vuln.CallStack.Select(c => c.Name)));
            WriteFilePath(vuln);
            WriteInfo(";");
        }

        public void WriteStoredVulnerability(IVulnerabilityInfo[] vulnerabilityPathInfos)
        {
            int pair = 0;
            foreach (var pathInfo in vulnerabilityPathInfos)
            {
                pair = pair % 2;
                if (pair == 0)
                {
                    string vulnType = GetVulnType(pathInfo.Message);
                    WriteInfo(vulnType + ";");
                    WriteInfo("Message: ");
                }
                
                WriteInfoLine(pathInfo.Message);
                WriteInfoLine(String.Join(_stackSeperator, pathInfo.IncludeStack));
                WriteInfo("Callstack: " + String.Join(_stackSeperator, pathInfo.CallStack.Select(c => c.Name)));
                WriteFilePath(pathInfo);
                if (pair == 1)
                {
                    WriteInfo(";");
                }
                pair++;
            }
        }

        private string GetVulnType(string message)
        {
            if (message.StartsWith("Stored XSS found"))
            {
                return "StoredXSS";
            }
            if (message.StartsWith("Stored SQLI found"))
            {
                return "StoredSQLI";
            }
            if (message.StartsWith("SQL vulnerability found"))
            {
                return "SQLI";
            }
            if (message.StartsWith("XSS vulnerability found"))
            {
                return "XSS";
            }
            if (message.StartsWith("Tainted outgoing"))
            {
                return "";
            }
            throw new Exception("Unknown vulntype found. Something went wrong! Message was: " + message);
            
        }

        public void WriteFilePath(IVulnerabilityInfo vulnInfo)
        {
            var funcList = vulnInfo.CallStack.Any() ? _funcHandler.LookupFunction(vulnInfo.CallStack.Peek().Name) : null;
            if (funcList == null || !funcList.Any())
            {
                return;
            }
            if (funcList.Count == 1)
            {
                var str = "Function/method: " + funcList.First().Name +
                          (string.IsNullOrWhiteSpace(funcList.First().File) ? "" : Environment.NewLine + "In file: " + funcList.First().File);
                WriteInfo(str);
            }
            else
            {
                WriteInfo("Function/method: " + funcList.First().Name + Environment.NewLine
                          + "File candidates: " + Environment.NewLine
                          + string.Join(Environment.NewLine, funcList.Select(x => x.File)));
            }
        }

        private void WriteInfo(string info)
        {
            File.AppendAllText(vulnDBFile, info);
        }

        private void WriteInfoLine(string info)
        {
            WriteInfo(info);
            WriteInfo(Environment.NewLine);
        }
    }
}
