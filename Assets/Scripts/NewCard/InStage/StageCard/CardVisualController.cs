using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class CardVisualController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CardSingleLayout singlePrefab;
        [SerializeField] private CardDualLayout dualPrefab;
        [SerializeField] private CardTripleLayout triplePrefab;

        [Space] 
        [SerializeField] private ColorJewelDecoration colorJewel;

        [Header("References")] 
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SpriteRenderer innerFrame;

        private ICardLayoutSpriteRenderer _currentLayout;
        private ColorJewelDecoration _currentColorJewel;

        public void Apply(ICardState state)
        {
            var visualInput = CardVisualInput.From(state);
            Apply(visualInput);
        }

        public void Apply(CardVisualInput visualInput)
        {
            // Layout
            var layoutData = CardLayoutResolver.Resolve(visualInput);

            if (_currentLayout?.GameObject)
            {
                Destroy(_currentLayout.GameObject);
            }

            innerFrame.sprite = layoutData.InnerFrame;

            _currentLayout = layoutData.LayoutType switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<CardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform)
                    .GetComponent<CardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<CardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<CardTripleLayout>(),
                _ => throw new System.NotImplementedException()
            };

            _currentLayout.SetBackground(background);
            _currentLayout.Apply(in layoutData);

            // Decoration
            if (_currentColorJewel)
            {
                Destroy(_currentColorJewel);
            }
            
            var decorationData = CardDecorationResolver.Resolve(visualInput);
            if (decorationData.Decorations.HasFlag(CardDecorations.ColorJewel))
            {
                _currentColorJewel = Instantiate(colorJewel, transform).GetComponent<ColorJewelDecoration>();
                _currentColorJewel.Apply(in decorationData);
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            innerFrame.sortingOrder = 100 * sortingOrder + 10;
            background.sortingOrder = 100 * sortingOrder + 0;
            
            _currentLayout?.SetSortingOrder(sortingOrder);
            _currentColorJewel?.SetSortingOrder(sortingOrder);
        }
    }
}
