using Cardevil.Attributes;
using Cardevil.NewCard.Common.Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public delegate void HandRankAction(in HandRankData handRank);
    
    public delegate void HandBarStateAction(in HandBarPresenter.SelectionState state);

    [Serializable]
    public class HandBarPresenter
    {
        [Header("States")]
        [SerializeField, VisibleOnly] private HandBarModel model = new();
        [field: SerializeField, VisibleOnly] public bool CanInteract { private get; set; }
        
        private HandBarView _view;
        private ValueSelectionPresenter _valueSelectionPresenter;
        
        private HandRankData _cachedHandRankData = HandRankData.None;

        public event HandRankAction HandRankChanged;
        public event HandBarStateAction HandBarStateChanged;
        
        /// <summary>
        /// 현재 선택 중인 카드.
        /// </summary>
        public IReadOnlyList<ICardState> Selection => model.Selection;

        /// <summary>
        /// 선택한 카드들을 사용할 수 있는지 여부를 반환.
        /// </summary>
        /// <remarks> 현재 검증 목록: 선택한 카드가 존재하고, 모든 값을 선택했는가. </remarks>
        public bool CanUseSelection => Selection.Count > 0 && Selection.All(state => state.ValueSelected);

        /// <summary>
        /// 선택한 카드들을 버릴 수 있는지 여부를 반환.
        /// </summary>
        public bool CanDiscard => Selection.Count > 0;

        /// <summary>
        /// 현재 족보 평가 데이터.
        /// </summary>
        public HandRankData HandRankData => _cachedHandRankData;

        public HandBarPresenter(HandBarView view, ValueSelectionPresenter valueSelectionPresenter)
        {
            _view = view;
            _view.SortByNumberClicked += SortByNumber;
            _view.SortByIconClicked += SortByIcon;
            
            _view.CardPointerEnter += OnPointerEnter;
            _view.CardPointerDown += OnPointerDown;
            _view.CardDragStart += OnDragStart;
            _view.CardDragging += OnDragging;
            _view.CardPointerUp += OnPointerUp;
            _view.CardDragEnd += OnDragEnd;
            _view.CardPointerExit += OnPointerExit;
            
            _valueSelectionPresenter = valueSelectionPresenter;
            _valueSelectionPresenter.ValueSelected += OnValueSelected;
        }

        public void AddCard(ICardState state)
        {
            model.Add(state);
            _view.CreateCard(state);
            _view.ArrangeCards(model.Hand);
        }

        public void RemoveCard(ICardState state)
        {
            model.Remove(state);
            _view.DestroyCard(state);
            _view.ArrangeCards(model.Hand);
        }

        public async UniTask<ICardState> DiscardAsync(ICardState state)
        {
            model.Remove(state);
            _view.ArrangeCards(model.Hand);
            
            await _view.MoveCardToDiscardAnchor(state);
            
            return state;
        }

        public async UniTask DrawAsync(IReadOnlyList<ICardState> states)
        {
            foreach (var state in states)
            {
                AddCard(state);
            }
        }

        public async UniTask<IReadOnlyList<ICardState>> DiscardSelectionAsync()
        {
            var selection = Selection.ToList();
            var toWait = new List<UniTask>(selection.Count);

            foreach (var state in selection)
            {
                model.Remove(state);
                _view.ArrangeCards(model.Hand);
                toWait.Add(_view.MoveCardToDiscardAnchor(state));
            }
            
            await UniTask.WhenAll(toWait);
            return selection;
        }
        
        public readonly struct SelectionState
        {
            public bool CanUseSelection { get; }
            public bool CanDiscard { get; }

            public SelectionState(bool canUseSelection, bool canDiscard)
            {
                CanUseSelection = canUseSelection;
                CanDiscard = canDiscard;
            }
        }

        private void OnPointerEnter(ICardState state)
        {
            if (!CanInteract) return;
        }

        private void OnPointerDown(ICardState state)
        {
            if (!CanInteract) return;
            
            model.SetPointerDownData(state);
        }

        private void OnDragStart(ICardState state)
        {
            if (!CanInteract) return;
            
            model.SetDraggingData(state);
            _view.StartDrag(state);
            
            _valueSelectionPresenter.TryOpenValueSelectionZone(state);
            _valueSelectionPresenter.CloseValueSelection();
        }

        private void OnDragging(ICardState state)
        {
            if (!CanInteract) return;

            bool inZone = _view.IsInHandZone(state);

            if (model.DetachData.Exists && inZone)
            {
                // 핸드로 복귀
                model.Reattach(state);
            }
            else if (!model.DetachData.Exists && !inZone)
            {
                // 핸드에서 빠짐
                model.Detach(state);
                _view.ArrangeCards(model.Hand);
            }

            if (inZone)
            {
                TrySwap(state);   
            }
        }

        private void OnPointerUp(ICardState state)
        {
            if (!CanInteract) return;
            
            // 드래그를 했다면 시간도 체크함.
            if (model.PointerDownData.Exists && Time.time - model.PointerDownData.LastInteractionTime > 0.2f) return;

            bool changed = false;
            if (model.Selection.Contains(state))
            {
                Debug.Log("Deselect");
                model.Deselect(state);
                _view.DeselectCard(state);
                changed = true;
            }
            else if (model.Selection.Count < 4)
            {
                Debug.Log("Select");
                model.Select(state);
                _view.SelectCard(state);
                changed = true;
            }

            if (changed)
            {
                PublishEvents();
            }
        }

        private void OnDragEnd(ICardState state)
        {
            _valueSelectionPresenter.CloseValueSelectionZone();
            
            var cardWorldPos = _view.GetWorldPosition(state);
            var isOnValueSelectionZone = _valueSelectionPresenter.IsOnValueSelectionZone(cardWorldPos);
            
            if (isOnValueSelectionZone && _valueSelectionPresenter.TryOpenValueSelection(state))
            {
                model.ClearDraggingData();
                _view.StartValueSelection(state, _valueSelectionPresenter.ZoneWorldPosition);
                return;
            }
            
            if (model.DetachData.Exists)
            {
                // 선택 존 아니고, 손패 영역 밖이면 복귀 처리
                model.Insert(model.DragData.OriginalIndex, state);
                model.ClearDetachData();
                
                _view.ArrangeCards(model.Hand);
            }
            
            model.ClearDraggingData();
            _view.EndDrag(state);
        }

        private void OnPointerExit(ICardState state)
        {
            if (!CanInteract) return;
        }

        private void TrySwap(ICardState dragging)
        {
            int from = model.IndexOf(dragging);
            float draggingX = _view.GetCurrentX(dragging);
            int count = model.Hand.Count;

            // 오른쪽으로 밀기
            while (from + 1 < count)
            {
                float nextX = _view.GetCurrentX(model.Hand[from + 1]);
                if (draggingX <= nextX) break;
        
                model.Swap(from, from + 1);
                from++;
            }

            // 왼쪽으로 밀기
            while (from - 1 >= 0)
            {
                float prevX = _view.GetCurrentX(model.Hand[from - 1]);
                if (draggingX >= prevX) break;
        
                model.Swap(from, from - 1);
                from--;
            }

            _view.ArrangeCards(model.Hand);
        }

        private void SortByNumber()
        {
            if (!CanInteract) return;
            
            model.Sort(CardStateComparers.ByNumber);
            _view.ArrangeCards(model.Hand);
        }

        private void SortByIcon()
        {
            if (!CanInteract) return;
            
            model.Sort(CardStateComparers.ByIcon);
            _view.ArrangeCards(model.Hand);
        }
        private void OnValueSelected(ICardState state)
        {
            model.Reattach(state);
                
            _view.UpdateCardVisual(state);
            _view.EndValueSelection(state);
            _view.ArrangeCards(model.Hand);
            
            PublishEvents();
        }

        private void PublishEvents()
        {
            _cachedHandRankData = HandRankEvaluator.GetHandRank(model.Selection);
            HandRankChanged?.Invoke(_cachedHandRankData);
            
            var handBarState = new SelectionState(CanUseSelection, CanDiscard);
            HandBarStateChanged?.Invoke(handBarState);
        }
    }
}