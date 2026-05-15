using Cardevil.Card.Common.Core;
using Cardevil.Core.Utils;
using System.Collections.Generic;

namespace Cardevil.Card.InStage
{
    /// <summary>
    /// ICardState 정렬 기준 모음.
    /// </summary>
    public static class CardStateComparers
    {
        public static readonly IComparer<ICardState> ByNumber = new NumberComparer();
        public static readonly IComparer<ICardState> ByIcon = new IconComparer();
        
        private sealed class NumberComparer : IComparer<ICardState>
        {
            public int Compare(ICardState a, ICardState b)
            {
                int cmp = TypeOrder(a).CompareTo(TypeOrder(b));
                if (cmp != 0) return cmp;

                cmp = SelectableCount(a).CompareTo(SelectableCount(b));
                if (cmp != 0) return cmp;

                cmp = DirectionOrder(a).CompareTo(DirectionOrder(b));
                if (cmp != 0) return cmp;

                return NumberOrder(a).CompareTo(NumberOrder(b));
            }
        }

        private sealed class IconComparer : IComparer<ICardState>
        {
            public int Compare(ICardState a, ICardState b)
            {
                int cmp = TypeOrder(a).CompareTo(TypeOrder(b));
                if (cmp != 0) return cmp;

                cmp = SelectableCount(a).CompareTo(SelectableCount(b));
                if (cmp != 0) return cmp;

                cmp = DirectionOrder(a).CompareTo(DirectionOrder(b));
                if (cmp != 0) return cmp;

                cmp = ColorOrder(a).CompareTo(ColorOrder(b));
                if (cmp != 0) return cmp;

                return NumberOrder(a).CompareTo(NumberOrder(b));
            }
        }
        
        private static int TypeOrder(ICardState s) => s.IsMove ? 0 : 1;

        private static int SelectableCount(ICardState s)
        {
            if (s.ValueSelected) return 0;

            return s.SelectableType switch
            {
                CardState.ValueSelectableType.Color     => s.Colors.AllOptionsCount,
                CardState.ValueSelectableType.Number    => s.Numbers.AllOptionsCount,
                CardState.ValueSelectableType.Direction => s.Directions.AllOptionsCount,
                _ => 0
            };
        }

        private static int DirectionOrder(ICardState s)
        {
            if (!s.IsMove) return 0;
            
            var current = s.Directions.Current;
            if (!current.HasValue) return int.MaxValue;

            return current.Value switch
            {
                Direction.Up => 0,
                Direction.Down => 1,
                Direction.Left => 2,
                Direction.Right => 3,
                
                _ => 4
            };
        }
        
        private static int ColorOrder(ICardState s)
        {
            if (!s.IsAttack) return 0;
            var cur = s.Colors.Current;
            return cur.HasValue ? (int)cur.Value : int.MaxValue;
        }

        private static int NumberOrder(ICardState state)
        {
            if (!state.IsAttack) return 0;

            var current = state.Numbers.Current;
            return current ?? int.MaxValue;
        }
    }
}