namespace Cardevil.Cards.InStage.View
{
    public readonly struct StageCardsViewState
    {
        public readonly bool CanUse;
        public readonly bool CanDiscard;
        public readonly bool CanSort;

        public StageCardsViewState(bool canUse, bool canDiscard, bool canSort)
        {
            CanUse = canUse;
            CanDiscard = canDiscard;
            CanSort = canSort;
        }
    }
}