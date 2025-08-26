using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    /// <summary>
    /// 유저의 Input을 정의하는 정의하는 인터페이스.
    /// TurnManager가 관리.
    /// </summary>
    public interface ITurnPlayerInput
    {
        bool IsNoCard { get; }

        UniTask DrawCard();
        void ActivateInteraction();
        UniTask WaitUserInput();
        void InactivateInteraction();
    }

    /// <summary>
    /// 플레이어의 행동을 정의하는 인터페이스.
    /// TurnManager가 관리.
    /// </summary>
    public interface ITurnPlayerAction
    {
        bool IsDead { get; }
        UniTask Attack();
        void GetDamage(int amount);
    }

    /// <summary>
    /// 플레이어의 움직임을 정의하는 인터페이스.
    /// TurnManager가 관리.
    /// </summary>
    public interface ITurnPlayerMove
    {
        UniTask Move();
    }

    /// <summary>
    /// 적의 행동을 정의하는 인터페이스.
    /// TurnManager가 관리.
    /// </summary>
    public interface ITurnEnemy
    {
        bool IsDead { get; }
        UniTask Attack();
        void GetDamage(int amount);

        /// <summary>
        /// 공격까지 남은 턴이 0이 됐나 확인.
        /// </summary>
        bool CheckAttack();
    }
}