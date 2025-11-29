using Cardevil.Cards.Data;
using Cardevil.Utils.Directions;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual
{
    public class CardVisualBaseLight : MonoBehaviour
    {
        [Header("SO")] 
        [SerializeField] private CardVisualAnimSetting animSetting;

        [Header("Default")] 
        [SerializeField] private Image innerFrame;
        [SerializeField] private Image mainValue;

        public void UpdateVisual(CardColor color, int numberValue)
        {
            innerFrame.sprite = CardSpriteCache.GetInnerFrame(color);
            mainValue.sprite = CardSpriteCache.GetNumber(color, numberValue);
        }

        public void UpdateVisual(Direction direction)
        {
            innerFrame.sprite = CardSpriteCache.GetInnerFrame(direction);
            mainValue.sprite = CardSpriteCache.GetArrow(direction);
        }
    }
}