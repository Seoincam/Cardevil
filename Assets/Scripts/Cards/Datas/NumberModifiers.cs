using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    /// <summary>
    /// 숫자 카드에 적용 가능한 Modifier(강화/효과)의 종류를 정의.
    /// </summary>
    public enum NumberModifierType
    {
        /// <summary>카드 색상 변경</summary>
        Color,
        /// <summary>선택 가능한 값 추가</summary>
        Selectable,
        /// <summary>선택 가능한 값 확정</summary>
        SelectableConfirm,
        /// <summary>데미지 배율 증가</summary>
        Damage,
    }

    /// <summary>
    /// 숫자 카드에 적용되는 Modifier의 기본 인터페이스.  
    /// 각 Modifier는 NumberBuildContext를 수정하는 Apply 메서드를 구현해야 함.
    /// </summary>
    public interface INumberModifier
    {
        /// <summary>Modifier의 유형.</summary>
        NumberModifierType Type { get; }

        /// <summary>
        /// 주어진 빌드 컨텍스트를 수정하여 카드 수치에 변화를 적용.
        /// </summary>
        /// <param name="ctx">변경 대상 NumberBuildContext (ref로 전달됨)</param>
        void Apply(ref NumberBuildContext ctx);
    }

    /// <summary>
    /// 카드의 색상을 변경하는 Modifier.
    /// </summary>
    public sealed class ColorModifier : INumberModifier
    {
        public NumberModifierType Type => NumberModifierType.Color;

        private readonly CardColor _color;

        /// <summary>
        /// 지정된 색상으로 초기화.
        /// </summary>
        /// <param name="color">적용할 카드 색상</param>
        public ColorModifier(CardColor color)
        {
            _color = color;
        }

        public void Apply(ref NumberBuildContext ctx)
        {
            ctx.Color = _color;
        }
    }

    /// <summary>
    /// 카드에 선택 가능한 미정 값을 추가하는 Modifier.  
    /// </summary>
    public sealed class SelectableNumberModifier : INumberModifier
    {
        public NumberModifierType Type => NumberModifierType.Selectable;

        public void Apply(ref NumberBuildContext ctx)
        {
            ctx.Selectables.Add(null);
        }
    }

    /// <summary>
    /// 선택 가능한 값 중 null 상태의 value를 실제 숫자로 확정하는 Modifier.  
    /// 값이 지정되지 않은 경우 2~10 중 중복되지 않는 임의의 값을 선택.
    /// </summary>
    public sealed class SelectableNumberConfirmModifier : INumberModifier
    {
        /// <inheritdoc/>
        public NumberModifierType Type => NumberModifierType.SelectableConfirm;

        private int? _value;

        /// <summary>
        /// 선택할 값을 지정하거나, null로 두면 랜덤으로 결정됨.
        /// </summary>
        /// <param name="value">확정할 값 (null 시 자동 결정)</param>
        public SelectableNumberConfirmModifier(int? value = null)
        {
            _value = value;
        }

        public void Apply(ref NumberBuildContext ctx)
        {
            // 값이 지정되지 않은 경우, 가능한 숫자 중 하나를 무작위로 선택
            if (!_value.HasValue)
            {
                var availableNumbers = Enumerable.Range(2, 9).ToList();

                // 이미 선택된 숫자 제거
                foreach (var v in ctx.Selectables)
                {
                    if (v.HasValue)
                        availableNumbers.Remove(v.Value);
                }

                int randomIndex = Random.Range(0, availableNumbers.Count);
                _value = availableNumbers[randomIndex];
            }

            // 첫 번째 null 슬롯에 선택값을 채움
            for (int i = 0; i < ctx.Selectables.Count; i++)
            {
                if (ctx.Selectables[i].HasValue) continue;
                ctx.Selectables[i] = _value;
                break;
            }
        }
    }
    
    /// <summary>
    /// 카드의 데미지 배율을 증가시키는 Modifier.
    /// </summary>
    public sealed class DamageModifier : INumberModifier
    {
        public NumberModifierType Type => NumberModifierType.Damage;

        private readonly float _damage;

        /// <summary>
        /// 증가시킬 데미지 배율을 지정하여 초기화.
        /// </summary>
        /// <param name="damage">추가할 데미지 배율</param>
        public DamageModifier(float damage)
        {
            _damage = damage;
        }

        public void Apply(ref NumberBuildContext ctx)
        {
            ctx.DamageMultiply += _damage;
        }
    }
}