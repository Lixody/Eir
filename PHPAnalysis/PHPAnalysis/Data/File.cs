using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Data.PHP;
using PHPAnalysis.Utils;
using QuickGraph;

namespace PHPAnalysis.Data
{
    public sealed class File
    {
        public XmlNode AstNode { get; private set; }
        public string FullPath { get; set; }
        public string Name { get { return Path.GetFileName(FullPath); } }

        public string Extension { get { return Path.GetExtension(FullPath); } }


        public IDictionary<string, List<Function>> Functions { get; set; }
        //public IReadOnlyDictionary<string, Function> Functions
        //{
        //    get { return new ReadOnlyDictionary<string, Function>(_functions); }
        //}

        public IDictionary<string, List<Class>> Classes { get; set; }
        //public IReadOnlyDictionary<string, Class> Classes
        //{
        //    get { return new ReadOnlyDictionary<string, Class>(_classes); }
        //}

        public IDictionary<string, List<Interface>> Interfaces { get; set; }
        public Closure[] Closures { get; set; }
        public IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> CFG { get; set; }

        public File()
        {
            this.Functions = new Dictionary<string, List<Function>>();
            this.Classes = new Dictionary<string, List<Class>>();
            this.Interfaces = new Dictionary<string, List<Interface>>();
        }

        public File(XmlNode node) : this()
        {
            Preconditions.NotNull(node, "node");

            this.AstNode = node;
        }

        public File(IDictionary<string, List<Function>> functions, IDictionary<string, List<Class>> classes)
        {
            Preconditions.NotNull(functions, "functions");
            Preconditions.NotNull(classes, "classes");

            this.Functions = functions;
            this.Classes = classes;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}