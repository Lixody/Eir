using System;

namespace PHPAnalysis
{
    [Flags]
    public enum MixedStatus
    {
        XSSSQL_UNSAFE = 0,
        XSS_SAFE_ONLY = 1,
        SQL_SAFE_ONLY = 2,
        XSSSQL_SAFE = XSS_SAFE_ONLY | SQL_SAFE_ONLY
    }
}

