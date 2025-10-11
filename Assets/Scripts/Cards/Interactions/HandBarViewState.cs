namespace Cardevil.Cards.Interactions
{
    public readonly struct HandBarViewState
    {
        public readonly bool CanUse;
        public readonly bool CanDiscard;
        public readonly bool CanSort;
        public readonly int RemainingCards;
        public readonly int RemainingDiscards;

        public HandBarViewState(bool canUse, bool canDiscard, bool canSort, int remainingCards, int remainingDiscards)
        {
            CanUse = canUse;
            CanDiscard = canDiscard;
            CanSort = canSort;
            RemainingCards = remainingCards;
            RemainingDiscards = remainingDiscards;
        }
    }
}