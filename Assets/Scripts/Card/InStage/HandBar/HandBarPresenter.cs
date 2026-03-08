using Cardevil.Attributes;
using Cardevil.Card.Common.Core;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public delegate void HandRankAction(in HandRankData handRank);
    
    public delegate void HandBarStateAction(in HandBarPresenter.SelectionState state);

    [Serializable]
    public class HandBarPresenter
    {
        [Header("States")]
        [SerializeField, VisibleOnly] private HandBarModel model = new();
        
        [SerializeField, VisibleOnly] private bool isInputEnabled; // 외부(Turn)에서 허용했는가?
        [SerializeField, VisibleOnly] private bool isBusy; // 내부(Animation 등)에서 작업 중인가?

        public bool CanInteract => isInputEnabled && !isBusy;

        private HandBarView _view;
        private ValueSelectionPresenter _valueSelectionPresenter;
        
        private HandRankData _cachedHandRankData = HandRankData.None;
        private CancellationTokenSource _dragLoopCancellationTokenSource;

        public event HandRankAction HandRankChanged;
        public event HandBarStateAction HandBarStateChanged;

        /// <summary>
        /// 손패 상에 위치한 순서대로 정렬된 선택 카드.
        /// 호출 시 새로운 객체를 반환함.
        /// </summary>
        public IReadOnlyList<ICardState> SortedSelection => model.SortedSelection;

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
        
        private IReadOnlyList<ICardState> Selection => model.Selection;

        public HandBarPresenter(HandBarView view, ValueSelectionPresenter valueSelectionPresenter)
        {
            _view = view;
            _view.SortByNumberClicked += SortByNumber;
            _view.SortByIconClicked += SortByIcon;
            
            _view.CardPointerEnter += OnPointerEnter;
            _view.CardPointerDown += OnPointerDown;
            _view.CardDragStart += OnDragStart;
            _view.CardPointerUp += OnPointerUp;
            _view.CardDragEnd += OnDragEnd;
            _view.CardPointerExit += OnPointerExit;
            
            _valueSelectionPresenter = valueSelectionPresenter;
            _valueSelectionPresenter.ValueSelected += OnValueSelected;
        }

        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
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
            using (Busy())
            {
                model.Remove(state);
                _view.ArrangeCards(model.Hand);
            
                await _view.MoveCardToDiscardAnchor(state);
            
                return state;
            }
        }

        public async UniTask DrawAsync(IReadOnlyList<ICardState> states)
        {
            using (Busy())
            {
                foreach (var state in states)
                {
                    AddCard(state);
                    await UniTask.Delay(TimeSpan.FromSeconds(.15f));
                }
            }
        }

        public async UniTask<IReadOnlyList<ICardState>> DiscardSelectionAsync()
        {
            using (Busy())
            {
                var selection = SortedSelection.ToList();
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
            
            StartDragLoop(state);
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
                if (state.IsMove) 
                    PublishMoveCardEvent();
                
                PublishEvents();
            }
        }

        private void OnDragEnd(ICardState state)
        {
            StopDragLoop();
            
            _valueSelectionPresenter.CloseValueSelectionZone();
            
            var cardWorldPos = _view.GetWorldPosition(state);
            var isOnValueSelectionZone = _valueSelectionPresenter.IsOnValueSelectionZone(cardWorldPos);

            var cardId = _view.GetCardId(state);
            if (isOnValueSelectionZone && _valueSelectionPresenter.TryOpenValueSelection(state, cardId))
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
            }
            
            model.ClearDraggingData();
            _view.EndDrag(state);
            _view.ArrangeCards(model.Hand);
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

            _view.ArrangeCards(model.Hand, dragging);
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
        private void OnValueSelected(ICardState state, uint cardId)
        {
            model.Reattach(state);
                
            _view.DestroyCard(state);
            _view.ConvertToHandCard(state, cardId);

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

        private void PublishMoveCardEvent()
        {
            var moveCards = model.Selection.Where(s => s.IsMove).ToList();
            bool shouldShow = moveCards.Any() && moveCards.All(s => s.Directions.HasSelected);

            List<Direction> directions = null;
            if (shouldShow)
            {
                directions = moveCards
                    .Select(c => c.Directions.Current!.Value)
                    .ToList();
            }

            var args = SelectedMoveChangedArgs.Get(shouldShow, directions);
            ExecEventBus<SelectedMoveChangedArgs>.InvokeMergedAndDispose(args).Forget();
        }

        private void StartDragLoop(ICardState state)
        {
            StopDragLoop();
            _dragLoopCancellationTokenSource = new CancellationTokenSource();
            DragLoopAsync(state, _dragLoopCancellationTokenSource.Token).Forget();
        }

        private void StopDragLoop()
        {
            _dragLoopCancellationTokenSource?.Cancel();
            _dragLoopCancellationTokenSource?.Dispose();
            _dragLoopCancellationTokenSource = null;
        }

        private async UniTaskVoid DragLoopAsync(ICardState state, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                OnDragging(state);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        private BusyScope Busy() => new(this);
        
        private readonly struct BusyScope : IDisposable
        {
            private readonly HandBarPresenter _presenter;

            public BusyScope(HandBarPresenter presenter)
            {
                _presenter = presenter;
                _presenter.isBusy = true;
            }

            public void Dispose()
            {
                _presenter.isBusy = false;
            }
        }
    }
}