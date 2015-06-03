using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PHPAnalysis.Parsing
{
    public sealed class ParseResult
    {
        private readonly Dictionary<string, XmlDocument> _parsedFiles = new Dictionary<string, XmlDocument>();
        public IDictionary<string, XmlDocument> ParsedFiles { get { return this._parsedFiles; } }

        private readonly List<string> _filesFailedToParse = new List<string>();
        public IList<string> FilesThatFailedToParse { get { return this._filesFailedToParse; } }
    }
}
