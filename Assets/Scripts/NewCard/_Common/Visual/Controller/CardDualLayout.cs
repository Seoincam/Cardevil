using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.Visual.Controller
{
    public class CardDualLayout : MonoBehaviour, ICardLayoutSpriteRenderer
    {
        [SerializeField] private SpriteRenderer background;
        
        [SerializeField] private SpriteRenderer subSprite0;
        [SerializeField] private SpriteRenderer subSprite1;
        
        private static readonly int TextureId = Shader.PropertyToID("_BackgroundTex");

        public GameObject GameObject => gameObject;

        public void Apply(in CardLayoutData data)
        {
            subSprite0.sprite = data.SubSprites[0];
            subSprite1.sprite = data.SubSprites[1];
        }

        public void SetBackground(SpriteRenderer sharedBackgroundRenderer)
        {
            var backgroundPropertyBlock = new MaterialPropertyBlock();   
            backgroundPropertyBlock.SetTexture(TextureId, sharedBackgroundRenderer.sprite.texture);
            
            background.SetPropertyBlock(backgroundPropertyBlock);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            background.sortingOrder = 100 * sortingOrder + 1;
            
            subSprite0.sortingOrder = 100 * sortingOrder + 50;
            subSprite1.sortingOrder = 100 * sortingOrder + 50;
        }
    }
}