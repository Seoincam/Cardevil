using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Utils;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.InStage.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DeckRemainView : UI_Popup
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CardVisualUI[] cardVisuals;
        [SerializeField] private CardVisualSettingSO visualSO;

        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;

        private bool _isVisible;
        private bool _isClicked;
        private Tween _tween;

        public void Init(IReadOnlyCardLibrary library, IReadOnlyStageCardsModel model)
        {
            _library = library;
            _model = model;

            if (cardVisuals is not { Length: 50 })
            {
                LogEx.LogError("CardVisualUI가 제대로 할당되지 않음!");
                return;
            }

            for (int i = 0; i < cardVisuals.Length; i++)
                UpdateSpriteSet(i);
            
            // 해당 시점에서 이미 손패가 결정되어있음.
            foreach (var card in _model.Hand)
            {
                int id = card.Data.Id;
                cardVisuals[id].SetStateImmediate(false);
            }

            _isVisible = false;
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnPointerEnterAtDeck()
        {
            if (_isClicked) return;
            _isVisible = true;
            _tween?.Kill();
            
            const float targetAlpha = 1f;

            canvasGroup.interactable = false;
            
            _tween = canvasGroup.DOFade(targetAlpha, visualSO.DeckRemainViewToggleDur)
                .SetLink(gameObject)
                .SetRecyclable(true);
            
            _tween.OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        public void OnPointerExitAtDeck()
        {
            if (_isClicked) return;
            _tween?.Kill();
            
            const float targetAlpha = 0f;

            canvasGroup.interactable = false;
            
            _tween = canvasGroup.DOFade(targetAlpha, visualSO.DeckRemainViewToggleDur)
                .SetLink(gameObject)
                .SetRecyclable(true);
            
            _tween.OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                _isVisible = false;
            });
        }

        public void OnPointerClickAtDeck()
        {
            _isClicked = !_isClicked;
        }
        
        /// <summary>
        /// 덱에 변경이 있을 때마다 UI를 업데이트함.
        /// </summary>
        public void OnDeckChanged()
        {
            int id;
            
            foreach (var handCard in _model.Hand)
            {
                id = handCard.Data.Id;
                if (_isVisible)
                    cardVisuals[id].SetStateAsync(false).Forget();
                else 
                    cardVisuals[id].SetStateImmediate(false);
            }

            foreach (var discarded in _model.DiscardPile)
            {
                id = discarded.Id;
                if (_isVisible)
                    cardVisuals[id].SetStateAsync(false).Forget();
                else 
                    cardVisuals[id].SetStateImmediate(false);
            }
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

            var spriteSet = _library.GetVisualSpriteSetById(index);
            if (spriteSet == null)
            {
                LogEx.LogError($"spriteSet is null! id: {index}");
                return;
            }
                
            cardVisualUI.Init(index, spriteSet);
        }
    }
}

