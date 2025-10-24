using Cardevil.Cards.Data;
using Cardevil.Cards.InStage.Model.ReadOnly;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    public class DeckRemainView : UI_Popup
    {
        [SerializeField] private CardVisualUI[] cardVisuals;

        private IReadOnlyCardLibrary _library;
        private IReadOnlyStageCardsModel _model;

        public void Init(IReadOnlyStageCardsModel model)
        {
            _model = model;
        }
    }
}

