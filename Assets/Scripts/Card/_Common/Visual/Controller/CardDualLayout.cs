using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardDualLayout : MonoBehaviour, ICardLayoutGraphic
    {
        [SerializeField] private GameObject backgroundObj;
        private ICardRenderer _background;
        private ICardRenderer Background => _background ??= backgroundObj?.GetComponent<ICardRenderer>();
        
        [SerializeField] private GameObject subSprite0Obj;
        private ICardRenderer _subSprite0;
        private ICardRenderer SubSprite0 => _subSprite0 ??= subSprite0Obj?.GetComponent<ICardRenderer>();

        [SerializeField] private GameObject subSprite1Obj;
        private ICardRenderer _subSprite1;
        private ICardRenderer SubSprite1 => _subSprite1 ??= subSprite1Obj?.GetComponent<ICardRenderer>();

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            if (SubSprite0 != null) SubSprite0.Sprite = data.SubSprites[0];
            if (SubSprite1 != null) SubSprite1.Sprite = data.SubSprites[1];
        }

        public void SetBackground(ICardRenderer sharedBackgroundRenderer)
        {
            Background?.SetSharedBackground(sharedBackgroundRenderer);
        }

        public void SetSortingOrder(int sortingOrder, int layerId)
        {
            Background?.SetSortingOrder(sortingOrder, 1, layerId);
            SubSprite0?.SetSortingOrder(sortingOrder, 50, layerId);
            SubSprite1?.SetSortingOrder(sortingOrder, 50, layerId);
        }

        public Tween SetAlpha(float targetAlpha, float duration, Ease ease)
        {
            var sq = DOTween.Sequence();
            if (SubSprite0 != null) sq.Join(SubSprite0.DOFade(targetAlpha, duration).SetEase(ease));
            if (SubSprite1 != null) sq.Join(SubSprite1.DOFade(targetAlpha, duration).SetEase(ease));
            return sq;
        }
    }
}