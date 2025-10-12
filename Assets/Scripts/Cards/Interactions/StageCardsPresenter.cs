using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Cardevil.Systems;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Utils;
using System.Linq;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.Interactions
{
    public class StageCardsPresenter : ITurnPlayerInput, IClearable
    {
        public event Action<HandRanking> OnSelectsChanged;
        
        private StageCardsModel _model;
        private StageCardsView _view;
        private CardVisualSettingSO _visualSetting;
        
        private StageCardsPresenterState _state;
        private bool _canInteract;
        private bool _isSwapping;
        
        public Card DraggedCard => _state.draggedCard;
        public bool CanInput => !_isSwapping && CanInteraction;

        private bool IsSwapping
        {
            get => _isSwapping;
            set
            {
                _isSwapping = value;
                UpdateUI();
            }
        }

        private bool CanInteraction
        {
            get => _canInteract;
            set
            {
                _canInteract = value;
                UpdateUI();
            }
        }
        
        public void Init(StageCardsModel model)
        {
            if (model == null)
            {
                LogEx.LogError("Init() 실패 — model이 null입니다.");
                return;
            }
            _model = model;

            // View 찾기
            var views = Object.FindObjectsByType<StageCardsView>(FindObjectsSortMode.None);
            if (views == null || views.Length == 0)
            {
                LogEx.LogError("StageCardsView를 찾을 수 없음. 씬에 View가 배치되어 있는지 확인하세요.");
                return;
            }
            _view = views[0];
            _view.BindButtonEvents(Use, Discard, SortByNumber, SortByIcon);

            // SO 로드
            string path = "ScriptableObjects/Cards/CardVisualSetting";
            _visualSetting = Resources.Load<CardVisualSettingSO>(path);
            if (_visualSetting == null)
            {
                LogEx.LogError($"CardVisualSettingSO 로드 실패. 경로가 올바른지 확인하세요: {path}");
                return;
            }
        }
        
        public void SetUp(int maxHandCount)
        {
            _view.ConfigureSlots(maxHandCount);
        }

        public void Clear()
        {
            CanInteraction = false;
            _view.Clear();
            UpdateUI();
        }
        
        private void Update()
        {
            if (!CanInput)
                return;
            if (!_state.draggedCard)
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
        private void OnSelectValueEnd(Card _)
        {
            UpdateUI();
        }
        
        #region Card Events
        
        private void OnCardPointerDown(Card card, CardPointerArgs args)
        {
            _state.activateCard = card;
            _state.pointerDownTime = args.time;
        }

        private void OnCardPointerUp(Card card, CardPointerArgs args)
        {
            if (_state.activateCard != card) return;
            if (args.time - _state.pointerDownTime > _visualSetting.ClickDetectThreshold) return;
            _state.activateCard = null;

            if (_model.Selection.Contains(card))
            {
                card.SetSelect(false);
                _model.Deselect(card);
                UpdateUI();
                OnSelectsChanged?.Invoke(_model.GetHandRanking());
                return;
            }

            if (_model.Selection.Count >= 4) return;
            
            card.SetSelect(true);
            _model.Select(card);
            OnSelectsChanged?.Invoke(_model.GetHandRanking());
            UpdateUI();
        }
        
        private void BeginDrag()
        {
            _state.draggedCard = _state.activateCard;
            _state.activateCard = null;
            UpdateUI();
        }

        private void EndDrag()
        {
            _state.draggedCard = null;
            UpdateUI();
        }

        #endregion
        
        #region Swap

        private void DetectSwap()
        {
            var dragged = _state.draggedCard;
            if (!dragged || _model == null) return;

            if (!dragged.IsDragging) return;

            var draggedX = dragged.transform.position.x;
            if (!_model.TryGetIndex(dragged, out var draggedIdx)) return;

            for (int i = 0; i < _model.Hand.Count; i++)
            {
                var other = _model.GetHandCard(i);
                if (!other || other == dragged) continue;

                var otherX = other.transform.position.x;

                if (!_model.TryGetIndex(other, out var otherIdx)) continue;

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

            _model.Swap(_state.draggedCard, index);
            UpdateSlots();

            IsSwapping = false;
        }

        #endregion

        #region Slot

        private void UpdateSlots()
        {
            foreach (var card in _model.Hand)
            {
                if (!_model.TryGetIndex(card, out var index)) continue;
                _view.SetCardToSlot(card, index);
            }          
        }

        public void MoveToHandBar(int index)
        {
            var card = _model.GetHandCard(index);
            _view.SetCardToSlot(card, index);
            card.CompleteReroll(this);
            AddListeners(card);
        }

        #endregion

        #region Use/Draw/Discard

        private void Use()
        {
            CanInteraction = false;
            CardResultEvaluator.PreEvaluate(_model.SortedSelection);
            _ = UseAsync();
        }

        private void Discard()
        {
            // TODO: 버리기 횟수 0되면 못 버리게
            _model.TryReduceDiscardRemainCount();
            _ = DiscardAndDrawAsync();
        }

        private async UniTask UseAsync()
        {
            await Managers.Card.EvaluationEvent.InvokeAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            await DiscardAsync();

            cmp.TrySetResult();
        }

        private async UniTask DiscardAsync()
        {
            foreach (var card in _model.SortedSelection)
            {
                RemoveListeners(card);
                _model.Discard(card);
                card.Discard();
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardInterval));
                UpdateSlots();
            }
        }

        private async UniTask DrawAsync()
        {
            IsSwapping = true;
            var count = Managers.Card.MaxHandCount - _model.Hand.Count;
            for (int i = 0; i < count; i++)
            {
                var card = Spawn();
                card.Drawn?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DrawInterval));
            }

            IsSwapping = false;
        }

        private async UniTask DiscardAndDrawAsync()
        {
            IsSwapping = true;
            await DiscardAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardDrawInterval));
            await DrawAsync();
            IsSwapping = false;
        }

        #endregion
        
        // Spawn
        // - - - - - - - - - - -
        private Card Spawn()
        {
            var cardData = _model.PopCard();
            if (cardData == null)
                return null;

            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.SpawnInHand(handBar: this, cardData);

            // 이벤트 구독
            AddListeners(card);

            _model.Draw(card);
            UpdateUI();
            UpdateSlots();

            return card;
        }
        
        // Sort
        // - - - - - - - - - - -
        private void SortByNumber()
        {
            _model.SortByNumber();
            UpdateUI();
            UpdateSlots();
        }

        private void SortByIcon()
        {
            _model.SortByIcon();
            UpdateUI();
            UpdateSlots();
        }
        
        // UI
        // - - - - - - - - - - -
        private void UpdateUI()
        {
            var canUse = CanInput && _model.CanUseCard && !_state.draggedCard;
            var canDiscard = CanInput && _model.Selection.Count > 0 && !_state.draggedCard;
            var viewState = new StageCardsViewState(canUse, canDiscard, true, _model.Deck.Count, _model.DiscardRemainCount);
            _view.UpdateUI(viewState);
        }
        
        public async UniTask Revive(int amount)
        {
            amount = Mathf.Min(amount, _model.DiscardPile.Count);
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
        private struct StageCardsPresenterState
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
            Managers.Card.ResultCtx.StepToNext();
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
