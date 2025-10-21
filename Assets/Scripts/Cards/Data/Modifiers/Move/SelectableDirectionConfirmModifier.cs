using Cardevil.Utils.Directions;
using System.Linq;

namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 선택 가능한 방향 슬롯에 실제 방향 값을 확정하는 Modifier.  
    /// - 슬롯이 1개인 경우: 지정된 방향(또는 랜덤)으로 확정  
    /// - 슬롯이 2개인 경우: 첫 번째 값이 정해져 있으면 두 번째를 반대 방향으로 자동 확정  
    /// - 슬롯이 4개인 경우: 남은 방향을 중복되지 않게 랜덤으로 채움
    /// </summary>
    public sealed class SelectableDirectionConfirmModifier : IModifier
    {
        /// <inheritdoc/>
        public ModifierType Type => ModifierType.MoveDirSelectable;

        private Direction? _value;

        /// <summary>
        /// 선택 방향을 지정하거나 null로 두면 자동으로 결정됨.
        /// </summary>
        /// <param name="direction">확정할 방향 (null 시 자동 결정)</param>
        public SelectableDirectionConfirmModifier(Direction? direction = null)
        {
            _value = direction;
        }

        /// <inheritdoc/>
        public void Apply(BuildCardContext ctx)
        {
            // 명시적으로 지정된 방향이 있는 경우
        //     if (_value.HasValue)
        //     {
        //         for (int i = 0; i < ctx.Selectables.Count; i++)
        //         {
        //             if (ctx.Selectables[i].HasValue) continue;
        //             ctx.Selectables[i] = _value.Value;
        //             return;
        //         }
        //         return;
        //     }
        //
        //     // 두 개의 슬롯이 있고, 첫 번째가 확정된 경우: 반대 방향으로 자동 확정
        //     if (ctx.Selectables.Count == 2 && ctx.Selectables[0].HasValue && !ctx.Selectables[1].HasValue)
        //     {
        //         ctx.Selectables[1] = ctx.Selectables[0]!.Value.Opposite();
        //         return;
        //     }
        //
        //     // 나머지 경우: 중복되지 않는 방향 중 하나를 랜덤으로 선택
        //     var used = ctx.Selectables.Where(d => d.HasValue).Select(d => d!.Value).ToHashSet();
        //     var all = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        //     var candidates = all.Where(d => !used.Contains(d)).ToList();
        //
        //     if (candidates.Count == 0) return; // 모두 확정됨
        //
        //     var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        //     for (int i = 0; i < ctx.Selectables.Count; i++)
        //     {
        //         if (!ctx.Selectables[i].HasValue)
        //         {
        //             ctx.Selectables[i] = pick;
        //             break;
        //         }
        //     }
        }
    }
}