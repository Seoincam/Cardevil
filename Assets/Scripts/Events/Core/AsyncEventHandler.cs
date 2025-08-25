using Cysharp.Threading.Tasks;

namespace Cardevil.Events
{
    public delegate UniTask AsyncEventHandler();
    public delegate UniTask AsyncEventHandler<T0>(T0 arg0);
    public delegate UniTask AsyncEventHandler<T0, T1>(T0 arg0, T1 arg1);
    public delegate UniTask AsyncEventHandler<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    

}