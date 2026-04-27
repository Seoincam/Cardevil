using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardTripleLayout : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        [SerializeField] private SpriteRenderer background0;
        [SerializeField] private SpriteRenderer background1;
        
        [Space]
        [SerializeField] private SpriteRenderer subSprite0;
        [SerializeField] private SpriteRenderer subSprite1;
        [SerializeField] private SpriteRenderer subSprite2;
        
        private static readonly int TextureId = Shader.PropertyToID("_BackgroundTex");

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            subSprite0.sprite = data.SubSprites[0];
            subSprite1.sprite = data.SubSprites[1];
            subSprite2.sprite = data.SubSprites[2];
        }

        public void SetBackground(SpriteRenderer sharedBackgroundRenderer)
        {
            var backgroundPropertyBlock = new MaterialPropertyBlock();   
            backgroundPropertyBlock.SetTexture(TextureId, sharedBackgroundRenderer.sprite.texture);
            
            background0.SetPropertyBlock(backgroundPropertyBlock);
            background1.SetPropertyBlock(backgroundPropertyBlock);
        }

        public void SetSortingOrder(int sortingOrder, int layerId)
        {
            background0.sortingLayerID = layerId;
            background0.sortingOrder = 100 * sortingOrder + 1;
            
            background1.sortingLayerID = layerId;
            background1.sortingOrder = 100 * sortingOrder + 2;
            
            subSprite0.sortingLayerID = layerId;
            subSprite0.sortingOrder = 100 * sortingOrder + 50;
            
            subSprite1.sortingLayerID = layerId;
            subSprite1.sortingOrder = 100 * sortingOrder + 50;
            
            subSprite2.sortingLayerID = layerId;
            subSprite2.sortingOrder = 100 * sortingOrder + 50;
        }

        public void SetAlpha(float targetAlpha)
        {
            subSprite0.color = new Color(subSprite0.color.r, subSprite0.color.g, subSprite0.color.b, targetAlpha);
            subSprite1.color = new Color(subSprite1.color.r, subSprite1.color.g, subSprite1.color.b, targetAlpha);
            subSprite2.color = new Color(subSprite2.color.r, subSprite2.color.g, subSprite2.color.b, targetAlpha);
        }

        public Tween SetAlpha(float targetAlpha, float duration, Ease ease)
        {
            var subSprite0Tween = subSprite0
                .DOFade(targetAlpha, duration)
                .SetEase(ease);
            
            var subSprite1Tween = subSprite1
                .DOFade(targetAlpha, duration)
                .SetEase(ease);
            
            var subSprite2Tween = subSprite2
                .DOFade(targetAlpha, duration)
                .SetEase(ease);
            
            return DOTween.Sequence()
                .Join(subSprite0Tween)
                .Join(subSprite1Tween)
                .Join(subSprite2Tween);
        }
    }
}