using Cardevil.Utils.Directions;
using System.Linq;

namespace Cardevil.Cards
{
    public enum CardColor { Red, Blue, Green, Black }

    public class CardData
    {
        public int reinforcement = 0;
        public bool canSelect;
    }

    [System.Serializable]
    public class NumberCard : CardData
    {
        public CardColor Color { get; private set; }
        public int DefaultValue { get; private set; }
        public int[] Numbers { get; private set; }

        public NumberCard(CardColor color, int defaultValue, bool canSelect = false)
        {
            Color = color;
            DefaultValue = defaultValue;
            Numbers = new int[] { defaultValue };
            this.canSelect = canSelect;
        }
    }

    [System.Serializable]
    public class DirectionCard : CardData
    {
        public Direction DefaultValue { get; private set; }
        public Direction[] Directions { get; private set; }

        public DirectionCard(Direction defaultValue, bool canSelect = false)
        {
            DefaultValue = defaultValue;
            Directions = new Direction[] { defaultValue };
            this.canSelect = canSelect;
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
