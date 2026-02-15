using Cardevil.NewCard.Common.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCoreModel
    {
        [SerializeReference] private List<ICardState> deck = new();
        [SerializeReference] private List<ICardState> discarded = new();
    }
}