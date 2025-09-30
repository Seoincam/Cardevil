namespace Cardevil.Core
{
    public interface IDeepClonable<T>
    {
        T DeepClone();
    }
    
    public interface IShallowClonable<T>
    {
        T ShallowClone();
    }
    
}