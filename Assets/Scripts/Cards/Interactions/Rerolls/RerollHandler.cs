using Cardevil.Cards.Interactions;
using Cardevil.Events;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil
{
    public class RerollHandler : MonoBehaviour, ITurnRerollInput
    {
        private UniTaskCompletionSource _rerollCmp = new();
        private bool _isPreviewing = false;

        [SerializeField] CardVisualSettingSO visual;
        [SerializeField] CardDeckVisual deck;

        [Header("Cards")]
        [SerializeField] GameObject cardPrefab;
        [Header("Slots")]
        [SerializeField] GameObject slotPrefab;
        [SerializeField] List<Transform> slots = new();

        [Header("Rerolls")]
        [SerializeField] Image backgroundPanel;
        [SerializeField] Transform bar;
        [SerializeField] TextMeshProUGUI countText;
        [Header("Buttons")]
        [SerializeField] Button endButton;
        [SerializeField] Button doRerollButton;
        [SerializeField] Button togglePreviewButton;


        void Awake()
        {
            gameObject.SetActive(false);           
        }

        // CardManager에서 호출
        public void Init()
        {
            foreach (var slot in slots)
            {
                foreach (Transform child in slot)
                    Destroy(child.gameObject);
            }
            if (slots.Count < Managers.Card.MaxHandCount)
            {
                for (int i = 0; i < Managers.Card.MaxHandCount; i++)
                    slots.Add(Instantiate(original: slotPrefab, bar).transform);
            }

            Managers.Event.RerollTicketChangeEvent.AddListener(OnTicketCountChanged, 1);

            void AddListener(Button btn, UnityAction listener) => btn.onClick.AddListener(listener);
            AddListener(endButton, EndReroll);
            AddListener(doRerollButton, DoReroll);
            AddListener(togglePreviewButton, TogglePreview);

            // 임시
            deck = GameObject.Find("Deck Button").GetComponent<CardDeckVisual>();
        }

        void OnDestroy()
        {
            Managers.Event.RerollTicketChangeEvent.RemoveListener(OnTicketCountChanged);
        }



        // ITurnRerollInput Interface
        public async UniTask RerollCard()
        {
            gameObject.SetActive(true);
            Managers.Game.PlayerStatus.RerollTicket = 5; // 임시
            _ = RerollAsync();
            await _rerollCmp.Task;
        }

        private async UniTask RerollAsync()
        {
            var ctx = Managers.Card.StageCardsCtx;

            var draw = visual.RerollDrawInterval;
            var discard = visual.RerollDiscardInterval;
            var drawDiscard = visual.RerollDrawDiscardInterval;
            var autoEnd = visual.EndRerollInterval;
            var end = .5f;

            void SetInteractable(bool v)
            {
                doRerollButton.interactable = v;
                endButton.interactable = v;
                togglePreviewButton.interactable = v;
            }

            SetInteractable(false);

            try
            {
                // 버리기 Tween
                foreach (var card in ctx.Hand)
                {
                    card.RerollDiscarded?.Invoke(deck.Front);
                    await UniTask.Delay(TimeSpan.FromSeconds(discard));
                }

                // 대기 후 셔플
                await UniTask.Delay(TimeSpan.FromSeconds(visual.RerollDrawDiscardInterval));
                ctx.Shuffle();

                Card Spawn()
                {
                    var cardData = ctx.PopCard();
                    if (cardData == null) return null;
                    var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
                    card.SpawnAsReroll(cardData);

                    ctx.Draw(card);
                    if (!ctx.TryGetIndex(card, out var idx)) return null;
                    card.UpdateIndex(slots[idx], idx);

                    return card;
                }

                // 카드 소환
                for (int i = 0; i < slots.Count; i++)
                {
                    Spawn().RerollDrawn?.Invoke();
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
                SetInteractable(true);
            }
            catch (Exception ex)
            {
                LogEx.LogError($"리롤 중 오류!: {ex}");
            }
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

        private async UniTask EndRerollAsync()
        {
            var manager = Managers.Card;
            var ctx = manager.StageCardsCtx;
            var hand = new List<Card>(ctx.Hand);
            var end = visual.EndRerollUpdateSlotInterval;

            backgroundPanel.gameObject.SetActive(false);

            for (int i = 0; i < hand.Count; i++)
            {
                manager.HandBar.MoveToHandBar(i);
                await UniTask.Delay(TimeSpan.FromSeconds(end));
            }

            _rerollCmp.TrySetResult();
            Destroy(gameObject);
        }

        private void TogglePreview()
        {
            _isPreviewing = !_isPreviewing;

        }



        public void OnTicketCountChanged(RerollTicketChangeArgs args)
        {
            _ = OnTicketCountChangedAsync(args);
        }

        private async UniTask OnTicketCountChangedAsync(RerollTicketChangeArgs args)
        {
            var remain = args.OldTicket;
            var delta = args.NewTicket - args.OldTicket;
            if (delta == 0)
            {
                countText.text = remain.ToString();
                return;
            }

            var tr = countText.transform;
            var half = visual.RerollCountScaleDuration * .5f;
            var ease = visual.RerollCountEase;
            var dir = Math.Sign(delta);
            var steps = Math.Abs(delta);

            // 리롤권 순차적 변경 tween
            for (int i = 0; i < steps; i++)
            {
                remain += dir;
                countText.text = remain.ToString();
                var tween = tr.DOScale(visual.RerollCountScale, half)
                    .SetEase(ease)
                    .SetLoops(2, LoopType.Yoyo);
                await tween.AsyncWaitForCompletion();
            }
        }
    }
}