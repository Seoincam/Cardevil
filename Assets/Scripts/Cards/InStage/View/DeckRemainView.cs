using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
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
        public enum AnimType { Type1, Type2 }

        [Header("Anim Setting")]
        public AnimType animType;          // 애니메이션 모드(type1,2,3)
        public int delay;                // 카드 간 딜레이(ms)
        public float duration;           // 개별 카드 애니메이션 시간
        public float angle;              // 딜레이 계산에 쓰는 기울기
        public float startY;             // 시작 Y offset
        public float startOpacity;       // 시작 투명도

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
 
                float randomScale = Random.Range(0.3f, 1.0f);
                card.Rect.localScale = Vector3.one * CardScale * randomScale;
                card.Rect.anchoredPosition += Vector2.up * startY;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            for (int i = 0; i < cardVisuals.Length; i++)
            {
                int row = i / 5;
                int col = i % 10;
                float d = (col * angle + row) * delay / 1000f;
                AnimateCard(cardVisuals[i], d).Forget();
            }
        }

        private async UniTaskVoid AnimateCard(CardVisualUI card, float d)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(d));

            var seq = DOTween.Sequence();
            
            // Pop
            if (animType == AnimType.Type1)
            {
                card.Rect.localScale = Vector3.one * CardScale * .3f;
                card.CanvasGroup.alpha = 0f;

                seq
                    .Append(card.CanvasGroup.DOFade(1f, duration * .5f))
                    .Join(card.Rect.DOScale(CardScale * 1.05f, duration * .5f).SetEase(Ease.OutBack))
                    .Append(card.Rect.DOScale(CardScale, duration * .2f).SetEase(Ease.OutQuad));
            }
            
            // fade in up
            else if (animType == AnimType.Type2)
            {
                var originalPos = card.Rect.anchoredPosition;
                card.Rect.anchoredPosition = originalPos + new Vector2(0, -startY);
                card.CanvasGroup.alpha = 0f;
                seq
                    .Append(card.CanvasGroup.DOFade(1f, duration))
                    .Join(card.Rect.DOAnchorPos(originalPos, duration).SetEase(Ease.OutCubic));
            }
        }
    }
}

