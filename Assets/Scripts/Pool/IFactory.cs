namespace Cardevil.Pools
{
    public interface IFactory<T> 
    {
        /// <summary>
        /// Creates an instance of type T.
        /// </summary>
        /// <returns>An instance of type T.</returns>
        T Create();

        /// <summary>
        /// The original instance of type T that this factory is based on.
        /// </summary>
        T Original { get; set; }
    }
}