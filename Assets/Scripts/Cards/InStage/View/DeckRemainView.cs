using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DeckRemainView : UI_Popup
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSO;

        [Header("UI")] 
        [SerializeField] private Button closeButton; 
        [SerializeField] private CardVisualUI[] cardVisuals;
        
        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;
        
        private CanvasGroup _canvasGroup;
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

        public void Open()
        {
            _isVisible = true;
            gameObject.SetActive(true);
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
    }
}

