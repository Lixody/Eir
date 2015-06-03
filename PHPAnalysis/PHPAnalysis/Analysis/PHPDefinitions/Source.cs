using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PHPAnalysis.Analysis.CFG;

namespace PHPAnalysis
{
    public class Source
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Formats { get; set; }
        public XSSTaintSet XssTaint { get; set; }
        public SQLITaintSet SqliTaint { get; set; }

        public Source(string json) : this(JToken.Parse(json))
        {
            //Nothing! Just enabling both string and JToken version of ctor
        }

        public Source(JToken JSON)
        {
            Name = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Name);
            Type = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Type);
            var xssTaintStr = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.XssTaint);
            var sqlTaintStr = (string)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.SqlTaint);

            //Set up XSS taint from JSON, if it cannot be parsed, then use the default XSS_ALL tag
            XSSTaint tmpXss = XSSTaint.XSS_ALL;
            var success = Enum.TryParse(xssTaintStr, out tmpXss);
            if (success)
            {
                XssTaint = new XSSTaintSet(tmpXss);
            }
            else
            {
                XssTaint = new XSSTaintSet(XSSTaint.XSS_ALL);
            }

            //Set up SQL taint from JSON. If it cannot be parsed then use the default SQL_ALL tag.
            SQLITaint tmpSqli = SQLITaint.SQL_ALL;
            success = Enum.TryParse(sqlTaintStr, out tmpSqli);
            if (success)
            {
                SqliTaint = new SQLITaintSet(tmpSqli);
            }
            else
            {
                SqliTaint = new SQLITaintSet(SQLITaint.SQL_ALL);
            }

            Formats = new List<string>();
            var formats = (JArray)JSON.SelectToken(Keys.PHPDefinitionJSONKeys.GeneralKeys.Formats);
            foreach (string format in formats)
            {
                Formats.Add(format);
            }
        }
    }
}