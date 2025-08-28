namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init();
        CardContext Context { get; }
        InStageDeck Deck { get; }
        InGameHand Hand { get; }
    }
}