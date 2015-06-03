using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data.CFG
{
    public enum EdgeType
    {
        Normal = 0,
        True,
        False
    }

    public class EdgeTag : IDeepCloneable<EdgeTag>
    {
        public EdgeType EdgeType { get; set;}
        public string EdgeData { get; set; }

        public EdgeTag(EdgeType type)
        {
            EdgeType = type;
        }

        public EdgeTag DeepClone()
        {
            return new EdgeTag(EdgeType) {
                                             EdgeData = this.EdgeData
                                         };
        }
    }
}
