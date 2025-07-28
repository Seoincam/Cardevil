using System.Threading.Tasks;
using Cardevil.Cards;

namespace Cardevil.Systems
{
    public interface IPlayerInputHandler
    {
        Task HandlePlayerInputAsync();
        void SubscribePlayerInput();
    }


    public interface IPlayerInputReceiver
    {
        void RecieveInput(CardResult result);
    }

    public interface IPlayerActionHandler
    {
        Task HandlePlayerActionAsync();
        void SubscribePlayerAction();
    }

    public interface IBossDamageReceiver
    {
        // 플레이어에게 받는 데미지 등
    }

    public interface IBossActionHandler
    {
        Task HandleBossActionAsync();
        void SubscribeBossAction();
    }

}