using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public interface ICardRenderer
    {
        Sprite Sprite { get; set; }
        Color Color { get; set; }
        GameObject gameObject { get; }

        void SetSortingOrder(int baseSortingOrder, int offset, int layerId);
        void SetSharedBackground(ICardRenderer sharedBackgroundRenderer);
        Tween DOFade(float targetAlpha, float duration);
    }
}
