namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init();
        CardContext Context { get; }
        InStageCards StageCards { get; }
    }
}