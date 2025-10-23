using Cardevil.Cards.ScriptableObjects;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using UnityEngine;

namespace Cardevil.Cards.Data.InStage
{
    public static class CardDataExtensions
    {
        public static CardVisualSpriteSet MakeSpriteSet(this CardData data, CardVisualSpriteFactorySO so)
        {
            if (data == null)
            {
                LogEx.LogError("CardData is null");
                return null;
            }

            CardVisualSpriteFactorySO.NumberSpriteSet numberSpriteSet = default;
            Sprite frontBackground = null;
            Sprite frontNumber = null;

            if (data.Kind == CardKind.Attack)
            {
                foreach (var set in so.numberSprites)
                {
                    if (data.Color != set.Color) continue;

                    numberSpriteSet = set;
                    break;
                }

                frontBackground = numberSpriteSet.Background;

                if (!data.NumberSelectState.FinalValue.HasValue)
                {
                    if (data.NumberSelectState.Selectables.Count == 9)
                        frontNumber = numberSpriteSet.Star;
                }
                else
                {
                    // index 계산
                    int index = (int)data.NumberSelectState.FinalValue - 2;
                    if (index >= 0 && index < numberSpriteSet.Numbers.Length)
                        frontNumber = numberSpriteSet.Numbers[index];
                    else
                        LogEx.LogWarning($"Invalid NumberValue {data.NumberSelectState.FinalValue} for color {data.Color} (Id) : {data.Id}");
                }
            }

            else if (data.Kind == CardKind.Move)
            {
                var m = so.moveSprites;

                if (data.DirectionSelectState.FinalValue == null)
                {
                    switch (data.DirectionSelectState.Selectables.Count)
                    {
                        case 2:
                            break;
                        case 4:
                            frontBackground = m.All;
                            break;
                        default:
                            LogEx.LogError($"Invalid MoveData. (Id) : {data.Id}");
                            break;
                    }
                }
                else
                {
                    frontBackground = data.DirectionSelectState.FinalValue switch
                    {
                        Direction.Up => m.Up,
                        Direction.Down => m.Down,
                        Direction.Left => m.Left,
                        Direction.Right => m.Right,
                        _ => null
                    };
                }
            }

            return new CardVisualSpriteSet(frontBackground, frontNumber);
        }
    }
}