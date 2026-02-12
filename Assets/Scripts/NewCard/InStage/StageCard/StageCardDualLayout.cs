using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class StageCardDualLayout : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        [SerializeField] private SpriteRenderer background;
        
        [SerializeField] private SpriteRenderer subSprite0;
        [SerializeField] private SpriteRenderer subSprite1;

        public GameObject GameObject => gameObject;

        public void Apply(in CardVisualData data)
        {
            subSprite0.sprite = data.SubSprites[0];
            subSprite1.sprite = data.SubSprites[1];
        }

        public void SetSortingOrder(int sortingOrder)
        {
            background.sortingOrder = 100 * sortingOrder + 1;
            
            subSprite0.sortingOrder = 100 * sortingOrder + 50;
            subSprite1.sortingOrder = 100 * sortingOrder + 50;
        }
    }
}