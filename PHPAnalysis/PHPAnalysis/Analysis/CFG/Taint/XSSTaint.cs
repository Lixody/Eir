using System;

namespace PHPAnalysis.Analysis.CFG
{
    [Flags]
    public enum XSSTaint
    {
        None = 0,
        XSS_JS = 1,
        XSS_HTML = 2,
        XSS_SQ = 4,
        XSS_AllQ = 8,
        XSS_ALL = XSS_JS | XSS_HTML | XSS_SQ | XSS_AllQ
    }
}