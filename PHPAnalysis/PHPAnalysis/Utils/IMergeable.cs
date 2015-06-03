namespace PHPAnalysis.Utils
{
    public interface IMergeable<T>
    {
        T Merge(T other);
    }
}