using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Save;
using System;
using UnityEngine;

namespace Cardevil.Cards.Data.Modifiers
{
    /// <summary>
    /// 카드에 선택 가능한 미정 값을 추가하는 Modifier.  
    /// </summary>
    [Serializable]
    public sealed class SelectableNumberModifier : IModifier
    {
        [SerializeField, VisibleOnly] private ModifierType type = ModifierType.AttackNumSelectable;
        
        public ModifierType Type => type;

        public void Apply(CardData.Builder b)
        {
            b.AddNumberSelectable(null);
        }

        public CardModifierSaveData Serialize()
        {
            return new CardModifierSaveData() { type = type, payload = string.Empty };
        }

        public void Deserialize(CardModifierSaveData data)
        {
            
        }
    }
}