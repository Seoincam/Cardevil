using Cardevil.Cards.Data.InStage;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 숫자(Number) 카드의 Modifier들을 관리,
    /// 이를 순차적으로 적용하여 최종 숫자 데이터를 생성하는 파이프라인 클래스.
    /// </summary>
    public class NumberModifierPipeline
    {
        private readonly List<INumberModifier> _mods = new();
        private readonly List<NumberEnhancementData> _possibleEnhancements = new();
        private NumberEnhancementData? _currentEnhancement = null;
        
        public IReadOnlyList<INumberModifier> Modifiers => _mods;
        public IReadOnlyList<NumberEnhancementData> PossibleEnhancements => _possibleEnhancements;
        public NumberEnhancementData? CurrentEnhancement => _currentEnhancement;

        /// <summary>
        /// 숫자 Modifier를 파이프라인에 추가.
        /// </summary>
        public void AddModifier(INumberModifier mod)
        {
            _mods.Add(mod);
        }

        public void SetCurrentEnhancement(NumberEnhancementData enhancement)
        {
            _currentEnhancement = enhancement;
        }

        public void SetPossibleEnhancements(params NumberEnhancementData[] enhancements)
        {
            _possibleEnhancements.Clear();
            foreach (var enhancement in enhancements)
                _possibleEnhancements.Add(enhancement);
        }

        /// <summary>
        /// 등록된 숫자 Modifier들을 순차적으로 적용하여 최종 <see cref="BuiltNumberData"/>를 생성.
        /// </summary>
        /// <remarks>
        /// 1. Color Modifier — 마지막에 등록된 색상 Modifier만 적용.<br/>
        /// 2. Selectable Modifier — 선택 가능한 숫자 슬롯을 생성.<br/>
        /// 3. SelectableConfirm Modifier — 슬롯 내 값을 확정.<br/>
        /// 4. Damage Modifier — 누적된 공격력 배수를 적용.
        /// </remarks>
        /// <returns>모든 Modifier 적용이 완료된 숫자 데이터</returns>
        public BuiltNumberData Build()
        {
            var ctx = new NumberBuildContext
            {
                Color = CardColor.None,
                Selectables = new(),
                DamageMultiply = 0f
            };
            
            // Modify Color
            // 가장 마지막의 Color만 적용
            INumberModifier color = null;
            foreach (var mod in _mods)
            {
                if (mod.Type == NumberModifierType.Color)
                    color = mod;
            }
            color?.Apply(ref ctx);
            
            // Modify Selectable
            foreach (var mod in _mods)
            {
                if (mod.Type == NumberModifierType.Selectable)
                    mod.Apply(ref ctx);
            }
            
            // Modify Selectable Confirm
            foreach (var mod in _mods)
            {
                if (mod.Type == NumberModifierType.SelectableConfirm)
                    mod.Apply(ref ctx);
            }
            
            // Modify Damage
            foreach (var mod in _mods)
            {
                if (mod.Type == NumberModifierType.Damage)
                    mod.Apply(ref ctx);
            }

            return new BuiltNumberData(ctx.Color, ctx.Selectables, ctx.DamageMultiply);
        }
    }
    
    /// <summary>
    /// Number Modifier들이 수정할 수 있는 빌드 중간 상태 컨텍스트
    /// </summary>
    public struct NumberBuildContext
    {
        public CardColor Color;
        public List<int?> Selectables;
        public float DamageMultiply;
    }
}