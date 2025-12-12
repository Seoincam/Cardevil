using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data.Save
{
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