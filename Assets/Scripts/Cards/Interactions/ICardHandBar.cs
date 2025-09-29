using Cardevil.Cards.Evaluations;
using Cardevil.Core;

namespace Cardevil.Cards
{
    public interface ICardHandBar
    {
        void Init(CardManager manager, StageCardsContext ctx);
    }
}