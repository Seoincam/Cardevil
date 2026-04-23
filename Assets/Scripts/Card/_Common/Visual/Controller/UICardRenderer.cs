using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.Visual.Controller
{
    [RequireComponent(typeof(Image))]
    public class UICardRenderer : MonoBehaviour, ICardRenderer
    {
        private Image _image;
        public Image Image
        {
            get
            {
                if (!_image) _image = GetComponent<Image>();
                return _image;
            }
        }

        public Sprite Sprite
        {
            get => Image.sprite;
            set => Image.sprite = value;
        }

        public Color Color
        {
            get => Image.color;
            set => Image.color = value;
        }

        public void SetSortingOrder(int baseSortingOrder, int offset, int layerId)
        {
            // UI primarily uses hierarchy order. We can implement Canvas override if needed later.
        }

        public void SetSharedBackground(ICardRenderer sharedBackgroundRenderer)
        {
            if (sharedBackgroundRenderer != null && sharedBackgroundRenderer.Sprite != null)
            {
                // UI Images can't use MaterialPropertyBlock with Textures in the same way out of the box, 
                // depending on the UI shader. If we have a custom UI shader, we'd instance the Material instead.
                // For a fallback, we just copy the sprite if it represents the background entirely.
                Sprite = sharedBackgroundRenderer.Sprite;
            }
        }

        public Tween DOFade(float targetAlpha, float duration)
        {
            return Image.DOFade(targetAlpha, duration);
        }
    }
}
