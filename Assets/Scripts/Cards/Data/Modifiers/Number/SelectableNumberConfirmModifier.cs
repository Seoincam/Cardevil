using System.Linq;

namespace Cardevil.Cards.Data.Modifiers.Number 
{
    /// <summary>
    /// 선택 가능한 값 중 null 상태의 value를 실제 숫자로 확정하는 Modifier.  
    /// 값이 지정되지 않은 경우 2~10 중 중복되지 않는 임의의 값을 선택.
    /// </summary>
    public sealed class SelectableNumberConfirmModifier : IModifier
    {
        /// <inheritdoc/>
        public ModifierType Type => ModifierType.AttackNumSelectableConfirm;

        private int? _value;

        /// <summary>
        /// 선택할 값을 지정하거나, null로 두면 랜덤으로 결정됨.
        /// </summary>
        /// <param name="value">확정할 값 (null 시 자동 결정)</param>
        public SelectableNumberConfirmModifier(int? value = null)
        {
            _value = value;
        }

        public void Apply(BuildCardContext ctx)
        {
            // 값이 지정되지 않은 경우, 가능한 숫자 중 하나를 무작위로 선택
            // if (!_value.HasValue)
            // {
            //     var availableNumbers = Enumerable.Range(2, 9).ToList();
            //
            //     // 이미 선택된 숫자 제거
            //     foreach (var v in ctx.Selectables)
            //     {
            //         if (v.HasValue)
            //             availableNumbers.Remove(v.Value);
            //     }
            //
            //     int randomIndex = UnityEngine.Random.Range(0, availableNumbers.Count);
            //     _value = availableNumbers[randomIndex];
            // }
            //
            // // 첫 번째 null 슬롯에 선택값을 채움
            // for (int i = 0; i < ctx.Selectables.Count; i++)
            // {
            //     if (ctx.Selectables[i].HasValue) continue;
            //     ctx.Selectables[i] = _value;
            //     break;
            // }
        }
    }
}