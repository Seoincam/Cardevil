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

            // 숫자 카드
            foreach (NumberData.CardColor color in Enum.GetValues(typeof(NumberData.CardColor)))
            {
                for (int num = 2; num <= 10; num++)
                {
                    card = new()
                    {
                        type = CardData.CardType.Number,
                        Number = new NumberData() { color = color, number = num },
                        Move = new MoveData() { direction = Direction.None, length = 0 },
                        isLocked = false
                    };
                    Deck.Add(card);
                }

                var selectable = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                card = new()
                {
                    type = CardData.CardType.Number,
                    Number = new NumberData() { color = color },
                    numberOptions = new(selectable),
                    Move = new MoveData() { direction = Direction.None, length = 0 },
                    isLocked = false
                };
                Deck.Add(card);
            }

            // 방향 카드
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (direction == Direction.None)
                    {

                        var selectable = new List<Direction>() { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                        card = new()
                        {
                            type = CardData.CardType.Move,
                            Move = new MoveData() { direction = Direction.None, length = 1 },
                            directionOptions = new(selectable),
                            Number = new NumberData() { number = 0 },
                            isLocked = false
                        };
                        Deck.Add(card);
                    }
                    else
                    {
                        card = new()
                        {
                            type = CardData.CardType.Move,
                            Move = new MoveData() { direction = direction, length = 1 },
                            Number = new NumberData() { number = 0 },
                            isLocked = false
                        };
                        Deck.Add(card);
                    }
                }
            }
        }
    }
}
