using Cardevil.Cards.Evaluations;
using Cardevil.Core;

namespace Cardevil.Cards
{
    public interface ICardHandBar: IClearable
    {
        void Init();
        StageCardsContext StageCardsCtx { get; }
    }
}