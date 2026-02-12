using Cardevil.NewCard.Common.Core;
using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.NewCard.Common.Visual
{
    public interface ICardLayoutProvider
    {
        LayoutType Type { get; }
        
        IReadOnlyList<CardColor> Colors { get; }
        IReadOnlyList<int> Numbers { get; }
        IReadOnlyList<Direction> Directions { get; }
    }

    public enum LayoutType
    {
        Single,
        Dual,
        Triple
    }
}