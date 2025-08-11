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
        UniTask HandleBossActionAsync();
    }
    #endregion
}