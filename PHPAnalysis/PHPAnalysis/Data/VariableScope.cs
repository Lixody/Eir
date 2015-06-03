namespace PHPAnalysis.Data
{
    public enum VariableScope
    {
        Unknown = 0,
        SuperGlobal = 1,
        File = 2,
        Function = 3,
        Class = 4,
        Instance = 5
    }
}