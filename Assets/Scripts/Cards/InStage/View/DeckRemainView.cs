using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.DebugConsole;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Cards.InStage.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DeckRemainView : UI_Popup
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CardVisualUI[] cardVisuals;

        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;

        private bool _isVisible;
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
        
        public async UniTaskVoid SetVisible(bool isVisible)
        {
            if (_isVisible == isVisible) return;
            _isVisible = isVisible;
            
            var dur = .5f;
            var targetAlpha = isVisible ? 1 : 0;
            
            // 이전 트윈 정리
            _tween?.Kill();

            canvasGroup.interactable = false;
            
            _tween = canvasGroup.DOFade(targetAlpha, dur)
                .SetLink(gameObject)
                .SetRecyclable(true);
            
            _tween.OnComplete(() =>
            {
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            });

            await _tween;
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

        [ContextMenu("Toggle Visible")]
        private void ToggleVisible()
        {
            SetVisible(!_isVisible).Forget();
        }
    }
}

