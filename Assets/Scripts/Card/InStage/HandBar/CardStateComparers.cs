using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Core.Utils;
using System.Collections.Generic;

namespace Cardevil.Card.InStage
{
    /// <summary>
    /// ICardState 정렬 기준 모음.
    /// </summary>
    public static class CardStateComparers
    {
        public static readonly IComparer<INewCardState> ByNumber = new NumberComparer();
        public static readonly IComparer<INewCardState> ByIcon = new IconComparer();
        
        private sealed class NumberComparer : IComparer<INewCardState>
        {
            public int Compare(INewCardState a, INewCardState b)
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

        private sealed class IconComparer : IComparer<INewCardState>
        {
            public int Compare(INewCardState a, INewCardState b)
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
        
        private static int TypeOrder(INewCardState s) => s.IsMove ? 0 : 1;

        private static int SelectableCount(INewCardState s)
        {
            if (s.ValueSelected) return 0;

            return s.UpgradePath switch
            {
                UpgradePath.MultiColor => s.ColorList.AllCandidateValues.Count,
                UpgradePath.MultiNumber => s.NumberList.AllCandidateValues.Count,
                UpgradePath.MultiDirection => s.DirectionList.AllCandidateValues.Count,
                _ => 0
            };
        }

        private static int DirectionOrder(INewCardState s)
        {
            if (!s.IsMove) return 0;

            var current = s.DirectionList.IsFixed ? s.DirectionList.FixedValue : Direction.None;

            return current switch
            {
                Direction.Up => 0,
                Direction.Down => 1,
                Direction.Left => 2,
                Direction.Right => 3,
                
                _ => 4
            };
        }
        
        private static int ColorOrder(INewCardState s)
        {
            if (!s.IsAttack) return 0;

            if (s.ColorList == null)
                return 0;
            
            return s.ColorList.IsFixed ? (int)s.ColorList.FixedValue : int.MaxValue;
        }

        private static int NumberOrder(INewCardState state)
        {
            if (!state.IsAttack) return 0;

            return state.NumberList.IsFixed ? state.NumberList.FixedValue : int.MaxValue;
        }
    }
}