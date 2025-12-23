using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Save
{
    /// <summary>
    /// 카드 상태 세이브 데이터.
    /// 카드 스펙 세이브 목록 포함.
    /// </summary>
    [Serializable]
    public class CardStatusSaveData
    {
        public List<CardSpecSaveData> specList;
    }
}