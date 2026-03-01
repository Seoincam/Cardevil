using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
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

        public void SetSortingOrder(int sortingOrder, int layerId)
        {
            mainSprite.sortingLayerID = layerId;
            mainSprite.sortingOrder = 100 * sortingOrder + 50;
            
            cornerSprite.sortingLayerID = layerId;
            cornerSprite.sortingOrder = 100 * sortingOrder + 50;
        }

        public Tween SetAlpha(float targetAlpha, float duration, Ease ease)
        {
            var mainSpriteTween = mainSprite
                .DOFade(targetAlpha, duration)
                .SetEase(ease);
            
            var cornerSpriteTween = cornerSprite
                .DOFade(targetAlpha, duration)
                .SetEase(ease);

            return DOTween.Sequence()
                .Join(mainSpriteTween)
                .Join(cornerSpriteTween);
        }
    }
}