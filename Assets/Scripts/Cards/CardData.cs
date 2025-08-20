using Cardevil.Utils.Directions;

namespace Cardevil.Cards
{
    public enum CardType { Move, Number }
    public enum CardColor { None, Red, Blue, Green, Black }

    [System.Serializable]
    public struct CardData
    {
        public CardType type;

        public int reinforce;

        // Move type
        public Direction direction;

        // Number type
        public CardColor color;
        public int value; // 2~10, *은 11으로 표기

        // 생성자 (Number)
        public CardData(CardColor color, int value, int reinforce)
        {
            type = CardType.Number;
            this.color = color;
            this.value = value;
            this.reinforce = reinforce;

            direction = Direction.None;
        }

        // 생성자 (Move)
        public CardData(Direction direction, int reinforce)
        {
            type = CardType.Move;
            this.direction = direction;
            this.reinforce = reinforce;

            color = CardColor.None;
            value = 0;
        }
    }

    public enum CardCombo
    {
        None = -1,

        High = 0,
        OnePair = 5,
        TwoPair = 20,
        Triple = 30,
        Straight = 50,
        Flush = 80,
        FourCard = 200,
        StraightFlush = 300  // 스티플
    }

    public struct CardResult
    {
        public CardCombo combo;
        public int damage;

        public Direction[] moves;


        public CardResult(CardCombo combo, int damage, Direction[] moves)
        {
            this.combo = combo;
            this.damage = damage;
            this.moves = moves;
        }

        public CardResult(Direction[] moves)
        {
            combo = CardCombo.None;
            damage = 0;
            this.moves = moves;
        }
    }
}
