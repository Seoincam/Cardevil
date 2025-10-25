using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Cards.InStage.View
{
    public class DeckRemainView : UI_Popup
    {
        [SerializeField] private CardVisualUI[] cardVisuals;

        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;

        public void Init(IReadOnlyCardLibrary library, IReadOnlyStageCardsModel model)
        {
            _library = library;
            _model = model;

            if (cardVisuals == null || cardVisuals.Length != 50)
            {
                LogEx.LogError("CardVisualUI가 제대로 할당되지 않음!");
                return;
            }

            for (int i = 0; i < cardVisuals.Length; i++)
                UpdateUI(i);
        }

        private void UpdateUI(int index)
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

