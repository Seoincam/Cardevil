namespace Cardevil.Core
{
    public interface IFactory<T>
    {
        public T Create();   
    }
}