namespace Cardevil.Cards
{
    public enum CardType { Move, Number }
    public enum CardDirection { None, Up, Down, Left, Right, All }
    public enum CardColor { None, Red, Blue, Green, Black }

    [System.Serializable]
    public struct CardData
    {
        public CardType type;

        public int reinforce;

        // Move type
        public CardDirection direction;

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

            direction = CardDirection.None;
        }

        // 생성자 (Move)
        public CardData(CardDirection direction, int reinforce)
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
        public int baseDamage;    // 카드들의 합계
        public int comboDamage;    // 콤보의 추가 점수
        public int totalDamage;

        public CardDirection[] moves;



        // 일반
        public CardResult(int baseDamage, CardCombo combo,  CardDirection[] moves)
        {
            this.combo = combo;
            this.baseDamage = baseDamage;

            comboDamage = combo == CardCombo.None ? 0 : (int)combo;
            totalDamage = baseDamage + comboDamage;

            this.moves = moves;
        }

        // 공격 x
        public CardResult(CardDirection[] moves)
        {
            combo = CardCombo.None;
            baseDamage = 0;
            comboDamage = 0;
            totalDamage = 0;

            this.moves = moves;
        }
    }
}
