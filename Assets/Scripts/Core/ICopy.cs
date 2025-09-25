namespace Cardevil.Core
{
    public interface ICopy<T>
    {
        public void CopyFrom(T other);
        public void CopyTo(T other);
    }
}