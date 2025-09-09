using Cardevil.Utils.Directions;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Cards
{
    [CreateAssetMenu(fileName = "CardSpriteManager", menuName = "Cards/CardSpriteManager")]
    public class CardSpriteManager : ScriptableObject
    {
        [Header("Number Sprites")]
        [SerializeField] Sprite[] RedNumberSprites;
        [SerializeField] Sprite[] BlueNumberSprites;
        [SerializeField] Sprite[] GreenNumberSprites;
        [SerializeField] Sprite[] BlackNumberSprites;

        [Header("Background Sprites")]
        [SerializeField] Sprite RedBackground;
        [SerializeField] Sprite BlueBackground;
        [SerializeField] Sprite GreenBackground;
        [SerializeField] Sprite BlackBackground;

        [Header("Direction Sprites")]
        [SerializeField] Sprite Up;
        [SerializeField] Sprite Down;
        [SerializeField] Sprite Left;
        [SerializeField] Sprite Right;
        [SerializeField] Sprite LeftRight;
        [SerializeField] Sprite UpDown;
        [SerializeField] Sprite AllDirection;



        /// <summary>
        /// 숫자 배경 스프라이트를 반환.
        /// </summary>
        public Sprite GetNumberBackground(NumberData.CardColor color)
        {
            return color switch
            {
                NumberData.CardColor.Red => RedBackground,
                NumberData.CardColor.Blue => BlueBackground,
                NumberData.CardColor.Green => GreenBackground,
                NumberData.CardColor.Black => BlackBackground,
                _ => null
            };
        }

        /// <summary>
        /// 숫자 스프라이트를 반환.
        /// </summary>
        /// <param name="number">2~10 사이의 값. 오망성은 11로 처리</param>
        public Sprite GetNumber(NumberData.CardColor color, int number, CardData.SelectType selectType)
        {
            if (selectType == CardData.SelectType.All && number == 0)
            {
                return color switch
                {
                    NumberData.CardColor.Red => RedNumberSprites[9],
                    NumberData.CardColor.Blue => BlueNumberSprites[9],
                    NumberData.CardColor.Green => GreenNumberSprites[9],
                    NumberData.CardColor.Black => BlackNumberSprites[9],
                    _ => null
                };
            }

            number -= 2;
            return color switch
            {
                NumberData.CardColor.Red => RedNumberSprites[number],
                NumberData.CardColor.Blue => BlueNumberSprites[number],
                NumberData.CardColor.Green => GreenNumberSprites[number],
                NumberData.CardColor.Black => BlackNumberSprites[number],
                _ => null
            };
        }

        public Sprite GetMoveBackground(Direction direction, CardData.SelectType selectType)
        {
            Sprite sprite = null;

            if (selectType == CardData.SelectType.None)
                sprite = direction switch
                {
                    Direction.Up => Up,
                    Direction.Down => Down,
                    Direction.Left => Left,
                    Direction.Right => Right,
                    _ => null
                };

            else if (selectType == CardData.SelectType.Multiple)
                sprite = direction switch
                {
                    Direction.Up => UpDown,
                    Direction.Down => UpDown,
                    Direction.Left => LeftRight,
                    Direction.Right => LeftRight,
                    _ => null
                };

            else if (selectType == CardData.SelectType.All)
                sprite = AllDirection;

            return sprite;
        }
    }
}
