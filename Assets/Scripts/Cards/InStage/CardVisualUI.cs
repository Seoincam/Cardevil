using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.ScriptableObjects;

namespace Cardevil.Cards.InStage
{
    public class CardVisualUI : MonoBehaviour
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;
        
        [Header("Card Visual")]
        [SerializeField] private Transform shakeObject;
        [SerializeField] private Image frontBackground;
        [SerializeField] private Image frontNumber;

        public void UpdateVisual(CardVisualSpriteSet visualSpriteSet)
        {
            frontBackground.sprite = visualSpriteSet.FrontBackgroundImage;
            frontNumber.sprite = visualSpriteSet.FrontNumberImage;
            frontNumber.gameObject.SetActive(visualSpriteSet.FrontNumberImage);
        }
    }
}


