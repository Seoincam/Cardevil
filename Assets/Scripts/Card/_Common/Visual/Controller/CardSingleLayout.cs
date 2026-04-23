using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardSingleLayout : MonoBehaviour, ICardLayoutGraphic
    {
        [SerializeField] private GameObject mainSpriteObj;
        private ICardRenderer _mainSprite;
        private ICardRenderer MainSprite => _mainSprite ??= mainSpriteObj?.GetComponent<ICardRenderer>();

        [SerializeField] private GameObject cornerSpriteObj;
        private ICardRenderer _cornerSprite;
        private ICardRenderer CornerSprite => _cornerSprite ??= cornerSpriteObj?.GetComponent<ICardRenderer>();

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            if (MainSprite != null) MainSprite.Sprite = data.MainSprite;
            if (CornerSprite != null) CornerSprite.Sprite = data.CornerSprite;
        }

        public void SetBackground(ICardRenderer sharedBackgroundRenderer)
        {
        }

        public void SetSortingOrder(int sortingOrder, int layerId)
        {
            MainSprite?.SetSortingOrder(sortingOrder, 50, layerId);
            CornerSprite?.SetSortingOrder(sortingOrder, 50, layerId);
        }

        public Tween SetAlpha(float targetAlpha, float duration, Ease ease)
        {
            var sq = DOTween.Sequence();
            if (MainSprite != null) sq.Join(MainSprite.DOFade(targetAlpha, duration).SetEase(ease));
            if (CornerSprite != null) sq.Join(CornerSprite.DOFade(targetAlpha, duration).SetEase(ease));
            return sq;
        }
    }
}