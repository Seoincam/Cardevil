using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.DataStructure.Serializables;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
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
        [SerializeField] private CardVisualUI[] cardVisuals;
        
        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;
        
        private CanvasGroup _canvasGroup;
        private bool _isVisible;
        private bool _isClicked;
        private Tween _tween;

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
            // _canvasGroup.interactable = false;
            // _canvasGroup.blocksRaycasts = false;
            
            _isVisible = false;
            gameObject.SetActive(false);
            OnDeckChanged();
        }
        
        /// <summary>
        /// 덱에 변경이 있을 때마다 UI를 업데이트함.
        /// </summary>
        public void OnDeckChanged()
        {
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
                if (_isVisible)
                    cardVisuals[id].SetStateAsync(false).Forget();
                else 
                    cardVisuals[id].SetStateImmediate(false);
                
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
            gameObject.SetActive(true);
            PlayAnimationAsync().Forget();
        }

        private void Close()
        {
            _isVisible = false;
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

        private async UniTaskVoid PlayAnimationAsync()
        {
            foreach (var card in cardVisuals)
            {
                card.CanvasGroup.alpha = 0;

                if (setting.animType == DeckRemainViewAnimSetting.AnimType.Pop)
                {
                    float randomScale = Random.Range(0.3f, 1.0f);
                    card.Rect.localScale = Vector3.one * CardScale * randomScale;
                }
                else
                {
                    card.Rect.anchoredPosition += Vector2.up * setting.startY;   
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(setting.startInterval));

            for (int i = 0; i < cardVisuals.Length; i++)
            {
                int row = i / 5;
                int col = i % 10;
                float d = (col * setting.angle + row) * setting.delay;
                
                AnimateCard(cardVisuals[i], d).Forget();
            }
        }

        private async UniTaskVoid AnimateCard(CardVisualUI card, float d)
        {
            LogEx.Log("wait start");
            await UniTask.Delay(TimeSpan.FromSeconds(d));
            LogEx.Log("Wait end");
            
            var seq = DOTween.Sequence();
            
            if (setting.animType == DeckRemainViewAnimSetting.AnimType.Pop)
            {
                card.Rect.localScale = Vector3.one * CardScale * .3f;
                card.CanvasGroup.alpha = 0f;

                seq.Append(card.CanvasGroup.DOFade(1f, setting.duration * .5f))
                    .Join(card.Rect.DOScale(CardScale * 1.05f, setting.duration * .5f).SetEase(Ease.OutBack))
                    .Append(card.Rect.DOScale(CardScale, setting.duration * .2f).SetEase(Ease.OutQuad));
            }
            
            else if (setting.animType == DeckRemainViewAnimSetting.AnimType.FadeInUp)
            {
                var originalPos = card.Rect.anchoredPosition;
                card.Rect.anchoredPosition = originalPos + new Vector2(0, -setting.startY);
                card.CanvasGroup.alpha = 0f;
                
                seq.Append(card.CanvasGroup.DOFade(1f, setting.duration))
                    .Join(card.Rect.DOAnchorPos(originalPos, setting.duration).SetEase(Ease.OutCubic));
            }
        }
    }
}

