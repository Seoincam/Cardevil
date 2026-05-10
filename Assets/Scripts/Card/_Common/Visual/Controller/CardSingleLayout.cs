using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardSingleLayout : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        private static readonly int SaturationAmountID = Shader.PropertyToID("Amount");
        
        [SerializeField] private SpriteRenderer mainSprite;
        [SerializeField] private SpriteRenderer cornerSprite;

        private MaterialPropertyBlock _propBlock;

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            mainSprite.sprite = data.MainSprite;
            cornerSprite.sprite = data.CornerSprite;
        }

        public void SetNoneColorMaterial(bool value)
        {
            _propBlock ??= new MaterialPropertyBlock();
            
            mainSprite.GetPropertyBlock(_propBlock);

            if (value)
            {
                _propBlock.SetFloat(SaturationAmountID, 0);
            }
            else
            {
                _propBlock.SetFloat(SaturationAmountID, 1);
            }
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

        public void SetAlpha(float targetAlpha)
        {
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, targetAlpha);
            cornerSprite.color = new Color(cornerSprite.color.r, cornerSprite.color.g, cornerSprite.color.b, targetAlpha);
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