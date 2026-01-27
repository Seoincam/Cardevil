using Cardevil.Cards.Utils;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.InStage
{
    public partial class StageCardsPresenter : IDisposable
    {
        private StageCardsModel _model;
        private StageCardsView _view;
        private DeckRemainView _deckRemainView;
        
        private State _state;
        private bool _completePlayerInput;

        private bool CanInteract => Is(State.PlayerTurn) &&
                                    !Is(State.Discarding) &&
                                    !Is(State.Drawing) &&
                                    !Is(State.Sorting) &&
                                    !Is(State.DiscardAndDrawInterval);
        
        public StageCardsPresenter(StageCardsModel model)
        {
            Debug.Assert(model != null);
            _model = model;

            ExecEventBus<EnterStageArgs>.RegisterStatic(
                (int)EnterStageArgs.Orders.Last, 
                OnEnterStageAsync);
            
            ExecEventBus<PlayerTurnStartArgs>.RegisterStatic(
                (int)PlayerTurnStartArgs.Orders.DrawCards, 
                OnPlayerTurnStartAsync);
        }
        
        public void Dispose()
        {
            ExecEventBus<EnterStageArgs>.UnregisterStatic(OnEnterStageAsync);
        }

        /// <summary>
        /// UI 초기화, 카드를 슬롯에 배치, 버튼 이벤트 바인딩. Update 루프 시작.
        /// </summary>
        private async UniTask OnEnterStageAsync(EnterStageArgs args, CancellationToken cancellationToken)
        {
            Transform canvas = GameObject.Find("CardCanvas").transform;
            
            // 1. View 생성
            var views = Object.FindObjectsByType<StageCardsView>(FindObjectsSortMode.None);
            if (views is { Length: > 0 }) _view = views[0];
            else
            {
                GameObject go = AssetUtil.Instantiate(CardAssetPath.StageCardsView, canvas);
                _view = go.GetComponent<StageCardsView>();
            }
            _view.Init(_model);
            _view.BindButtonEvents(
                OnUseButtonClicked, 
                OnDiscardButtonClicked, 
                OnSortByNumberButtonClicked, 
                OnSortByIconButtonClicked);
            
            // TODO: 2. Deck Remain View
            
            // TODO: 3. Value Selection View
            
            // 4. 생성되어 있는 카드를 모두 HandBar Slot으로 이동
            _view.UpdateAllCardsParentSlot();

            var moveTasks = ListPool<UniTask>.Get();
            using (Scope(State.Drawing))
            {
                foreach (var card in _model.Hand)
                {
                    card.gameObject.name = $"{_model.GetIndexInHand(card)}";
                    
                    BindCallback(card);
                    card.Set(StageCard.State.Rerolling, false);
                    moveTasks.Add(card.MoveOnRerollEnd());
                }
                await UniTask.WhenAll(moveTasks);
            }
            ListPool<UniTask>.Release(moveTasks);


            await _view.EnterHandBarAsync(_model.Deck.Count, _model.DiscardRemain);
        }

        /// <summary>
        /// 플레이어의 인풋이 끝날 때까지 대기.
        /// </summary>
        public async UniTask WaitPlayerInputCompleted(CancellationToken cancellationToken)
        {
            _completePlayerInput = false;
            
            using (Scope(State.PlayerTurn))
            {
                // TODO: UI 갱신
                await UniTask.WaitUntil(() => _completePlayerInput);
                // TODO: UI 갱신
            }
        }

        public async UniTask UseAllCardsAsync()
        {
            
        }

        private void OnUseButtonClicked()
        {
            _completePlayerInput = true;
            // TODO: UI 갱신
        }

        private void OnDiscardButtonClicked()
        {
            DiscardAndDrawAsync().Forget();
        }

        private void OnSortByNumberButtonClicked()
        {
            SortByNumberAsync().Forget();
        }

        private void OnSortByIconButtonClicked()
        {
            SortByIconAsync().Forget();
        }

        private async UniTask OnPlayerTurnStartAsync(PlayerTurnStartArgs args, CancellationToken cancellationToken)
        {
            await DrawUntilMaxHandAsync();
        }

        private async UniTask DiscardAndDrawAsync()
        {
            // TODO: Update UI
            await DiscardSelectingCardsAsync();
            using (Scope(State.DiscardAndDrawInterval))
            {
                // TODO: await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardDrawInterval));
            }
            await DrawUntilMaxHandAsync();
            // TODO: Update UI
        }
        
        // 버리기를 눌러서 버려질때만 호출됨.
        // 그 외에는 머리 위에 쌓여서 다른 Presenter가 제어함.
        private async UniTask DiscardSelectingCardsAsync()
        {
            var discardTasks = ListPool<UniTask>.Get();
            
            using (Scope(State.Discarding))
            {
                foreach (var card in _model.GetSortedSelection().Reverse())
                {
                    var args = EachCardDiscardedArgs.Get(card);
                    ExecEventBus<EachCardDiscardedArgs>.InvokeSequentiallyAndDispose(args).Forget();
                
                    UnbindCallback(card);
                    _model.Discard(card);
                    
                    discardTasks.Add(card.FadeOutAsync());
                    // TODO: await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardInterval));
                }
                await UniTask.WhenAll(discardTasks);
            }
            
            // TODO: UI
            ListPool<UniTask>.Release(discardTasks);
        }

        public async UniTask DrawUntilMaxHandAsync()
        {
            var drawTasks = ListPool<UniTask>.Get();

            using (Scope(State.Drawing))
            {
                int count = _model.MaxHand - _model.Hand.Count;
                while (count-- > 0)
                {
                    var card = InstantiateCard();
                    if (!card) continue;

                    drawTasks.Add(card.DrawAsync());
                    // TODO: await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DrawInterval));
                }
                await UniTask.WhenAll(drawTasks);
            }
            
            // TODO: UI
            ListPool<UniTask>.Release(drawTasks);
        }

        private async UniTask SortByNumberAsync()
        {
            var sortTasks = ListPool<UniTask>.Get();

            using (Scope(State.Sorting))
            {
                _model.SortHandByNumber();
                _view.UpdateAllCardsParentSlot();
                
                // foreach (var card in _model.Hand)
                //     sortTasks.Add(card.MoveToSlotAsync());
                // await UniTask.WhenAll(sortTasks);
            }
            
            ListPool<UniTask>.Release(sortTasks);
        }

        private async UniTask SortByIconAsync()
        {
            var sortTasks = ListPool<UniTask>.Get();

            using (Scope(State.Sorting))
            {
                _model.SortHandByIcon();
                _view.UpdateAllCardsParentSlot();
                
                // foreach (var card in _model.Hand)
                //     sortTasks.Add(card.MoveToSlotAsync());
                // await UniTask.WhenAll(sortTasks);
            }
            
            ListPool<UniTask>.Release(sortTasks);
        }

        private StageCard InstantiateCard()
        {
            var cardData = _model.PopCardData();
            if (cardData == null) return null;

            var card = AssetUtil.Instantiate(CardAssetPath.Card).GetComponent<StageCard>();
            card.Initialize(cardData);
            BindCallback(card);
            
            _model.AddHand(card);
            _view.UpdateAllCardsParentSlot();
            
            // TODO:
            // deckchanged?.invoke
            // updateui

            return card;
        }

        [Flags]
        private enum State
        {
            None = 0,
            
            /// <summary>
            /// 카드를 뽑고 있음.
            /// </summary>
            Drawing = 1 << 0,
            
            /// <summary>
            /// 카드를 버리고 있음.
            /// </summary>
            Discarding = 1 << 1,
            
            /// <summary>
            /// 카드를 버리고 다시 뽑기 전 인터벌 상태임.
            /// </summary>
            DiscardAndDrawInterval = 1 << 2,
            
            /// <summary>
            /// 현재 플레이어의 턴임.
            /// </summary>
            PlayerTurn = 1 << 3,
            
            /// <summary>
            /// 정렬 중임.
            /// </summary>
            Sorting = 1 << 4,
        }

        private bool Is(State targetState) => (_state & targetState) != 0;

        private void Set(State targetState, bool value)
        {
            _state = (_state &= ~targetState) | (value ? targetState : 0);
        }

        // Using을 사용해 해당 블록에서만 특정 상태가 적용되도록 만들었음
        private readonly struct StateScope : IDisposable
        {
            private readonly StageCardsPresenter _presenter;
            private readonly State _targetState;
            private readonly bool _originalValue;

            public StateScope(StageCardsPresenter presenter, State targetState, bool value)
            {
                _presenter = presenter;
                _targetState = targetState;
                _originalValue = _presenter.Is(targetState);
                
                _presenter.Set(targetState, value);
            }
            
            public void Dispose()
            {
                _presenter.Set(_targetState, _originalValue);
            }
        }

        private StateScope Scope(State targetState, bool value = true) => new(this, targetState, value);
    }
}