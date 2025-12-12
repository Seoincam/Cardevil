using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Save
{
    [Serializable]
    public class CardDataPipelineSaveData
    {
        public int id;
        public CardKind kind;

        public List<CardModifierSaveData> modifiers;

        public Guid currentEnhancementId;
        public List<Guid> nextEnhancementIds;
    }
}