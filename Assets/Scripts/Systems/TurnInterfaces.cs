using System.Threading.Tasks;

namespace Cardevil.Systems
{
    public interface IPlayerInputHandler
    {
        Task HandlePlayerInputAsync();
        void SubscribePlayerInput();
    }

    public interface IPlayerActionHandler
    {
        Task HandlePlayerActionAsync();
        void SubscribePlayerAction();
    }

    public interface IBossActionHandler
    {
        Task HandleBossActionAsync();
        void SubscribeBossAction();
    }

}