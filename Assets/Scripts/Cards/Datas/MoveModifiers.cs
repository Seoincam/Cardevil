using Cardevil.Utils.Directions;
using System.Linq;

namespace Cardevil.Cards.Data
{
    /// <summary>
    /// 이동 카드에 적용 가능한 Modifier(강화/효과)의 종류를 정의.
    /// </summary>
    public enum MoveModifierType
    {
        /// <summary>선택 가능한 방향 슬롯을 추가.</summary>
        Selectable,
        
        /// <summary>선택 가능한 방향 슬롯의 값을 확정.</summary>
        SelectableConfirm,
    }

    /// <summary>
    /// 이동 카드의 Modifier(강화 효과)를 정의하는 인터페이스.  
    /// 각 Modifier는 <see cref="MoveBuildContext"/>를 참조하여 카드의 방향 데이터를 수정함.
    /// </summary>
    public interface IMoveModifier
    {
        /// <summary>Modifier의 유형.</summary>
        MoveModifierType Type { get; }

        /// <summary>
        /// 주어진 빌드 컨텍스트를 수정하여 이동 카드의 데이터를 변경.
        /// </summary>
        /// <param name="ctx">변경 대상 <see cref="MoveBuildContext"/> (ref로 전달)</param>
        void Apply(ref MoveBuildContext ctx);
    }

    /// <summary>
    /// 이동 카드에 선택 가능한 방향을 하나 추가하는 Modifier.
    /// </summary>
    public sealed class SelectableDirectionModifier : IMoveModifier
    {
        /// <inheritdoc/>
        public MoveModifierType Type => MoveModifierType.Selectable;

        /// <inheritdoc/>
        public void Apply(ref MoveBuildContext ctx)
        {
            ctx.Selectables.Add(null);
        }
    }

    /// <summary>
    /// 선택 가능한 방향 슬롯에 실제 방향 값을 확정하는 Modifier.  
    /// - 슬롯이 1개인 경우: 지정된 방향(또는 랜덤)으로 확정  
    /// - 슬롯이 2개인 경우: 첫 번째 값이 정해져 있으면 두 번째를 반대 방향으로 자동 확정  
    /// - 슬롯이 4개인 경우: 남은 방향을 중복되지 않게 랜덤으로 채움
    /// </summary>
    public sealed class SelectableDirectionConfirmModifier : IMoveModifier
    {
        /// <inheritdoc/>
        public MoveModifierType Type => MoveModifierType.SelectableConfirm;

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
        public void Apply(ref MoveBuildContext ctx)
        {
            // 명시적으로 지정된 방향이 있는 경우
            if (_value.HasValue)
            {
                for (int i = 0; i < ctx.Selectables.Count; i++)
                {
                    if (ctx.Selectables[i].HasValue) continue;
                    ctx.Selectables[i] = _value.Value;
                    return;
                }
                return;
            }

            // 두 개의 슬롯이 있고, 첫 번째가 확정된 경우: 반대 방향으로 자동 확정
            if (ctx.Selectables.Count == 2 && ctx.Selectables[0].HasValue && !ctx.Selectables[1].HasValue)
            {
                ctx.Selectables[1] = ctx.Selectables[0]!.Value.Opposite();
                return;
            }

            // 나머지 경우: 중복되지 않는 방향 중 하나를 랜덤으로 선택
            var used = ctx.Selectables.Where(d => d.HasValue).Select(d => d!.Value).ToHashSet();
            var all = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            var candidates = all.Where(d => !used.Contains(d)).ToList();

            if (candidates.Count == 0) return; // 모두 확정됨

            var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            for (int i = 0; i < ctx.Selectables.Count; i++)
            {
                if (!ctx.Selectables[i].HasValue)
                {
                    ctx.Selectables[i] = pick;
                    break;
                }
            }
        }
    }
}