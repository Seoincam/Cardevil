using Cardevil.NewCard.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class HandBarModel
    {
        [SerializeReference] private List<ICardState> hand = new();
        [SerializeReference] private List<ICardState> selection = new();
        
        public IReadOnlyList<ICardState> Hand => hand;
        
        public void Add(ICardState card) => hand.Add(card);
        public void Remove(ICardState card) => hand.Remove(card);

        public void MoveTo(int fromIndex, int targetIndex)
        {
            if (fromIndex == targetIndex) return;
            
            var card = hand[fromIndex];
            hand.RemoveAt(fromIndex);
            hand.Insert(targetIndex, card);
        }
        
        public int IndexOf(ICardState card) => hand.IndexOf(card);

        public void Swap(int indexA, int indexB)
        {
            (hand[indexA], hand[indexB]) = (hand[indexB], hand[indexA]);
        }
    }
}