using Cardevil.Card.Common.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
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
        public IReadOnlyList<INewCardState> SortedSelection => model.SortedSelection;

        public int HandCardCount => model.Hand.Count;

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
        
        private bool CanInteract => isInputEnabled && !isBusy;
        
        private IReadOnlyList<INewCardState> Selection => model.Selection;

        public HandBarPresenter(HandBarView view, ValueSelectionPresenter valueSelectionPresenter)
        {
            _view = view;
            _view.SortByNumberClicked += OnSortByNumberRequested;
            _view.SortByIconClicked += OnSortByIconRequested;
            
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

        public void AddCard(INewCardState state)
        {
            model.Add(state);
            _view.CreateCard(state);
            _view.ArrangeCards(model.Hand);
        }

        public void RemoveCard(INewCardState state)
        {
            model.Remove(state);
            _view.DestroyCard(state);
            _view.ArrangeCards(model.Hand);
        }

        public async UniTask<INewCardState> DiscardAsync(INewCardState state)
        {
            using (Busy())
            {
                model.Remove(state);
                _view.ArrangeCards(model.Hand);
            
                await _view.PlayDiscardAnimationAsync(state);
            
                return state;
            }
        }

        public async UniTask<IReadOnlyList<INewCardState>> RerollAllCardToDeck()
        {
            using (Busy())
            {
                var hand = model.Hand.ToList();
                var toWait = new List<UniTask>(model.Hand.Count);

                foreach (var state in hand)
                {
                    model.Remove(state);
                    _view.ArrangeCards(model.Hand);
                    toWait.Add(_view.PlayRerollAnimationAsync(state));
                }
            
                await UniTask.WhenAll(toWait);
                return hand;
            }
        }

        public async UniTask DrawAsync(IReadOnlyList<INewCardState> states)
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

        public async UniTask<IReadOnlyList<INewCardState>> DiscardSelectionAsync()
        {
            using (Busy())
            {
                var selection = SortedSelection.ToList();
                var toWait = new List<UniTask>(selection.Count);

                foreach (var state in selection)
                {
                    model.Remove(state);
                    _view.ArrangeCards(model.Hand);
                    toWait.Add(_view.PlayDiscardAnimationAsync(state));
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

        private void OnPointerEnter(INewCardState state)
        {
            if (!CanInteract) return;
        }

        private void OnPointerDown(INewCardState state)
        {
            if (!CanInteract) return;
            
            model.SetPointerDownData(state);
        }

        private void OnDragStart(INewCardState state)
        {
            if (!CanInteract) return;
            
            model.SetDraggingData(state);
            _view.StartDrag(state);
            
            _valueSelectionPresenter.TryOpenValueSelectionZone(state);
            _valueSelectionPresenter.CloseValueSelection();
            
            StartDragLoop(state);
        }

        private void OnDragging(INewCardState state)
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

        private void OnPointerUp(INewCardState state)
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

        private void OnDragEnd(INewCardState state)
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

        private void OnPointerExit(INewCardState state)
        {
            if (!CanInteract) return;
        }

        private void TrySwap(INewCardState dragging)
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

        private void OnSortByNumberRequested()
        {
            if (!CanInteract) return;
            
            model.Sort(CardStateComparers.ByNumber);
            _view.ArrangeCards(model.Hand);
        }

        private void OnSortByIconRequested()
        {
            if (!CanInteract) return;
            
            model.Sort(CardStateComparers.ByIcon);
            _view.ArrangeCards(model.Hand);
        }
        private void OnValueSelected(INewCardState state, uint cardId)
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
            bool shouldShow = moveCards.Any() && moveCards.All(s => s.DirectionList.IsFixed);

            List<Direction> directions = null;
            if (shouldShow)
            {
                directions = moveCards
                    .Select(c => c.DirectionList.FixedValue)
                    .ToList();
            }

            var args = SelectedMoveChangedArgs.Get(shouldShow, directions);
            ExecEventBus<SelectedMoveChangedArgs>.InvokeMergedAndDispose(args).Forget();
        }

        private void StartDragLoop(INewCardState state)
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

        // HandBarPresenter는 MonoBehaviour가 아니므로 Update 이벤트를 사용 못함.
        // 드래깅 중 위치 감지를 위해 내부적으로 구현한 Update.
        private async UniTaskVoid DragLoopAsync(INewCardState state, CancellationToken cancellationToken)
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