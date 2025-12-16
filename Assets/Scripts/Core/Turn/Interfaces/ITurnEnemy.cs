using Cysharp.Threading.Tasks;

namespace Cardevil.Core.Turn.Interfaces
{
    /// <summary>
    /// 적의 행동을 정의하는 인터페이스.
    /// <see cref="TurnManager"/>가 제어.
    /// </summary>
    public interface ITurnEnemy : ITurnActor, ITurnTarget
    {
        /// <summary>
        /// Enemy가 죽은 후, 다음 Enemy로 교체.
        /// 실제 교체는 <c>EnemySpawner</c>가 수행.
        /// 교체 애니메이션, 기존 Enemy Despawn 등을 수행함.
        /// </summary>
        UniTask Replace();

        /// <summary>
        /// 첫 공격 범위를 표시.
        /// </summary>
        UniTask ShowInitialAttackArea();
    }
}