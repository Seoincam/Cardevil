using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards
{
    /// <summary>
    /// Card의 Sprite를 가지고 있고, 직접 CardVisual의 Sprite를 수정함.
    /// </summary>
    [CreateAssetMenu(menuName = "Cards/Card Visual Sprites Factory")]
    public class CardVisualSpriteFactorySO : ScriptableObject
    {
        [SerializeField] NumberSpriteSet[] numberSprites;
        [SerializeField] MoveSpriteSet moveSprites;

        /// <summary>
        /// CardVisual로부터 Image를 받아 Sprite를 수정함.
        /// </summary>
        public void UpdataVisual(CardData data, Image frontImg, Image primaryNumberImg)
        {
            if (data.valueType == CardData.ValueType.Move)
            {
                primaryNumberImg.gameObject.SetActive(false);
                UpdateVisual(data, frontImg);
            }
            else if (data.valueType == CardData.ValueType.Number)
            {
                primaryNumberImg.gameObject.SetActive(true);
                UpdateVisual(data.Number, frontImg, primaryNumberImg);
            }
            else
            {
                LogEx.LogError("Invalid CardData Type.");
            }
        }

        /// <summary>
        /// Move 타입의 CardVisual을 수정.
        /// </summary>
        private void UpdateVisual(CardData data, Image frontImg)
        {
            var m = moveSprites;

            if (data.Move.DirectionValue == Direction.None)
            {
                if (data.selectType == CardData.SelectType.Multiple)
                {

                }
                else if (data.selectType == CardData.SelectType.All)
                {
                    frontImg.sprite = m.All;
                }
                else
                {
                    LogEx.LogError("Invalid MoveData.");
                }
                return;
            }

            frontImg.sprite = data.Move.DirectionValue switch
            {
                Direction.Up => m.Up,
                Direction.Down => m.Down,
                Direction.Left => m.Left,
                Direction.Right => m.Right,
                _ => null
            };
        }

        /// <summary>
        /// Number 타입의 CardVisual을 수정
        /// </summary>
        private void UpdateVisual(NumberData data, Image frontImg, Image primaryNumberImg)
        {
            NumberSpriteSet? spriteSet = numberSprites.FirstOrDefault(s => s.Color == data.ColorValue);
            if (spriteSet is not { } s)
            {
                LogEx.LogError($"No sprite set found. Color: {data.ColorValue}");
                return;
            }

            frontImg.sprite = s.Background;

            // Star 처리
            if (data.NumberValue == 0)
            {
                primaryNumberImg.sprite = s.Star;
                return;
            }

            // Index 계산
            int index = data.NumberValue - 2;
            if (s.Numbers != null && index >= 0 && index < s.Numbers.Length)
            {
                primaryNumberImg.sprite = s.Numbers[index];
            }
            else
            {
                LogEx.LogWarning($"Invalid NumberValue {data.NumberValue} for color {data.ColorValue}");
            }
        }


        #region Structs

        [Serializable]
        public struct NumberSpriteSet
        {
            public NumberData.CardColor Color;
            [Space]
            public Sprite Background;
            public Sprite Star;
            public Sprite[] Numbers;
        }

        [Serializable]
        public struct MoveSpriteSet
        {
            public Sprite Up;
            public Sprite Down;
            public Sprite Left;
            public Sprite Right;
            [Space]
            public Sprite UpDown;
            public Sprite LeftRight;
            public Sprite All;
        }

        #endregion
    }
}
