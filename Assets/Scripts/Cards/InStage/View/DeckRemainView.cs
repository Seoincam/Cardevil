using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Cards.Visual.Handler;
using Cardevil.DataStructure.Serializables;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Cardevil.Cards.InStage.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DeckRemainView : UI_Popup
    {        
        [Header("SO")] 
        [SerializeField] private DeckRemainViewAnimSetting setting;

        [Header("Summary Area")] 
        [SerializeField] private SerializableDictionary<string, TextMeshProUGUI> countTexts;
        [SerializeField] private Button closeButton;
        
        [Header("Cards Area")]
        [SerializeField] private CardVisualDeckRemainView[] cardVisuals;
        
        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;
        
        private CanvasGroup _canvasGroup;
        private bool _isVisible;
        private bool _changedWhenNotVisible;
        private bool _isClicked;
        private Tween _tween;

        private CancellationTokenSource _openAnimCts;

        private const float CardScale = .48f;

        public void Init(IReadOnlyCardLibrary library, IReadOnlyStageCardsModel model)
        {
            _library = library;
            _model = model;

            if (cardVisuals is not { Length: 50 })
            {
                LogEx.LogError("CardVisualUI가 제대로 할당되지 않음!");
                return;
            }

            // UI 바인딩
            for (int i = 0; i < cardVisuals.Length; i++)
                UpdateSpriteSet(i);
            foreach (var card in _model.Hand)
            {
                int id = card.Data.Id;
                cardVisuals[id].SetStateImmediate(false);
            }
            
            closeButton.onClick.AddListener(Close);
            
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _isVisible = false;
            gameObject.SetActive(false);
            OnDeckChanged();
        }
        
        /// <summary>
        /// 덱에 변경이 있을 때마다 UI를 업데이트함.
        /// </summary>
        public void OnDeckChanged()
        {
            if (!_isVisible)
            {
                _changedWhenNotVisible = true;
                return;
            }
            
            int red = 10, green = 10, blue = 10, black = 10, arrow = 10;
            
            foreach (var handCard in _model.Hand)
                DecreaseCountById(handCard.Data.Id, ref red, ref green, ref blue, ref black, ref arrow);

            foreach (var discarded in _model.DiscardPile)
                DecreaseCountById(discarded.Id, ref red, ref green, ref blue, ref black, ref arrow);

            int allRemains = red + green + blue + black + arrow;
            countTexts["all"].text = $"{allRemains}/50";
            countTexts["red"].text = red.ToString();
            countTexts["green"].text = green.ToString();
            countTexts["blue"].text = blue.ToString();
            countTexts["black"].text = black.ToString();
            countTexts["arrow"].text = arrow.ToString();
            
            void DecreaseCountById(int id, ref int red, ref int green, ref int blue, ref int black, ref int arrow)
            {
                cardVisuals[id].SetStateAsync(false).Forget();
                
                var group = id / 10;
                switch (group)
                {
                    case 0: red--; break;
                    case 1: green--; break;
                    case 2: blue--; break;
                    case 3: black--; break;
                    case 4: arrow--; break;
                }
            }
        }

        public void Open()
        {
            _isVisible = true;
            if (_changedWhenNotVisible)
                OnDeckChanged();
            
            gameObject.SetActive(true);

            _openAnimCts?.Cancel();
            _openAnimCts?.Dispose();
            _openAnimCts ??= new CancellationTokenSource();
            
            PlayAnimationAsync(_openAnimCts.Token).Forget();
        }

        private void Close()
        {
            _openAnimCts?.Cancel();
            _openAnimCts?.Dispose();
            _openAnimCts = null;
            
            _isVisible = false;
            _changedWhenNotVisible = false;
            gameObject.SetActive(false);
        }

        private void UpdateSpriteSet(int index)
        {
            if (index < 0 || index >= cardVisuals.Length)
            {
                LogEx.LogError("Index out of range!");
                return;
            }
            
            var cardVisualUI = cardVisuals[index];
            if (!cardVisualUI)
            {
                LogEx.LogError($"CardVisual_UI[{index}] is null!");
                return;
            }

            var data = _library.GetCardDataById(index);
            cardVisualUI.Init(data);
        }

        private async UniTaskVoid PlayAnimationAsync(CancellationToken ct)
        {
            foreach (var visual in cardVisuals)
                visual.CanvasGroup.alpha = 0;

            bool delayCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(setting.startInterval), cancellationToken: ct)
                .SuppressCancellationThrow();
            
            if (delayCanceled)
                return;
            
            for (int i = 0; i < cardVisuals.Length; i++)
            {
                int row = i / 5;
                int col = i % 10;
                float d = (col * setting.angle + row) * setting.delay;

                AnimateCard(cardVisuals[i], d, ct).Forget();
            }
        }

        private async UniTaskVoid AnimateCard(CardVisualDeckRemainView card, float startDelay, CancellationToken ct)
        {
            var originalPos = card.Rect.anchoredPosition;
            card.Rect.anchoredPosition -= Vector2.up * setting.startY;
            
            // 대기
            bool delayCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: ct)
                .SuppressCancellationThrow();
                
            if (delayCanceled)
                return;
            
            // 애니메이션
            card.Rect.DOKill();
            card.CanvasGroup.DOKill();

            bool animCanceled = await DOTween.Sequence()
                .Join(card.CanvasGroup.DOFade(1f, setting.duration))
                .Join(card.Rect.DOAnchorPos(originalPos, setting.duration).SetEase(Ease.OutCubic))
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();

            if (animCanceled)
                card.Rect.anchoredPosition = originalPos;
        }
    }
}

