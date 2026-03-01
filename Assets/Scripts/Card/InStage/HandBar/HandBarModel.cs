using Cardevil.Card.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.InStage
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
        
        private readonly HashSet<ICardState> handSet = new();
        private readonly HashSet<ICardState> selectionSet = new();

        public IReadOnlyList<ICardState> Hand => hand;
        public IReadOnlyList<ICardState> Selection => selection;

        /// <summary>
        /// 손패 상에 위치한 순서대로 정렬된 선택 목록.
        /// 호출 시 새로운 객체를 반환함.
        /// </summary>
        public IReadOnlyList<ICardState> SortedSelection => selection.OrderBy(s => hand.IndexOf(s)).ToList();

        public void Add(ICardState state)
        {
            if (!handSet.Add(state)) return;
            hand.Add(state);
        }

        public void Insert(int index, ICardState state)
        {
            if (!handSet.Add(state)) return;
            hand.Insert(index, state);
        }

        public void Remove(ICardState state)
        {
            if (!handSet.Remove(state)) return;
            hand.Remove(state);
            if (selectionSet.Remove(state))
                selection.Remove(state);
        }

        public void Select(ICardState state)
        {
            if (selectionSet.Add(state))
                selection.Add(state);
        }

        public void Deselect(ICardState state)
        {
            if (selectionSet.Remove(state))
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

            if (handSet.Add(state))
                hand.Insert(DetachData.OriginalIndex, state);
            if (DetachData.IsSelected && selectionSet.Add(state))
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
            PointerDownData = new InteractionData(state, IndexOf(state), selectionSet.Contains(state));
        }

        public void ClearPointerDownData()
        {
            PointerDownData = InteractionData.Empty;
        }

        public void SetDraggingData(ICardState state)
        {
            DragData = new InteractionData(state, IndexOf(state), selectionSet.Contains(state));
        }

        public void ClearDraggingData()
        {
            DragData = InteractionData.Empty;
        }

        public void SetDetachData(ICardState state)
        {
            DetachData = new InteractionData(state, IndexOf(state), selectionSet.Contains(state));
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