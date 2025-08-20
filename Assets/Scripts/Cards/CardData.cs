using Cardevil.Utils.Directions;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards
{
    public enum CardColor { Red, Blue, Green, Black }

    public class CardData
    {
        public int reinforcement = 0;
        public bool canSelect;
        public int canSelectCount;
    }


    [System.Serializable]
    public class NumberCardData : CardData
    {
        public CardColor Color { get; private set; }
        public int Value { get; private set; }
        public HashSet<int> numbers;

        public NumberCardData(CardColor color, int value, bool canSelect = false)
        {
            Color = color;
            Value = value;
            numbers = new();
            this.canSelect = canSelect;
        }

        public void AddSelect(int[] values)
        {
            foreach (int value in values)
                numbers.Add(value);

            canSelectCount = numbers.Count();
        }

        public bool SelectValue(int value)
        {
            if (!canSelect)
                return false;
            if (numbers.Count() == 1)
                return false;
            if (!numbers.Contains(value))
                return false;

            Value = value;
            return true;
        }
    }


    [System.Serializable]
    public class DirectionCardData : CardData
    {
        public Direction Value { get; private set; }
        public HashSet<Direction> directinos;

        public DirectionCardData(Direction value, bool canSelect = false)
        {
            Value = value;
            directinos = new();
            this.canSelect = canSelect;
        }

        public void AddSelect(Direction[] values)
        {
            foreach (var value in values)
                directinos.Add(value);

            canSelectCount = directinos.Count();
        }

        public bool SelectValue(Direction value)
        {
            if (!canSelect)
                return false;
            if (directinos.Count() == 1)
                return false;
            if (!directinos.Contains(value))
                return false;

            Value = value;
            return true;
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
