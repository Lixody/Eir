using PHPAnalysis.Data;

namespace PHPAnalysis.Analysis
{
    public interface ITaintProvider
    {
        ImmutableVariableStorage GetTaint();
    }
}