using Cardevil.Cards.Core;
using System;

namespace Cardevil.Cards.Persistence
{
    /// <summary>
    /// 카드 스펙 수정자 팩토리.
    /// 세이브 데이터 기반 수정자 인스턴스 생성.
    /// </summary>
    public static class ModifierFactory
    {
        public static IModifier Create(CardModifierSaveData saveData)
        {
            IModifier mod = saveData.type switch
            {
                ModifierType.AttackColorSelectable => new ColorModifier(CardColor.None),
                ModifierType.AttackDamage => new DamageModifier(),
                ModifierType.AttackNumSelectable => new SelectableNumberModifier(),
                ModifierType.AttackNumSelectableConfirm => new SelectableNumberConfirmModifier(),
                ModifierType.MoveDirSelectable => new DirSelectableModifier(),
                _ => throw new ArgumentOutOfRangeException()
            };
            mod.Deserialize(saveData);
            
            return mod;
        }
    }
}