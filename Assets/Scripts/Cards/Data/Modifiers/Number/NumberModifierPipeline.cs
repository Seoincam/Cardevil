using Cardevil.Cards.InStageData;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Modifiers.Number
{
    // Modifier!
    public class NumberModifierPipeline
    {
        private readonly List<INumberModifier> _mods = new();

        public void Add(INumberModifier mod)
        {
            _mods.Add(mod);
        }

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
            /*
             * 큰 도움이 될지는 모르겠지만,
             * Selectable Modifier가 9개일 경우
             * 바로 2~10 할당하는 방법도 있을 듯.
             */
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