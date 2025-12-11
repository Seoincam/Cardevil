using Cysharp.Threading.Tasks;

namespace Cardevil.Manager
{
    public interface IManager
    {
        UniTask InitializeAsync();
    }
}