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
        
        [field: Space]
        [field: SerializeField] public InteractionData CurrentInteraction { get; private set; }
        
        public IReadOnlyList<ICardState> Hand => hand;
        
        public void Add(ICardState state) => hand.Add(state);

        public void Remove(ICardState state)
        {
            hand.Remove(state);
            selection.Remove(state);
        }

        /// <summary>
        /// 카드를 들고 HandBar에서 뺄 때 호출함.
        /// 정보를 저장하고 리스트에서 제거함.
        /// </summary>
        public void Detach(ICardState state)
        {
            CurrentInteraction = new InteractionData(state, IndexOf(state), selection.Contains(state));
            Remove(state);
        }

        /// <summary>
        /// HandBar에서 뺀(들고 있는) 카드를 다시 넣을 때 호출함.
        /// 정보를 바탕으로 리스트에 복구함.
        /// </summary>
        public void Reattach(ICardState state)
        {
            Debug.Assert(state.Id == CurrentInteraction.Card.Id);
            
            hand.Add(state);
            if (CurrentInteraction.IsSelected)
                selection.Add(state);
            
            CurrentInteraction = InteractionData.Empty;
        }

        public int IndexOf(ICardState state) => hand.IndexOf(state);

        public void Swap(int indexA, int indexB)
        {
            (hand[indexA], hand[indexB]) = (hand[indexB], hand[indexA]);
        }

        [Serializable]
        public struct InteractionData
        {
            [field: SerializeReference] public ICardState Card { get; private set; }
            [field: SerializeField] public int OriginalIndex { get; private set; }
            [field: SerializeField] public bool IsSelected { get; private set; }
            [field: SerializeField] public float LastInteractionTime { get; private set; }

            public InteractionData(ICardState card, int originalIndex, bool isSelected)
            {
                Card = card;
                OriginalIndex = originalIndex;
                IsSelected = isSelected;
                LastInteractionTime = Time.time;
            }
            
            public bool Exists => Card != null;
            
            public static InteractionData Empty => new(null, -1, false);
        }
    }
}