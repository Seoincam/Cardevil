using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Save
{
    [Serializable]
    public class CardLibrarySaveData
    {
        public List<CardDataPipelineSaveData> pipelines;
    }
}