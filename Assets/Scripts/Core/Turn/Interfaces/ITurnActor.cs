using Cysharp.Threading.Tasks;

namespace Cardevil.Core.Turn.Interfaces
{
    /// <summary>
    /// 턴 행동 주체 인터페이스.
    /// 턴 중 공격 행동 비동기 수행.
    /// </summary>
    public interface ITurnActor
    {
        UniTask<AttackResult> TurnAttackAsync(IReadOnlyTurnContext ctx);
    }
}