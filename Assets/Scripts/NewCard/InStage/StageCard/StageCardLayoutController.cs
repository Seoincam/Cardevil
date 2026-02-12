using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class StageCardLayoutController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private StageCardSingleLayout singlePrefab;
        [SerializeField] private StageCardDualLayout dualPrefab;
        [SerializeField] private StageCardTripleLayout triplePrefab;

        [Header("References")] 
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SpriteRenderer innerFrame;

        private ICardLayoutSpriteRenderer _currentLayout;
        
        public void Apply(ICardState state)
        {
            var visualInput = CardVisualInput.From(state);
            var visualData = CardLayoutResolver.Resolve(visualInput);
            
            if (_currentLayout?.GameObject)
            {
                Destroy(_currentLayout.GameObject);
            }

            innerFrame.sprite = visualData.InnerFrame;
            
            _currentLayout = visualData.LayoutType switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<StageCardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<StageCardTripleLayout>(),
                _ => throw new System.NotImplementedException()
            };
            
            _currentLayout.SetBackground(background);
            _currentLayout.Apply(in visualData);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            innerFrame.sortingOrder = 100 * sortingOrder + 10;
            background.sortingOrder = 100 * sortingOrder + 0;
            
            _currentLayout.SetSortingOrder(sortingOrder);
        }
    }
}
