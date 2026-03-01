using Cysharp.Threading.Tasks;

namespace Cardevil.Core.Turn
{
    public interface ITurnEnemy
    {
        bool IsDead { get; }

        /// <summary>
        /// '턴 시작 시' 관련 적 기믹이 있다면, 적 상태정보 갱신 및 처리.
        /// </summary>
        UniTask OnStartTurnAsync();

        /// <summary>
        /// 적이 플레이어에게 피격되고, 관련된 기믹을 처리.
        /// </summary>
        UniTask OnTakeDamageAsync(float damage);

        /// <summary>
        /// 적 사망 처리 및 모션 진행.
        /// </summary>
        UniTask OnDieAsync();

        /// <summary>
        /// 적 교체 처리 및 모션 진행.
        /// </summary> 
        UniTask OnReplaceAsync();

        /// <summary>
        /// 적의 공격 턴을 1 감소시키고, 공격 턴이 0이 됐나 체크해 반환.
        /// </summary>
        UniTask<bool> CheckAttackCountAsync();

        /// <summary>
        /// 공격을 수행하고, 공격이 성공했는지 여부를 반환.
        /// </summary>
        UniTask<(bool success, int damage)> TryAttackAsync(IEnemyContext context);

        /// <summary>
        /// '적이 공격 성공 시' 관련 적 기믹 처리.
        /// </summary>
        UniTask OnAttackSuccessAsync();

        /// <summary>
        /// '턴 종료 시' 관련 적 기믹이 있다면, 적 상태정보 갱신 및 처리.
        /// </summary>
        UniTask OnEndTurnAsync();

        /// <summary>
        /// 적이 사용할 공격 족보를 처리하고, 적의 공격 범위를 갱신함.
        /// </summary>
        UniTask UpdateAttackAsync(IEnemyContext context);
    }
}