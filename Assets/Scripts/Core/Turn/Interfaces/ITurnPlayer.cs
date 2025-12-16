using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Core.Turn.Interfaces
{
    /// <summary>
    /// 플레이어의 행동을 정의하는 인터페이스.
    /// <see cref="TurnManager"/>가 제어.
    /// </summary>
    public interface ITurnPlayer : ITurnActor, ITurnTarget
    {
        /// <returns>이동 후 타일 기준 위치를 반환.</returns>
        UniTask<Vector2Int> TurnMove(IReadOnlyTurnContext ctx);
    }
}