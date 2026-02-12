using UnityEngine;

namespace Cardevil.NewCard.Common.Visual
{
    public interface ICardLayout
    {
        GameObject GameObject { get; }
        void Apply(in CardLayoutData data);
    }

    public interface ICardLayoutSpriteRenderer : ICardLayout
    {
        void SetBackground(SpriteRenderer sharedBackgroundRenderer);
        void SetSortingOrder(int sortingOrder);
    }
}
