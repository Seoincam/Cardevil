using Cardevil.Cards.Data.Modifiers.Move;
using Cardevil.Cards.Data.Modifiers.Number;

namespace Cardevil.Cards.Data
{
    /// <summary>
    /// 카드의 기본 데이터를 정의하는 클래스.
    /// 각 카드는 고유한 Id와 함께 Number 또는 Move 타입의 Modifier 파이프라인을 가질 수 있음.
    /// </summary>
    public class CardPipeline
    {
        public int Id { get; }
        public NumberModifierPipeline NumberPipeline { get; }
        public MoveModifierPipeline MovePipeline { get; }

        public CardPipeline(int id, NumberModifierPipeline numberPipeline)
        {
            Id = id;
            NumberPipeline = numberPipeline;
        }

        public CardPipeline(int id, MoveModifierPipeline movePipeline)
        {
            Id = id;
            MovePipeline = movePipeline;
        }
    }
}
