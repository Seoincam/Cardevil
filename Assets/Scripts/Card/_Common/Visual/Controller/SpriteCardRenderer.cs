using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteCardRenderer : MonoBehaviour, ICardRenderer
    {
        private SpriteRenderer _spriteRenderer;
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (!_spriteRenderer) _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }

        public Sprite Sprite
        {
            get => SpriteRenderer.sprite;
            set => SpriteRenderer.sprite = value;
        }

        public Color Color
        {
            get => SpriteRenderer.color;
            set => SpriteRenderer.color = value;
        }

        public void SetSortingOrder(int baseSortingOrder, int offset, int layerId)
        {
            SpriteRenderer.sortingLayerID = layerId;
            SpriteRenderer.sortingOrder = 100 * baseSortingOrder + offset;
        }

        private static readonly int TextureId = Shader.PropertyToID("_BackgroundTex");

        public void SetSharedBackground(ICardRenderer sharedBackgroundRenderer)
        {
            if (sharedBackgroundRenderer != null && sharedBackgroundRenderer.Sprite != null)
            {
                var backgroundPropertyBlock = new MaterialPropertyBlock();   
                // Note: material property blocks take textures
                backgroundPropertyBlock.SetTexture(TextureId, sharedBackgroundRenderer.Sprite.texture);
                SpriteRenderer.SetPropertyBlock(backgroundPropertyBlock);
            }
        }

        public Tween DOFade(float targetAlpha, float duration)
        {
            return SpriteRenderer.DOFade(targetAlpha, duration);
        }
    }
}
