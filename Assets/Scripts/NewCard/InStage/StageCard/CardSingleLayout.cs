using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class CardSingleLayout : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        [SerializeField] private SpriteRenderer mainSprite;
        [SerializeField] private SpriteRenderer cornerSprite;

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            mainSprite.sprite = data.MainSprite;
            cornerSprite.sprite = data.CornerSprite;
        }

        public void SetBackground(SpriteRenderer sharedBackgroundRenderer)
        {
        }

        public void SetSortingOrder(int sortingOrder)
        {
            mainSprite.sortingOrder = 100 * sortingOrder + 50;
            cornerSprite.sortingOrder = 100 * sortingOrder + 50;
        }
    }
}