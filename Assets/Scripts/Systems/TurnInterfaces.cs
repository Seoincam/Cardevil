using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    interface IUserInput
    {
        bool IsNoCard { get; }

        UniTask DrawCard();
        void ActivateInteraction();
        UniTask HandleUserInput();
        void InactivateInteraction();
    }

    interface IPlayerAction
    {
        bool IsDead { get; }
        UniTask Attack();
        void GetDamage();
    }

    interface IPlayerMove
    {
        UniTask Move();
    }

    interface IEnemy
    {
        bool IsDead { get; }
        bool CheckAttack();
        UniTask Attack();
        void GetDamage();
    }
}