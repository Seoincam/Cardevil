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
        private void InitBaseDeckConfig()
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
                    number = new NumberData() { number = num, color = color };
                    move = null;

                    card = new(number, move, CardData.ValueType.Number);
                    Deck.Add(card);
                }

                number = new NumberData() { number = 0, color = color };
                move = null;
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
                        number = null;
                        move = null;
                        card = new(number, move, CardData.ValueType.Move, CardData.SelectType.All);
                        Deck.Add(card);
                    }
                    else
                    {
                        number = null;
                        move = new() { direction = direction, length = 1 };
                        card = new(number, move, CardData.ValueType.Move);
                        Deck.Add(card);
                    }
                }
            }
        }
    }
}
