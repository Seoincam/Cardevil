using Cardevil.Attributes;
using Cardevil.Cards.Persistence;
using System;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    [Serializable]
    public sealed class SelectableColorModifier : IModifier
    {
        [field: SerializeField, VisibleOnly]
        public ModifierType Type { get; private set; } = ModifierType.AttackColorSelectable;

        public void Apply(CardData.Builder b)
        {
            b.AddColorSelectable(null);
        }
        
        public CardModifierSaveData Serialize()
        {
            return new CardModifierSaveData {type = Type, payload = string.Empty};
        }

        public void Deserialize(CardModifierSaveData data)
        {
            throw new System.NotImplementedException();
        }
    }
}