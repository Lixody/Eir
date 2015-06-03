namespace PHPAnalysis.Utils
{
    public interface IDeepCloneable<out T>
    {
        T DeepClone();
    }

    public interface IShallowCloneable<out T>
    {
        T ShallowClone();
    }
}