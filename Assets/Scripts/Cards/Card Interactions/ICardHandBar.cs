namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init();
        CardContext Context { get; }
        InGameDeck Deck { get; }
        InGameHand Hand { get; }
    }
}