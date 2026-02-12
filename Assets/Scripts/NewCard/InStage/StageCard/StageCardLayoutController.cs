using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class StageCardLayoutController : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        [Header("Prefabs")]
        [SerializeField] private StageCardSingleLayout singlePrefab;
        [SerializeField] private StageCardDualLayout dualPrefab;
        [SerializeField] private StageCardTripleLayout triplePrefab;

        [Header("References")] 
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SpriteRenderer innerFrame;

        [Header("States")]
        [SerializeReference] private ICardLayoutSpriteRenderer currentLayout;

        public GameObject GameObject => gameObject;

        public void Apply(in CardVisualData data)
        {
            if (currentLayout?.GameObject)
            {
                Destroy(currentLayout.GameObject);
            }

            innerFrame.sprite = data.InnerFrame;
            
            ICardLayoutSpriteRenderer layout = data.Type switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<StageCardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<StageCardTripleLayout>(),
                _ => throw new System.NotImplementedException()
            };

            layout.Apply(data);
            currentLayout = layout;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            innerFrame.sortingOrder = 100 * sortingOrder + 10;
            background.sortingOrder = 100 * sortingOrder + 0;
            
            currentLayout.SetSortingOrder(sortingOrder);
        }
    }
}
