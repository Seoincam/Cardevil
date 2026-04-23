using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardTripleLayout : MonoBehaviour, ICardLayoutGraphic
    {
        [SerializeField] private GameObject background0Obj;
        private ICardRenderer _background0;
        private ICardRenderer Background0 => _background0 ??= background0Obj?.GetComponent<ICardRenderer>();

        [SerializeField] private GameObject background1Obj;
        private ICardRenderer _background1;
        private ICardRenderer Background1 => _background1 ??= background1Obj?.GetComponent<ICardRenderer>();
        
        [Space]
        [SerializeField] private GameObject subSprite0Obj;
        private ICardRenderer _subSprite0;
        private ICardRenderer SubSprite0 => _subSprite0 ??= subSprite0Obj?.GetComponent<ICardRenderer>();

        [SerializeField] private GameObject subSprite1Obj;
        private ICardRenderer _subSprite1;
        private ICardRenderer SubSprite1 => _subSprite1 ??= subSprite1Obj?.GetComponent<ICardRenderer>();

        [SerializeField] private GameObject subSprite2Obj;
        private ICardRenderer _subSprite2;
        private ICardRenderer SubSprite2 => _subSprite2 ??= subSprite2Obj?.GetComponent<ICardRenderer>();

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            if (SubSprite0 != null) SubSprite0.Sprite = data.SubSprites[0];
            if (SubSprite1 != null) SubSprite1.Sprite = data.SubSprites[1];
            if (SubSprite2 != null) SubSprite2.Sprite = data.SubSprites[2];
        }

        public void SetBackground(ICardRenderer sharedBackgroundRenderer)
        {
            Background0?.SetSharedBackground(sharedBackgroundRenderer);
            Background1?.SetSharedBackground(sharedBackgroundRenderer);
        }

        public void SetSortingOrder(int sortingOrder, int layerId)
        {
            Background0?.SetSortingOrder(sortingOrder, 1, layerId);
            Background1?.SetSortingOrder(sortingOrder, 2, layerId);
            
            SubSprite0?.SetSortingOrder(sortingOrder, 50, layerId);
            SubSprite1?.SetSortingOrder(sortingOrder, 50, layerId);
            SubSprite2?.SetSortingOrder(sortingOrder, 50, layerId);
        }

        public Tween SetAlpha(float targetAlpha, float duration, Ease ease)
        {
            var sq = DOTween.Sequence();
            if (SubSprite0 != null) sq.Join(SubSprite0.DOFade(targetAlpha, duration).SetEase(ease));
            if (SubSprite1 != null) sq.Join(SubSprite1.DOFade(targetAlpha, duration).SetEase(ease));
            if (SubSprite2 != null) sq.Join(SubSprite2.DOFade(targetAlpha, duration).SetEase(ease));
            return sq;
        }
    }
}