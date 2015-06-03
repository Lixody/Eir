using System;

namespace PHPAnalysis.Analysis.CFG
{
    [Flags]
    public enum SQLITaint
    {
        None = 0,
        SQL_SQ = 1,
        SQL_DQ = 2,
        SQL_NoQ = 4,
        SQL_ALL = SQL_SQ | SQL_DQ | SQL_NoQ
    }
}