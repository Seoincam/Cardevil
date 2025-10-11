using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Systems;
using TMPro;
using System.Collections.Generic;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Attributes;

namespace Cardevil.Cards.Interactions
{
    public class CardHandBar : MonoBehaviour, ITurnPlayerInput, IClearable
    {
        [Header("SO")]
        [SerializeField] CardVisualSettingSO visualSetting;
        
        [Header("States")]
        [SerializeField, VisibleOnly] private HandBarState state;
        [SerializeField, VisibleOnly] private bool canInteraction = false;
        [SerializeField, VisibleOnly] private bool isSwapping = false;

        private CardManager _manager;
        private StageCardsContext _ctx;
        private HandBarView _view;
        
        public Card DraggedCard => state.draggedCard;
        public bool CanInput => !isSwapping && CanInteraction;

        private bool IsSwapping
        {
            get => isSwapping;
            set
            {
                isSwapping = value;
                UpdateUI();
            }
        }

        private bool CanInteraction
        {
            get => canInteraction;
            set
            {
                canInteraction = value;
                UpdateUI();
            }
        }

        public void Init(CardManager manager, StageCardsContext ctx)
        {
            _manager = manager;
            _ctx = ctx;
            _view = FindObjectsByType<HandBarView>(FindObjectsSortMode.None)[0];
            _view.BindButtonEvents(Use, Discard, SortByNumber, SortByIcon);
        }

        public void Clear()
        {
            CanInteraction = false;
            _view.Clear();
            _view.ConfigureSlots(Managers.Card.MaxHandCount);
            UpdateUI();
        }

        private void Update()
        {
            if (!CanInput)
                return;
            if (!state.draggedCard)
                return;
            DetectSwap();
        }
        
        private void AddListeners(Card card)
        {
            card.PointerDown += OnCardPointerDown;
            card.PointerUp += OnCardPointerUp;
            card.DragStarted += BeginDrag;
            card.DragEnded += EndDrag;
            card.ValueSelectionEnded += OnSelectValueEnd;
        }
        
        private void RemoveListeners(Card card)
        {
            card.PointerDown -= OnCardPointerDown;
            card.PointerUp -= OnCardPointerUp;
            card.DragStarted -= BeginDrag;
            card.DragEnded -= EndDrag;
            card.ValueSelectionEnded -= OnSelectValueEnd;
        }
        
        // 플레이어 턴이면서 카드 값 선택이 바뀔 때.
        // Card.onselectEnded에서 호출
        public void OnSelectValueEnd(Card _)
        {
            UpdateUI();
        }
        
        #region Card Events
        
        private void OnCardPointerDown(Card card, CardPointerArgs args)
        {
            state.activateCard = card;
            state.pointerDownTime = args.time;
        }

        private void OnCardPointerUp(Card card, CardPointerArgs args)
        {
            if (state.activateCard != card) return;
            if (args.time - state.pointerDownTime > visualSetting.ClickDetectThreshold) return;
            state.activateCard = null;

            if (_ctx.Selects.Contains(card))
            {
                card.SetSelect(false);
                _ctx.Deselect(card);
                UpdateUI();
                return;
            }

            if (_ctx.SelectCount >= 4) return;
            
            card.SetSelect(true);
            _ctx.Select(card);
            UpdateUI();
        }
        
        private void BeginDrag()
        {
            state.draggedCard = state.activateCard;
            state.activateCard = null;
            UpdateUI();
        }

        private void EndDrag()
        {
            state.draggedCard = null;
            UpdateUI();
        }

        #endregion
        
        #region Swap

        private void DetectSwap()
        {
            var dragged = state.draggedCard;
            if (!dragged || _ctx == null) return;

            if (!dragged.IsDragging) return;

            var draggedX = dragged.transform.position.x;
            if (!_ctx.TryGetIndex(dragged, out var draggedIdx)) return;

            for (int i = 0; i < _ctx.HandCount; i++)
            {
                var other = _ctx.GetHandCard(i);
                if (!other || other == dragged) continue;

                var otherX = other.transform.position.x;

                if (!_ctx.TryGetIndex(other, out var otherIdx)) continue;

                if (draggedX > otherX && draggedIdx < otherIdx)
                {
                    Swap(i);
                    break;
                }

                if (draggedX < otherX && draggedIdx > otherIdx)
                {
                    Swap(i);
                    break;
                }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            _manager.StageCardsCtx.Swap(state.draggedCard, index);
            UpdateSlots();

            IsSwapping = false;
        }

        #endregion

        #region Slot

        private void UpdateSlots()
        {
            foreach (var card in _ctx.Hand)
            {
                if (!_ctx.TryGetIndex(card, out var index)) continue;
                _view.SetCardToSlot(card, index);
            }          
        }

        public void MoveToHandBar(int index)
        {
            var card = _ctx.GetHandCard(index);
            _view.SetCardToSlot(card, index);
            card.CompleteReroll(this);
            AddListeners(card);
        }

        #endregion

        #region Use/Draw/Discard

        private void Use()
        {
            CanInteraction = false;
            CardResultEvaluator.PreEvaluate(_ctx.Selects);
            _ = UseAsync();
        }

        private void Discard()
        {
            // TODO: 버리기 횟수 0되면 못 버리게
            _ctx.ReduceDiscardCount();
            _ = DiscardAndDrawAsync();
        }

        private async UniTask UseAsync()
        {
            await _manager.EvaluationEvent.InvokeAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            await DiscardAsync();

            cmp.TrySetResult();
        }

        private async UniTask DiscardAsync()
        {
            foreach (var card in _ctx.SortedSelect)
            {
                RemoveListeners(card);
                _ctx.Discard(card);
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DiscardInterval));
                UpdateSlots();
            }

            _ctx.Selects.Clear();
        }

        private async UniTask DrawAsync()
        {
            IsSwapping = true;
            var count = Managers.Card.MaxHandCount - _ctx.HandCount;
            for (int i = 0; i < count; i++)
            {
                var card = Spawn();
                card.Drawn?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DrawInterval));
            }

            IsSwapping = false;
        }

        private async UniTask DiscardAndDrawAsync()
        {
            IsSwapping = true;
            await DiscardAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DiscardDrawInterval));
            await DrawAsync();
            IsSwapping = false;
        }

        #endregion
        

        // Spawn
        // - - - - - - - - - - -
        private Card Spawn()
        {
            var cardData = _ctx.PopCard();
            if (cardData == null)
                return null;

            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.SpawnInHand(handBar: this, cardData);

            // 이벤트 구독
            AddListeners(card);

            _ctx.Draw(card);
            UpdateUI();
            UpdateSlots();

            return card;
        }


        // Sort
        // - - - - - - - - - - -
        private void SortByNumber()
        {
            _ctx.SortByNumber();
            UpdateUI();
            UpdateSlots();
        }

        private void SortByIcon()
        {
            _ctx.SortByIcon();
            UpdateUI();
            UpdateSlots();
        }


        // UI
        // - - - - - - - - - - -
        private void UpdateUI()
        {
            var canUse = CanInput && _ctx.CanUseCard && !state.draggedCard;
            var canDiscard = CanInput && _ctx.SelectCount > 0 && !state.draggedCard;
            var viewState = new HandBarViewState(canUse, canDiscard, true, _ctx.DeckCount, _ctx.DiscardRemainCount);
            _view.UpdateUI(viewState);
        }


        public async UniTask Revive(int amount)
        {
            amount = Mathf.Min(amount, _ctx.DiscardCount);
            for (int i = 0; i < amount; i++)
            {
                // var dummyCard = Instantiate(dummyCardVisual, parent: deck.Front.transform);
                // dummyCard.transform.SetSiblingIndex(1);
                // var tween = dummyCard.transform.DOLocalMove(new Vector3(0, 0), visualSetting.ReviveInterval)
                //                             .SetEase(Ease.OutCubic);
                // await tween.AsyncWaitForCompletion();
                // Destroy(dummyCard);
                // _ctx.Revive();
                // UpdateUI();
            }
        }

        #region nested

        [Serializable]
        private struct HandBarState
        {
            public Card activateCard;
            public float pointerDownTime;
            [Space] public Card draggedCard;
        }

        #endregion
        
        // Turn Interface UserInput
        // - - - - - - - - - - -
        public bool IsNoCard => false;

        public void ActivateInteraction()
        {
            _manager.ResultCtx.StepToNext();
            cmp = new();
            CanInteraction = true;
        }

        public void InactivateInteraction()
        {
            CanInteraction = false;
            Managers.Card.ResultCtx.Push();
        }

        public async UniTask DrawCard()
        {
            await DrawAsync();
        }

        private UniTaskCompletionSource cmp;

        public async UniTask WaitUserInput()
        {
            await cmp.Task;
        }
    }
}
