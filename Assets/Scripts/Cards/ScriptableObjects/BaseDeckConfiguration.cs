using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards
{
    [CreateAssetMenu(menuName = "Cards/BaseDeckConfiguration")]
    public class BaseDeckConfiguration : ScriptableObject
    {
        [SerializeField] NumberDamageReinforceConfiguration numberDamageReinforceConfig;
        [SerializeField] NumberSelectReinforceConfiguration numberSelectReinforceConfig;
        [SerializeField] DirectionSelectReinforceConfiguration directionSelectReinforceConfig;

        public List<CardData> Deck;

        [ContextMenu("Initialize Deck")]
        public void InitBaseDeckConfig()
        {
            Deck.Clear();
            CardData card;
            NumberData number;
            MoveData move;

            // 숫자 카드
            foreach (NumberData.CardColor color in Enum.GetValues(typeof(NumberData.CardColor)))
            {
                if (color == NumberData.CardColor.None)
                    continue;
                    
                for (int num = 2; num <= 10; num++)
                {
                    number = new(color, num);
                    move = new();

                    card = new(number, move, CardData.ValueType.Number);
                    Deck.Add(card);
                }

                number = new(color, 0);
                move = new();
                card = new(number, move, CardData.ValueType.Number, CardData.SelectType.All);
                Deck.Add(card);
            }

            // 방향 카드
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (direction == Direction.None)
                    {
                        number = new();
                        move = new();
                        card = new(number, move, CardData.ValueType.Move, CardData.SelectType.All);
                        Deck.Add(card);
                    }
                    else
                    {
                        number = new();
                        move = new(direction, 1);
                        card = new(number, move, CardData.ValueType.Move);
                        Deck.Add(card);
                    }
                }
            }
        }
    }
}
