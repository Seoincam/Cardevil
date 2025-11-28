using Cardevil.Cards.Data;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Cardevil.Systems;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Utils;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.View;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Utils.Directions;
using System.Linq;
using System.Threading;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.InStage.Presenter
{
    public class StageCardsPresenter : ITurnPlayerInput, IClearable
    {
        private IReadOnlyCardLibrary _library;
        private StageCardsModel _model;
        private IEvaluationPresenter _evaluationPresenter;
        
        private StageCardsView _view;
        private DeckRemainView _deckRemainView;
        private CardValueSelectionView _selectionView;
        
        private CardVisualSettingSO _visualSetting;
        
        private StageCardsPresenterState _state;
        private CancellationTokenSource _updateCts = new(); // UpdateAsync에 사용
        
        private event Action HandChanged;
        private event Action DeckChanged;
        
        private bool CanInput => _state is { isSwapping: false, canInteract: true };
        
        /// <summary>
        /// StageCardsPresenter 초기화.  
        /// model 참조를 저장, 카드 시각 효과 설정용 So를 로드.  
        /// 이미 초기화된 경우 중복 실행을 방지.
        /// </summary>
        public void Init(IReadOnlyCardLibrary library, StageCardsModel model, IEvaluationPresenter evaluationPresenter)
        {
            if (_state.isInitialized) return;

            if (library == null)
            {
                LogEx.LogError("library가 null입니다.");
                return;
            }
            _library = library;
            
            if (model == null)
            {
                LogEx.LogError("model이 null입니다.");
                return;
            }
            _model = model;

            if (evaluationPresenter == null)
            {
                LogEx.LogError("evaluation Presenter가 null입니다.");
                return;
            }
            _evaluationPresenter = evaluationPresenter;

            // SO 로드
            string path = "ScriptableObjects/Cards/CardVisualSetting";
            _visualSetting = Resources.Load<CardVisualSettingSO>(path);
            if (!_visualSetting)
            {
                LogEx.LogError($"CardVisualSettingSO 로드 실패. 경로가 올바른지 확인하세요: {path}");
                return;
            }

            _state.isInitialized = true;
        }

        /// <summary>
        /// UI를 초기화, 카드를 슬롯에 배치, 버튼 이벤트를 바인딩.  
        /// 카드의 시각적 상태를 설정, 입력 감지 Update 루프 시작.
        /// </summary>
        /// <returns>UI 초기화 완료 후 완료되는 <see cref="UniTask"/></returns>
        public async UniTask SetUp()
        {
            Transform canvas = GameObject.Find("CardCanvas").transform;
            
            // View 생성
            var views = Object.FindObjectsByType<StageCardsView>(FindObjectsSortMode.None);
            if (views is { Length: > 0 }) _view = views[0];
            else
            {
                GameObject go = Managers.Resource.Instantiate("UI/CardUI/StageCardsView", canvas);
                _view = go.GetComponent<StageCardsView>();
            }
            _view.ConfigureSlots(_model.MaxHand);
            _view.BindButtonEvents(Use, Discard, SortByNumber, SortByIcon);
            
            // 현재 생성된 카드 모두 HandBar 슬롯으로 이동
            foreach (var card in _model.Hand)
            {
                WireCard(card);
                HandChanged += card.OnHandChanged;
                card.SetRerollState(false);
                _model.TryGetIndex(card, out int index);
                _view.SetCardToSlot(card, index);
            }
            HandChanged?.Invoke();
            
            // Deck Remain View 생성 및 DeckVisual에 바인딩
            var deckRemainViews = Object.FindObjectsByType<DeckRemainView>(FindObjectsSortMode.None);
            if (views is {Length: > 0}) _deckRemainView = deckRemainViews[0];
            else
            {
                GameObject go = Managers.Resource.Instantiate("UI/CardUI/DeckRemainView", canvas);
                _deckRemainView = go.GetComponent<DeckRemainView>();
            }
            _deckRemainView.Init(_library, _model);
            DeckChanged += _deckRemainView.OnDeckChanged;

            CardDeckVisual.Instance.PointerEnter += _deckRemainView.OnPointerEnterAtDeck;
            CardDeckVisual.Instance.PointerExit += _deckRemainView.OnPointerExitAtDeck;
            CardDeckVisual.Instance.PointerUp += _deckRemainView.OnPointerClickAtDeck;
            CardDeckVisual.Instance.transform.SetAsLastSibling();
            
            // Value Selection View 생성
            var valueSelectionViews = Object.FindObjectsByType<CardValueSelectionView>(FindObjectsSortMode.None);
            if (views is { Length: > 0}) _selectionView = valueSelectionViews[0];
            else
            {
                const string path = "UI/CardUI/ValueSelectionView";
                GameObject go = Managers.Resource.Instantiate(path, canvas);
                _selectionView = go.GetComponent<CardValueSelectionView>();
            }
            _selectionView.Init();
            _selectionView.ValueSelected += OnValueSelected;
            
            await _view.EnterHandBarAsync();
            
            // Update Async 구성
            _updateCts.Cancel();
            _updateCts = new();
            UpdateAsync(_updateCts.Token).Forget();
        }
        
        // MonoBehaviour의 Update()를 대체
        private async UniTask UpdateAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (CanInput && _state.draggedCard)
                    DetectSwap();

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        
        /// <summary>
        /// 스테이지가 종료된 후 UI를 비활성화, 
        /// 내부 업데이트 루프를 정지시킵니다.
        /// </summary>
        /// <returns>UI 비활성화 후 완료되는 <see cref="UniTask"/></returns>
        public async UniTask Exit()
        {
            // Update Async 정지
            _updateCts.Cancel();
            
            CardDeckVisual.Instance.PointerEnter -= _deckRemainView.OnPointerEnterAtDeck;
            CardDeckVisual.Instance.PointerExit -= _deckRemainView.OnPointerExitAtDeck;
            CardDeckVisual.Instance.PointerUp -= _deckRemainView.OnPointerClickAtDeck;
            
            await _view.ExitHandBarAsync();
        }
        
        /// <summary>
        /// Presenter의 내부 상태를 초기화합니다.  
        /// 상호작용 가능 여부를 비활성화하고, View를 파괴합니다.
        /// </summary>
        public void Clear()
        {
            _state.canInteract = false;
            UpdateUI();
            
            if (!_view) return;
            _view.Clear();
            Managers.Resource.Destroy(_view.gameObject);
        }

        
        private void WireCard(Card card)
        {
            card.AddDragStart(OnDragStarted);
            card.AddDragEnd(OnDragEnded);
            
            card.AddPointerDown(OnPointerDown);
            card.AddPointerUp(OnPointerUp);
            
            card.AddSelectionButtonTapped(OnValueSelectionButtonTapped);
        }
        
        private void UnwireCard(Card card)
        {
            card.RemoveDragStart(OnDragStarted);
            card.RemoveDragEnd(OnDragEnded);
            
            card.RemovePointerDown(OnPointerDown);
            card.RemovePointerUp(OnPointerUp);

            card.RemoveSelectionButtonTapped(OnValueSelectionButtonTapped);
        }
        
        #region ITurnPlayerInput

        public bool IsNoCard => false;
        private UniTaskCompletionSource cmp;

        public async UniTask DrawCard()
        {
            await DrawAsync();
        }
        
        public async UniTask WaitUserInput()
        {
            cmp = new();
            _evaluationPresenter.ClearHandRankingText();
            _state.canInteract = true;
            UpdateUI();
            
            await cmp.Task; // Input 완료까지 대기
            
            _state.canInteract = false;
            UpdateUI();
        }

        #endregion
        
        #region Card Events
        
        private void OnDragStarted()
        {
            _state.draggedCard = _state.activateCard;
            _state.activateCard = null;
            
            foreach (var card in _model.Hand) card.SetAnyCardDragged(true);
            UpdateUI();
        }

        private void OnDragEnded()
        {
            _state.draggedCard = null;
            
            foreach (var card in _model.Hand) card.SetAnyCardDragged(false);
            UpdateUI();
        }
        
        // 카드 클릭 상태를 기록
        private void OnPointerDown(Card card, CardPointerArgs args)
        {
            if (!_state.canInteract) return;
            
            _state.activateCard = card;
            _state.pointerDownTime = args.Time;
        }

        // 카드가 선택 가능 상태면 선택,
        // 이미 선택된 상태면 선택 해제를 함
        private void OnPointerUp(Card card, CardPointerArgs args)
        {
            if (!_state.canInteract) return;
            
            if (_state.activateCard != card) return;
            if (args.Time - _state.pointerDownTime > _visualSetting.ClickDetectThreshold) return;
            _state.activateCard = null;
            
            // 좌클릭 처리
            if (args.Button == MouseButton.LeftMouse)
            {
                if (_model.Selection.Contains(card))
                {
                    _model.Deselect(card);
                    card.SetSelect(false);
                }
                else if (_model.Selection.Count < 4)
                {
                    _model.Select(card);
                    card.SetSelect(true);
                }
                else return;
                
                HandChanged?.Invoke();
                _evaluationPresenter.UpdateHandRankingText(_model.Selection);
                UpdateUI();
            }
            
            // 우클릭 처리
            // TODO: 우클릭 관련 로직 추후 '전환 버튼'으로 이동
            else if (args.Button == MouseButton.RightMouse)
            {
                // if (!card.Data.CanOpenSelection) return;
                
                var selectContainers = Object.FindObjectsByType<SelectContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (selectContainers == null || selectContainers.Length == 0) { LogEx.LogError("Select Container가 씬에 존재하지 않음"); return; }
                var container = selectContainers[0];
                
                container.OpenSelection(card);
            }
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
            Card swapped = _model.GetHandCard(index);
            _model.TryGetIndex(_state.draggedCard, out int i);
            
            _model.Swap(_state.draggedCard, index);
            HandChanged?.Invoke();
            _view.SetCardToSlot(swapped, i);
            _view.SetCardToSlot(_state.draggedCard, index);
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
        #endregion

        #region Use/Draw/Discard

        private void Use()
        {
            _state.canInteract = false;
            UpdateUI();
            _evaluationPresenter.ConfigureSequence(_model.SortedSelection);
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
            await _evaluationPresenter.ExcuteSequenceAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            await DiscardAsync();

            cmp.TrySetResult();
        }

        private async UniTask DiscardAsync()
        {
            foreach (var card in _model.SortedSelection.Reverse())
            {
                _model.TryGetIndex(card, out var index);
                
                UnwireCard(card);
                _model.Discard(card);
                HandChanged?.Invoke();
                card.DoDiscard();
                
                // slot 비활성화
                _view.SetSlotActive(false, index, _model.Hand.Count);
                
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardInterval));
            }
            
            _evaluationPresenter.ClearHandRankingText();
            _view.AlignSlot();
        }

        private async UniTask DrawAsync()
        {
            _state.isSwapping = true;
            UpdateUI();
            
            int count = _model.MaxHand - _model.Hand.Count;
            int indexFactor = _model.Hand.Count;
            for (int i = 0; i < count; i++)
            {
                // 슬롯 활성화
                var card = Spawn();
                _view.SetSlotActive(true, indexFactor + i, _model.Hand.Count);
                card.DoDraw();
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DrawInterval));
            }

            _state.isSwapping = false;
            UpdateUI();
        }

        private async UniTask DiscardAndDrawAsync()
        {
            _state.isSwapping = true;
            UpdateUI();
            
            await DiscardAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardDrawInterval));
            await DrawAsync();
            
            _state.isSwapping = false;
            UpdateUI();
        }

        #endregion
        
        private Card Spawn()
        {
            var cardData = _model.PopCard();
            if (cardData == null) return null;

            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.Init(cardData, _model);

            // 이벤트 구독
            WireCard(card);
            HandChanged += card.OnHandChanged;

            _model.Draw(card);
            HandChanged?.Invoke();
            DeckChanged?.Invoke();
            UpdateUI();
            UpdateSlots();

            return card;
        }
        
        // Sort
        // - - - - - - - - - - -
        private void SortByNumber()
        {
            _model.SortByNumber();
            HandChanged?.Invoke();
            UpdateUI();
            UpdateSlots();
        }

        private void SortByIcon()
        {
            _model.SortByIcon();
            HandChanged?.Invoke();
            UpdateUI();
            UpdateSlots();
        }
        
        // UI
        // - - - - - - - - - - -
        private void UpdateUI()
        {
            if (!_view) return;
            var canUse = CanInput && _model.CanUseCard && !_state.draggedCard;
            var canDiscard = CanInput && _model.Selection.Count > 0 && !_state.draggedCard;
            var viewState = new StageCardsViewState(canUse, canDiscard, true, _model.Deck.Count, _model.DiscardRemain);
            _view.UpdateUI(viewState);
        }

        #region nested

        private struct StageCardsPresenterState
        {
            public bool isInitialized;
            
            public bool canInteract;
            public bool isSwapping;
            
            public float pointerDownTime;
            public Card activateCard;
            public Card draggedCard;
        }

        #endregion
        
        // 이동해야할 로직들
        // - - - - - - - - - 
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
        
        // 플레이어 턴이면서 카드 값 선택이 바뀔 때.
        // Card.onselectEnded에서 호출
        private void OnSelectValueEnd(Card _)
        {
            UpdateUI();
        }

        private void OnValueSelectionButtonTapped(Card card)
        {
            _selectionView.Open(card, new Vector2(0, -180));
        }

        private void OnValueSelected(Card card, (int num, Direction dir) values)
        {
            var d = card.Data;
            var error = false;

            error = d.Kind switch
            {
                CardKind.Attack => !d.NumberSelectState.TrySelect(values.num),
                CardKind.Move => !d.DirectionSelectState.TrySelect(values.dir),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (error)
                LogEx.LogWarning($"잘못된 데이터를 선택했습니다! {d.Id} : {values.num} {values.dir}");
            
            card.UpdateVisual();
        }
    }
}
