using UnityEngine;

namespace Cardevil.NewCard.Common.Visual
{
    public interface ICardLayout
    {
        GameObject GameObject { get; }
        void Apply(in CardVisualData data);
    }

    public interface ICardLayoutSpriteRenderer : ICardLayout
    {
        void SetSortingOrder(int sortingOrder);
    }
}
