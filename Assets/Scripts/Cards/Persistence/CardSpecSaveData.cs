using Cardevil.Cards.Core;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Persistence
{
    /// <summary>
    /// 카드 스펙 세이브 데이터.
    /// 카드 ID, 종류, 수정자 및 강화 상태 포함.
    /// </summary>
    [Serializable]
    public class CardSpecSaveData
    {
        public int id;
        public CardKind kind;

        public List<CardModifierSaveData> modifiers;

        public Guid currentEnhancementId;
        public List<Guid> nextEnhancementIds;
    }
}