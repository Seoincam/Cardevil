using Cardevil.Cards.Core;
using System;
using UnityEngine;

namespace Cardevil.Cards.Visual
{
    public interface ISimpleCardVisual
    {
        CanvasGroup CanvasGroup { get; }
        
        void ChangeVisualInstant(CardData cardData);
    }
}