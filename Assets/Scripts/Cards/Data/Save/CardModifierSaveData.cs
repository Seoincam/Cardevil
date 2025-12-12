using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data.Save
{
    [Serializable]
    public struct CardModifierSaveData
    {
        public ModifierType type;
        public string payload;
    }
}