using Cardevil.NewCard.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.NewCard.Visual.Controller
{
    public interface ICardLayout
    {
        GameObject GameObject { get; }
        void Apply(in CardLayoutData data);
    }

    public interface ICardLayoutSpriteRenderer : ICardLayout
    {
        void SetBackground(SpriteRenderer sharedBackgroundRenderer);
        void SetSortingOrder(int sortingOrder, int layerId);
        Tween SetAlpha(float targetAlpha, float duration, Ease ease);
    }
}
