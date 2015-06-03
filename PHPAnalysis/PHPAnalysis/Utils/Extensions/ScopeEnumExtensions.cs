namespace PHPAnalysis.Data
{
    public static class ScopeEnumExtensions
    {
        public static VariableScope ToVariableScope(this AnalysisScope analysisScope)
        {
            return analysisScope == AnalysisScope.File ? VariableScope.File : VariableScope.Function;
        }
    }
}