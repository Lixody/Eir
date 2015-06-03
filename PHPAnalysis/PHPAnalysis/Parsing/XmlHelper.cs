using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace PHPAnalysis.Parsing
{
    public static class XmlHelper
    {
        public static bool ContainsIllegalCharacters(string toCheck)
        {
            try
            {
                XmlConvert.VerifyXmlChars(toCheck);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        //The outline for this method has been found on stack overflow: http://stackoverflow.com/questions/8331119/escape-invalid-xml-characters-in-c-sharp
        /// <summary>
        /// Method to replace Illegal XML characters, such that we can actually parse the files and analyse them!
        /// </summary>
        /// <returns>The string that has been stripped for illegal characters</returns>
        /// <param name="inputString">XML string with illegal characters</param>
        public static string ReplaceIllegalXmlCharacters(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return inputString;
            }

            var inputLength = inputString.Length;
            var output = new StringBuilder();
            for (int i = 0; i < inputLength; i++)
            {
                if (XmlConvert.IsXmlChar(inputString[i]))
                {
                    output.Append(inputString[i]);
                }
                else if (i + 1 < inputLength && XmlConvert.IsXmlSurrogatePair(inputString[i + 1], inputString[i]))
                {
                    output.Append(inputString[i]);
                    i++;
                    output.Append(inputString[i]);
                }
                else
                {
                    Debug.WriteLine("Found invalid XML character! Converting to HEX value with 0x prepended! The char was: {0}", inputString[i]);
                    var utf8EndoEncoding = new UTF8Encoding();
                    byte[] encoded = utf8EndoEncoding.GetBytes(inputString[i].ToString());
                    output.Append("0x" + BitConverter.ToString(encoded));
                }
            }
            return output.ToString();
        }
    }
}