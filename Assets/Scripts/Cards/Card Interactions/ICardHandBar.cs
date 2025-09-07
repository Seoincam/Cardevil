namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init();
        CardContext Context { get; }
        StageCardsContext StageCardsCtx { get; }
    }
}