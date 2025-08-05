using System;
using System.Collections;
using Cardevil.Cards;
using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    #region Player Input
    public interface IPlayerInputHandler
    {
        void SubscribePlayerInput();
        UniTask HandlePlayerInputAsync();
        void EndGetInput(CardResult _);
    }
    #endregion


    #region  Player
    public interface IPlayerInputReceiver
    {
        void ReceiveInput(CardResult result);
    }

    public interface IPlayerDamageReceiver
    {
        void SubscribeBossDamage();
        void ReceiveBossDamage(int amount);
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
        event Action<int> OnBossDamageDealt;

        void SubscribePlayerDamage();
        void UnsubscribePlayerDamage();
        void ReceivePlayerDamage(int amount);
    }

    public interface IBossActionHandler
    {
        void SubscribeBossAction();
        UniTask HandleBossActionAsync();
    }
    #endregion
}