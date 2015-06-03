using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Data;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using File = PHPAnalysis.Data.File;

namespace PHPAnalysis.Analysis.AST
{
    public interface IIncludeResolver
    {
        bool TryResolveInclude(XmlNode node, out File path);
    }

    public sealed class IncludeResolver : IIncludeResolver
    {
        // TODO - Currently we're just looking at the filename and see if we can resolve it. 
        //        We should at least try to use the path if it is present.
        private readonly List<File> _projectFiles; 
        public IncludeResolver(ICollection<File> projectFiles)
        {
            Preconditions.NotNull(projectFiles, "projectFiles");
            _projectFiles = new List<File>(projectFiles);
        }

        /// <summary>
        /// Matching last string in include expression against all files and select the first match. 
        /// This is incredibly basic and not necessarily correct. The path is ignored and there could be multiple files
        /// with the same name. 
        /// </summary>
        public bool TryResolveInclude(XmlNode node, out File path)
        {
            Preconditions.NotNull(node, "node");
            Preconditions.IsTrue(node.Name == AstConstants.Node + ":" + AstConstants.Nodes.Expr_Include, "Given node was not an include node. It was " + node.Name, "node");

            string includeString = "";
            
            node.IterateAllNodes(xmlNode =>
                                 {
                                     if (xmlNode.LocalName != AstConstants.Nodes.Scalar_String)
                                         return false;
                                     includeString = xmlNode.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Value).InnerText;
                                     return true; 
                                 });

            string fileName = Path.GetFileName(includeString);

            return TryGetFile(fileName, out path);
        }

        private bool TryGetFile(string fileName, out File file)
        {
            var matchingFiles = _projectFiles.Where(projectFile => projectFile.Name == fileName).ToList();

            if (!matchingFiles.Any() || matchingFiles.Count > 1)
            {
                file = null;
                return false;
            }
            file = matchingFiles.Single();
            return true;
        }
    }
}
