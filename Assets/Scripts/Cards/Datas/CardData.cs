namespace Cardevil.Cards.Data
{
    public enum CardColor
    {
        None = -1,
        Red = 0,
        Green = 1,
        Blue = 2,
        Black = 3
    }

    // 카드의 “기본 데이터 원본”을 보관하는 클래스.
    // 강화 등이 적용됨.
    // 전 스테이지에 걸쳐 사용됨.
    public class CardData
    {
        public int Id { get; }
        public NumberModifierPipeline NumberModifiers { get; }
        public MoveModifierPipeline MoveModifiers { get; }

        public CardData(int id, NumberModifierPipeline numberModifiers)
        {
            Id = id;
            NumberModifiers = numberModifiers;
        }

        public CardData(int id, MoveModifierPipeline moveModifiers)
        {
            Id = id;
            MoveModifiers = moveModifiers;
        }
    }

    public interface ILockable
    {
        void Lock();
    }
}
