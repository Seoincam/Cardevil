using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data.Save
{
    /// <summary>
    /// 카드 스펙 수정자 세이브 데이터.
    /// 수정자 타입 및 직렬화 페이로드 포함.
    /// </summary>
    [Serializable]
    public struct CardModifierSaveData
    {
        public ModifierType type;
        public string payload;
    }
}