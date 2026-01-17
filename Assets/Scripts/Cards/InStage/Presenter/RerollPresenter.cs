using Cardevil.Cards.Config;
using Cardevil.Cards.Utils;
using Cardevil.Core.Bootstrap;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.InStage
{
    public class RerollPresenter : IDisposable
    {
        private StageCardsModel _model;
        private RerollView _view;
        private CardVisualSettingSO _visualSetting;

        // 버리기-뽑기 트윈 재생 중인가.
        private bool _discardAndDrawing;
        
        // 리롤을 끝냈는가 (다 쓰거나, 스스로 종료).
        private bool _completeReroll;

        private static int RerollTicket => CardevilCore.Instance.Game.PlayerStatus.RerollTicket;
        private static bool HasRerollTicket => RerollTicket > 0; 
        
        public RerollPresenter(StageCardsModel model)
        {
            Debug.Assert(model != null, "[Reroll Presenter] Cards Model cannot be null.");
            _model = model;
            
            // SO 로드
            string path = "ScriptableObjects/Cards/CardVisualSetting";
            _visualSetting = Resources.Load<CardVisualSettingSO>(path);
            Debug.Assert(_visualSetting, $"CardVisualSettingSO 로드 실패. 경로가 올바른지 확인하세요: {path}");
            
            // 이벤트 등록
            int priority = (int)EnterStageArgs.Orders.Reroll;
            ExecEventBus<EnterStageArgs>.RegisterStatic(priority, OnEnterStage, 0);
            ExecEventBus<EnterStageArgs>.RegisterStatic(priority, WaitPlayerRerollCompleted, 1);
        }
        
        public void Dispose()
        {
            ExecEventBus<EnterStageArgs>.UnregisterStatic(OnEnterStage);
            ExecEventBus<EnterStageArgs>.UnregisterStatic(WaitPlayerRerollCompleted);
        }

        // 리롤 View를 구성 및 초기 상태 설정. 슬롯 수 지정하고 버튼 이벤트 바인딩.
        private async UniTask OnEnterStage(EnterStageArgs args, CancellationToken cancellationToken)
        { 
            _model.ClearAndShuffle();
            
            // View 구성
            var views = Object.FindObjectsByType<RerollView>(FindObjectsSortMode.None);
            if (views is { Length: > 0} ) _view = views[0];
            else
            {
                Transform canvas = GameObject.Find("CardCanvas").transform;
                GameObject go = AssetUtil.Instantiate("UI/CardUI/RerollView", canvas);
                _view = go.GetComponent<RerollView>();
            }
            
            _view.Init(_visualSetting);
            _view.ConfigureSlots(_model.MaxHand);
            _view.BindButtonEvents(OnDoRerollButtonClicked, OnEndRerollButtonClicked);

            await _view.EnterStageAsync();
            _view.SetInteractable(false);
            await DrawUntilMaxHandAsync();
            _view.SetInteractable(true);
            if (!HasRerollTicket)
            {
                _completeReroll = true;
            }
        }

        private async UniTask WaitPlayerRerollCompleted(EnterStageArgs args, CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _completeReroll, cancellationToken: cancellationToken);
            await _view.ExitRerollAsync();
        }

        private void OnDoRerollButtonClicked()
        {
            var previous = RerollTicket;
            CardevilCore.Instance.Game.PlayerStatus.RerollTicket--;

            _view.AnimateTicketChangeAsync(previous, RerollTicket);
            DiscardAndDrawAsync().Forget();
        }

        private void OnEndRerollButtonClicked()
        {
            _completeReroll = true;
        }

        private async UniTask DiscardAndDrawAsync()
        {
            _view.SetInteractable(false);
            
            CardevilCore.Instance.Game.PlayerStatus.RerollTicket--;
            await DiscardAllCardsAsync();
            await DrawUntilMaxHandAsync();

            if (!HasRerollTicket)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.EndRerollInterval));
                _completeReroll = true;
                return;
            }
            
            _view.SetInteractable(true);
        }
        
        private async UniTask DiscardAllCardsAsync()
        {
            var discardTasks = ListPool<UniTask>.Get();

            foreach (var card in _model.Hand)
            {
                discardTasks.Add(card.SafeMoveToDeckWithFlipAsync());
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DiscardInterval));
            }
            await UniTask.WhenAll(discardTasks);
            
            ListPool<UniTask>.Release(discardTasks);
            _model.ClearAndShuffle();
        }

        private async UniTask DrawUntilMaxHandAsync()
        {
            var drawTasks = ListPool<UniTask>.Get();
            int count = _model.MaxHand;

            while (count-- > 0)
            {
                var card = InstantiateCard();
                drawTasks.Add(card.SafeMoveToSlotWithFlipAsync());
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.DrawInterval));
            }
            await UniTask.WhenAll(drawTasks);
            
            ListPool<UniTask>.Release(drawTasks);
        }
        
        private Card InstantiateCard()
        {
            var cardData = _model.PopCardData();
            if (cardData == null) return null;

            var card = AssetUtil.Instantiate(CardAssetPath.Card).GetComponent<Card>();
            card.Initialize(cardData);
            card.Set(Card.State.Rerolling, true);
            
            _model.AddHand(card);
            _view.SetCardToSlot(card, _model.GetIndexInHand(card));
            card.SafeMoveToSlotWithFlipAsync().Forget();
            
            return card;
        }
    }
}