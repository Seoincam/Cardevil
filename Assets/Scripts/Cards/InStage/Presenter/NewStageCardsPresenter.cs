using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.NCard;
using Cardevil.Cards.InStage.View;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace Cardevil.Cards.InStage.Presenter
{
    // Core
    public partial class NewStageCardsPresenter
    {
        private NewStageCardsModel _model;
        
        private Action _handChanged;
        private Action _deckChanged;

        private State _state;

        private bool CanInteract => !Is(State.Discarding) && !Is(State.Drawing) && !Is(State.DiscardAndDrawInterval);
        
        public void Initialize(NewStageCardsModel model)
        {
            Debug.Assert(_model != null);
            
            _model = model;

            int priority = (int)EnterStageArgs.Orders.Last;
            ExecStaticEventBus<EnterStageArgs>.Register(priority, OnEnterStageAsync);
        }

        public async UniTask OnEnterStageAsync(EnterStageArgs args, CancellationToken cancellationToken)
        {
            Transform canvas = GameObject.Find("CardCanvas").transform;
            
            // 1. View 생성
            
            // 2. Deck Remain View
            
            // 3. Value Selection View
            
            // 생성되어 있는 카드를 모두 HandBar Slot으로 이동
            
            // Update Async
            UpdateAsync(cancellationToken).Forget();
        }
        
        // MonoBehaviour의 Update()를 대체함.
        private async UniTask UpdateAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateDetectSwap();
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
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
                
                    // TODO: 연출 확인하기
                    discardTasks.Add(card.SafeMoveToDeckWithFlipAsync());
                    // TODO: await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardInterval));
                }
                await UniTask.WhenAll(discardTasks);
            }
            
            // TODO: UI
            ListPool<UniTask>.Release(discardTasks);
        }

        private async UniTask DrawUntilMaxHandAsync()
        {
            var drawTasks = ListPool<UniTask>.Get();

            using (Scope(State.Drawing))
            {
                int count = _model.MaxHand - _model.Hand.Count;
                while (count-- > 0)
                {
                    var card = SpawnCard();
                    if (!card) continue;

                    drawTasks.Add(card.SafeMoveToSlotWithFlipAsync());
                    // TODO: await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DrawInterval));
                }
                await UniTask.WhenAll(drawTasks);
            }
            
            // TODO: UI
            ListPool<UniTask>.Release(drawTasks);
        }

        private NewCard SpawnCard()
        {
            const string cardPath = "Cards/Card!!!";

            var cardData = _model.PopCardData();
            if (cardData == null) return null;

            var card = AssetUtil.Instantiate(cardPath).GetComponent<NewCard>();
            card.Initialize(cardData);
            BindCallback(card);
            
            _model.AddHand(card);
            
            // TODO:
            // HandChanged?.invoke
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
            DiscardAndDrawInterval = 1 << 2
        }

        private bool Is(State targetState) => (_state & targetState) != 0;

        private void Set(State targetState, bool value)
        {
            _state = (_state &= ~targetState) | (value ? targetState : 0);
        }

        private readonly struct StateScope : IDisposable
        {
            private readonly NewStageCardsPresenter _presenter;
            private readonly State _targetState;
            private readonly bool _originalValue;

            public StateScope(NewStageCardsPresenter presenter, State targetState, bool value)
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