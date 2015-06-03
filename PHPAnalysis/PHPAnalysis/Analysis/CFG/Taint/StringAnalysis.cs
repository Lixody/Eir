using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public static class StringAnalysis
    {
        /// <summary>
        /// Checks whether the given string statement is an SQL insertion statement
        /// Examples: INSERT INTO, UPDATE
        /// </summary>
        public static bool IsSQLInsertionStmt(string statement)
        {
            return statement.ToUpper().StartsWith("INSERT INTO") || statement.ToUpper().StartsWith("UPDATE");
        }

        /// <summary>
        /// Checks whether the given string statement is an SQL retrieve statement
        /// Examples: SELECT
        /// </summary>
        public static bool IsSQLRetrieveStmt(string statement)
        {
            return statement.ToUpper().StartsWith("SELECT");
        }

        public static string RetrieveSQLTableName(string statement)
        {
            var result = "";
            if (statement.ToUpper().StartsWith("INSERT INTO"))
            {
                var rex = new Regex(@"(?i)(?<=\bINSERT INTO\s)[\p{L}_-]+");
                result = rex.Match(statement).Value.ToLower();
            }
            else if (statement.ToUpper().StartsWith("UPDATE"))
            {
                var rex = new Regex(@"(?i)(?<=\bUPDATE\s)[\p{L}_-]+");
                result = rex.Match(statement).Value.ToLower();
            }
            else if (statement.ToUpper().StartsWith("SELECT"))
            {
                var rex = new Regex(@"(?i)(?<=\bFROM\s)[\p{L}_-]+");
                result = rex.Match(statement).Value.ToLower();
            }

            return result;
        }
    }
}
