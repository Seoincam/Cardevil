using UnityEngine;

namespace Cardevil.NewCard.Common.Visual
{
    public interface ICardLayout
    {
        GameObject GameObject { get; }
        void Apply(CardVisualData data);
    }
}
