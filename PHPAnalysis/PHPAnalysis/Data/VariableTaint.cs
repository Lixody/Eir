using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Analysis.CFG;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Data
{
    public abstract class VariableTaint : IMergeable<VariableTaint>
    {
        public abstract VariableTaint NoTaint { get; }

        public abstract VariableTaint Merge(VariableTaint other);
    }
}
