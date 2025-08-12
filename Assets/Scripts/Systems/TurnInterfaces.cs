using System;
using Cardevil.Cards;
using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    #region Player Input
    public interface IPlayerInputHandler
    {
        event Action<CardResult> OnPlayerInputReceived;

        void SubscribePlayerInput();
        UniTask HandlePlayerInputAsync();
        void OnCardUsed(CardResult _);
    }
    #endregion


    #region  Player
    public interface IPlayerInputReceiver
    {
        void SubscribePlayerInput();
        void ReceiveInput(CardResult result);
    }

    public interface IPlayerDamageReceiver
    {
        // 필드 상 플레이어 위치 기반으로 수정
    }

    public interface IPlayerActionHandler
    {
        event Action<int> OnPlayerDamageDealt;

        void SubscribePlayerAction();
        UniTask HandlePlayerActionAsync();
    }
    #endregion


    #region Boss
    public interface IBossDamageReceiver
    {
        void SubscribePlayerDamage();
        void UnsubscribePlayerDamage();
        void ReceivePlayerDamage(int amount);
    }

    public interface IBossActionHandler
    {
        void SubscribeBossAction();
        void UnsubscribeBossAction();
        UniTask HandleBossActionAsync();
    }
    #endregion
}