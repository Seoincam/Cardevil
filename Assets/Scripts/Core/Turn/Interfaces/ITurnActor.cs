using Cysharp.Threading.Tasks;

namespace Cardevil.Core.Turn.Interfaces
{
    public interface ITurnActor
    {
        UniTask<AttackResult> TurnAttackAsync(IReadOnlyTurnContext ctx);
    }
}