using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.ScriptableObjects
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
            if (data.Kind == CardKind.Move)
            {
                primaryNumberImg.gameObject.SetActive(false);
                UpdateVisual(data, frontImg);
            }
            else if (data.Kind == CardKind.Attack)
            {
                primaryNumberImg.gameObject.SetActive(true);
                UpdateVisual(data, frontImg, primaryNumberImg);
            }
            else
            {
                LogEx.LogError($"Invalid CardData Type. (Id) : {data.Id}");
            }
        }

        /// Move 타입의 CardVisual을 수정.
        private void UpdateVisual(CardData data, Image frontImg)
        {
            var m = moveSprites;

            if (data.DirectionSelectState.FinalValue == null)
            {
                switch (data.DirectionSelectState.Selectables.Count)
                {
                    case 2:
                        break;
                    case 4:
                        frontImg.sprite = m.All;
                        break;
                    default:
                        LogEx.LogError($"Invalid MoveData. (Id) : {data.Id}");
                        break;
                }
                return;
            }

            frontImg.sprite = data.DirectionSelectState.FinalValue switch
            {
                Direction.Up => m.Up,
                Direction.Down => m.Down,
                Direction.Left => m.Left,
                Direction.Right => m.Right,
                _ => null
            };
        }

        /// Attack 타입의 CardVisual을 수정
        private void UpdateVisual(CardData data, Image frontImg, Image primaryNumberImg)
        {
            NumberSpriteSet? spriteSet = numberSprites.FirstOrDefault(s => s.Color == data.Color);
            if (spriteSet is not { } s)
            {
                LogEx.LogError($"No sprite set found. Color: {data.Color}");
                return;
            }

            frontImg.sprite = s.Background;

            if (!data.NumberSelectState.FinalValue.HasValue)
            {
                if (data.NumberSelectState.Selectables.Count == 9)
                {
                    primaryNumberImg.sprite = s.Star;
                    return;
                }
                // TODO: 2, 3개는 그래픽 정해지면 설정
            }

            // Index 계산
            int index = (int)data.NumberSelectState.FinalValue - 2;
            if (s.Numbers != null && index >= 0 && index < s.Numbers.Length)
            {
                primaryNumberImg.sprite = s.Numbers[index];
            }
            else
            {
                LogEx.LogWarning($"Invalid NumberValue {data.NumberSelectState.FinalValue} for color {data.Color} (Id) : {data.Id}");
            }
        }


        #region Structs

        [Serializable]
        public struct NumberSpriteSet
        {
            public CardColor Color;
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
