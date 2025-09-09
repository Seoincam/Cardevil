namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init();
        CardResultContext Context { get; }
        StageCardsContext StageCardsCtx { get; }
    }
}