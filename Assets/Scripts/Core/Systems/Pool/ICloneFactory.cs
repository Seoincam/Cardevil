using Cardevil.Core;

namespace Cardevil.Pools
{
    public interface ICloneFactory<T> : IFactory<T> where T : class
    {
        /// <summary>
        /// The original instance of type T that this factory is based on.
        /// </summary>
        T Original { get; set; }
    }
}