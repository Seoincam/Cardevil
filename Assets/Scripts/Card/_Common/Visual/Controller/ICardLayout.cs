using Cardevil.Card.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public interface ICardLayout
    {
        GameObject GameObject { get; }
        void Apply(in CardLayoutData data);
    }

    public interface ICardLayoutGraphic : ICardLayout
    {
        void SetBackground(ICardRenderer sharedBackgroundRenderer);
        void SetSortingOrder(int sortingOrder, int layerId);
        Tween SetAlpha(float targetAlpha, float duration, Ease ease);
    }
}
