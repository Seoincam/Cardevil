using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class StateCardVisualController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private StageCardSingleLayout singlePrefab;
        [SerializeField] private StageCardDualLayout dualPrefab;
        [SerializeField] private StageCardTripleLayout triplePrefab;

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

            // Layout
            var layoutData = CardLayoutResolver.Resolve(visualInput);

            if (_currentLayout?.GameObject)
            {
                Destroy(_currentLayout.GameObject);
            }

            innerFrame.sprite = layoutData.InnerFrame;

            _currentLayout = layoutData.LayoutType switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform)
                    .GetComponent<StageCardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<StageCardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<StageCardTripleLayout>(),
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
