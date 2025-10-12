using Cardevil.Core;
using Cardevil.Events;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cardevil.Cards.Interactions
{
    public class RerollPresenter : ITurnRerollInput, IClearable
    {
        private StageCardsModel _model;
        private RerollView _view;
        private CardVisualSettingSO _visualSetting;
        
        private int _maxHandCount;
        private bool _isPreviewing;
        private UniTaskCompletionSource _cmp;

        [SerializeField] CardDeckVisual deck;

        public void Init(StageCardsModel model)
        {
            if (model == null)
            {
                LogEx.LogError("Init() 실패 — model이 null입니다.");
                return;
            }
            _model = model;
            
            // View 찾기
            var views = Object.FindObjectsByType<RerollView>(FindObjectsSortMode.None);
            if (views == null || views.Length == 0)
            {
                LogEx.LogError("RerollView를 찾을 수 없습니다. 씬에 View가 배치되어 있는지 확인하세요.");
                return;
            }
            _view = views[0];
            _view.BindButtonEvents(DoReroll, EndReroll);
            
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
            _maxHandCount = maxHandCount;
            _view.ConfigureSlots(maxHandCount);
        }
        
        public void Clear()
        {
            _cmp = null;
        }

        private void DoReroll()
        {
            Managers.Game.PlayerStatus.RerollTicket--;
            _ = RerollAsync();
        }

        private void EndReroll()
        {
            _ = EndRerollAsync();
        }

        // ITurnRerollInput Interface
        public async UniTask RerollCard()
        {
            var newTicketValue = 5; // 임시
            Managers.Game.PlayerStatus.RerollTicket = newTicketValue;
            await _view.AnimateTicketChangeAsync(Managers.Game.PlayerStatus.RerollTicket, newTicketValue);
            
            _ = RerollAsync();
            await _cmp.Task;
        }

        private async UniTask RerollAsync()
        {
            var draw = _visualSetting.RerollDrawInterval;
            var discard = _visualSetting.RerollDiscardInterval;
            var drawDiscard = _visualSetting.RerollDrawDiscardInterval;
            var autoEnd = _visualSetting.EndRerollInterval;
            var end = .5f;
            
            _view.SetInteractable(false);

            try
            {
                // 버리기 Tween
                foreach (var card in _model.Hand)
                {
                    card.RerollDiscarded?.Invoke(deck.Front);
                    await UniTask.Delay(TimeSpan.FromSeconds(discard));
                }

                // 대기 후 셔플
                await UniTask.Delay(TimeSpan.FromSeconds(_visualSetting.RerollDrawDiscardInterval));
                _model.Shuffle();

                // 카드 소환
                for (int i = 0; i < _maxHandCount; i++)
                {
                    var card = Spawn();
                    card.RerollDrawn?.Invoke();
                    await UniTask.Delay(TimeSpan.FromSeconds(draw));
                }

                // 리롤권 소진시 자동 종료
                if (Managers.Game.PlayerStatus.RerollTicket <= 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(autoEnd));
                    EndReroll();
                    return;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(end));
                _view.SetInteractable(true);
            }
            catch (Exception ex)
            {
                LogEx.LogError($"리롤 중 오류!: {ex}");
            }

            return;
        }
        
        private Card Spawn()
        {
            var cardData = _model.PopCard();
            if (cardData == null) return null;
            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.SpawnAsReroll(cardData);

            _model.Draw(card);
            if (!_model.TryGetIndex(card, out var idx)) return null;
            _view.SetCardToSlot(card, idx);

            return card;
        }

        private async UniTask EndRerollAsync()
        {
            var manager = Managers.Card;
            var hand = new List<Card>(_model.Hand);
            var end = _visualSetting.EndRerollUpdateSlotInterval;

            // backgroundPanel.gameObject.SetActive(false);

            for (int i = 0; i < hand.Count; i++)
            {
                manager.StageCardsPresenter.MoveToHandBar(i);
                await UniTask.Delay(TimeSpan.FromSeconds(end));
            }

            _cmp.TrySetResult();
            // Destroy(gameObject);
        }
    }
}