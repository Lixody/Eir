using System.Collections.Generic;
using PHPAnalysis.Data.PHP;
using YamlDotNet.Serialization.ObjectFactories;

namespace PHPAnalysis.Data
{
    public sealed class Project
    {
        public List<File> Files { get; private set; }

        public List<Class> Classes { get; private set; }

        public List<Function> Functions { get; private set; }

        public List<Interface> Interfaces { get; private set; }
        
        public KeyValuePair<string, string> Constants { get; private set; }

        public Project()
        {
            Files = new List<File>();
            Classes = new List<Class>();
            Functions = new List<Function>();
            Interfaces = new List<Interface>();            
        }
    }
}