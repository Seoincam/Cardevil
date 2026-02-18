using Cardevil.NewCard.Common.Core;
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
        [field: SerializeField] public InteractionData PointerDownData { get; private set; }
        [field: SerializeField] public InteractionData DragData { get; private set; }
        [field: SerializeField] public InteractionData DetachData { get; private set; }

        public IReadOnlyList<ICardState> Hand => hand;
        public IReadOnlyList<ICardState> Selection => selection;

        public void Add(ICardState state) => hand.Add(state);
        
        public void Insert(int index, ICardState state) => hand.Insert(index, state);

        public void Remove(ICardState state)
        {
            hand.Remove(state);
            selection.Remove(state);
        }

        public void Select(ICardState state)
        {
            if (!selection.Contains(state)) selection.Add(state);
        }

        public void Deselect(ICardState state)
        {
            selection.Remove(state);
        }

        /// <summary>
        /// 카드를 들고 HandBar에서 뺄 때 호출함.
        /// 정보를 저장하고 리스트에서 제거함.
        /// </summary>
        public void Detach(ICardState state)
        {
            Remove(state);

            DetachData = new InteractionData(state, DragData.OriginalIndex, DragData.IsSelected);
        }

        /// <summary>
        /// HandBar에서 뺀(들고 있는) 카드를 다시 넣을 때 호출함.
        /// 정보를 바탕으로 리스트에 복구함.
        /// </summary>
        public void Reattach(ICardState state)
        {
            Debug.Assert(state.Id == DetachData.Card.Id);

            hand.Insert(DetachData.OriginalIndex, state);
            if (DetachData.IsSelected)
                selection.Add(state);

            ClearDetachData();
        }

        public int IndexOf(ICardState state) => hand.IndexOf(state);

        public void Swap(int indexA, int indexB)
        {
            (hand[indexA], hand[indexB]) = (hand[indexB], hand[indexA]);
        }

        public void Sort(IComparer<ICardState> comparer)
        {
            hand.Sort(comparer.Compare);
        }

        public void SetPointerDownData(ICardState state)
        {
            PointerDownData = new InteractionData(state, IndexOf(state), selection.Contains(state));
        }

        public void ClearPointerDownData()
        {
            PointerDownData = InteractionData.Empty;
        }

        public void SetDraggingData(ICardState state)
        {
            DragData = new InteractionData(state, IndexOf(state), selection.Contains(state));
        }

        public void ClearDraggingData()
        {
            DragData = InteractionData.Empty;
        }

        public void SetDetachData(ICardState state)
        {
            DetachData = new InteractionData(state, IndexOf(state), selection.Contains(state));
        }

        public void ClearDetachData()
        {
            DetachData = InteractionData.Empty;
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