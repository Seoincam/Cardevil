using Cardevil.Cards.CardInteractinos;
using Cardevil.Utils.Directions;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards
{
    [System.Serializable]
    public abstract class CardData
    {
        public int reinforcement = 0;
        public virtual bool CanSelect => false;
        public virtual bool valueSelected => false;

        public abstract bool OpenSelection(SelectContainer selectContainer, Card card);
        public abstract CardData CreateInGame();
    }


    [System.Serializable]
    public class NumberCardData : CardData
    {
        public override bool CanSelect => selectableValues != null && selectableValues.Length > 0;
        public override bool valueSelected => value != 0;


        [Space]
        public CardColor color;
        public int value;
        public int[] selectableValues;

        private NumberCardData(CardColor color, int value, int reinforcement, int[] selectableValues)
        {
            this.color = color;
            this.value = value;
            this.reinforcement = reinforcement;
            this.selectableValues = selectableValues;
        }

        public bool SetValue(int value)
        {
            if (!selectableValues.Contains(value))
                return false;

            this.value = value;
            return true;
        }

        public override bool OpenSelection(SelectContainer selectContainer, Card card)
        {
            if (!CanSelect)
                return false;
            selectContainer.SetContainer(card, selectableValues);
            return true;
        }


        public override CardData CreateInGame()
        {
            var data = new NumberCardData(color, value, reinforcement, selectableValues);
            return data;
        }
    }


    [System.Serializable]
    public class DirectionCardData : CardData
    {
        public override bool CanSelect => selectableValues != null && selectableValues.Length > 0;
        public override bool valueSelected => value.direction != Direction.None;

        [Space]
        public CardDirection value;
        public Direction[] selectableValues;

        private DirectionCardData(CardDirection value, int reinforcement, Direction[] selectableValues)
        {
            this.value = new CardDirection(value.direction, value.length);
            this.reinforcement = reinforcement;
            this.selectableValues = selectableValues;
        }

        public bool SelectValue(Direction value)
        {
            if (!selectableValues.Contains(value))
                return false;

            this.value.direction = value;
            return true;
        }

        public override bool OpenSelection(SelectContainer selectContainer, Card card)
        {
            if (!CanSelect)
                return false;
            selectContainer.SetContainer(card, selectableValues);
            return true;
        }

        public override CardData CreateInGame()
        {
            var cardDirection = new CardDirection(value.direction, value.length);
            var data = new DirectionCardData(cardDirection, reinforcement, selectableValues);
            return data;
        }
    }


    public enum CardColor { Red, Blue, Green, Black }

    public struct CardDirection
    {
        public Direction direction;
        public int length;

        public CardDirection(Direction direction, int length)
        {
            this.direction = direction;
            this.length = length;
        }
    }

    public enum HandRanking
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
}
