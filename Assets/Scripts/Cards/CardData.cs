namespace Cardevil.Cards
{
    public enum CardType { Move, Number }
    public enum CardDirection { None, Up, Down, Left, Right, All }
    public enum CardColor { None, Red, Blue, Green, Black }

    [System.Serializable]
    public struct CardData
    {
        public CardType Type;

        // Move type
        public CardDirection Direction;

        // Number type
        public CardColor Color;
        public int Value; // 1~9, *은 10으로 표기


        // 생성자 (Number)
        public CardData(CardColor color, int value)
        {
            Type = CardType.Number;
            Color = color;
            Value = value;

            Direction = CardDirection.None;
        }

        // 생성자 (Move)
        public CardData(CardDirection direction)
        {
            Type = CardType.Move;
            Direction = direction;

            Color = CardColor.None;
            Value = 0;
        }
    }

    public enum CardCombo
    {
        High,
        OnePair,
        TwoPair,
        Triple,
        Straight,
        Flush,
        StraightFlush,  // 스티플
        FourCard
    }
    
    public struct CardResult
    {
        public CardCombo Combo;
        public int BaseDamage;    // 카드들의 합계
        public int ComboDamage;    // 콤보의 추가 점수
        public int TotalDamage;
        
        public CardDirection[] directions;
    }
}
