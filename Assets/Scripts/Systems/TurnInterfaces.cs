using System;
using System.Collections;
using Cardevil.Cards;

namespace Cardevil.Systems
{
    #region Player Input
    public interface IPlayerInputHandler
    {
        IEnumerator HandlePlayerInputCoroutine();
        void EndGetInput(CardResult _);
        void SubscribePlayerInput();
    }
    #endregion


    #region  Player
    public interface IPlayerInputReceiver
    {
        void ReceiveInput(CardResult result);
    }

    public interface IPlayerDamageReceiver
    {
        // IBossActionHandler.OnBossDamageDealt에 ReceiveBossDamage 구독
        void SubscribeBossDamage();
        void ReceiveBossDamage(int amount);
    }

    public interface IPlayerActionHandler
    {
        event Action<int> OnPlayerDamageDealt;

        // TurnManager.PlayerActionCoroutine에 HandlePlayerActionCoroutine 구독
        void SubscribePlayerAction();
        IEnumerator HandlePlayerActionCoroutine();
    }
    #endregion


    #region Boss
    public interface IBossDamageReceiver
    {
        event Action<int> OnBossDamageDealt;

        // IPlayerActionHandler.OnPlayerDamageDealt에 ReceiveBossDamage 구독
        void SubscribePlayerDamage();
        void UnsubscribePlayerDamage();
        void ReceivePlayerDamage(int amount);
    }

    public interface IBossActionHandler
    {
        // TurnManager.BossActionCoroutine에 HandleBossActionCoroutine 구독
        void SubscribeBossAction();
        IEnumerator HandleBossActionCoroutine();
    }
    #endregion
}