using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCoreModel
    {
        [SerializeReference] private List<IStageCard> deck = new();
        [SerializeReference] private List<IStageCard> discarded = new();
    }
}