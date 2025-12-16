using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data.Save
{
    /// <summary>
    /// 카드 파이프라인 수정자 팩토리.
    /// 세이브 데이터 기반 수정자 인스턴스 생성.
    /// </summary>
    public static class ModifierFactory
    {
        public static IModifier Create(CardModifierSaveData saveData)
        {
            IModifier mod = saveData.type switch
            {
                ModifierType.AttackColor => new ColorModifier(CardColor.None),
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