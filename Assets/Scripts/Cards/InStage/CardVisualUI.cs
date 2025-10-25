using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core;
using System;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.InStage
{
    public class CardVisualUI : MonoBehaviour, IPointerClickHandler, IClearable
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;
        
        [Header("Card Visual")]
        [SerializeField] private Transform shakeObject;
        [SerializeField] private Image frontBackground;
        [SerializeField] private Image frontNumber;

        public event Action OnClicked;
        
        private int _id = -1;

        public void Init(int id, CardVisualSpriteSet visualSpriteSet)
        {
            _id = id;    
            UpdateVisual(visualSpriteSet);
        }
        
        public void Clear()
        {
            OnClicked = null;
            _id = -1;
        }
        
        public void UpdateVisual(CardVisualSpriteSet visualSpriteSet)
        {
            frontBackground.sprite = visualSpriteSet.FrontBackgroundImage;
            frontNumber.sprite = visualSpriteSet.FrontNumberImage;
            frontNumber.gameObject.SetActive(visualSpriteSet.FrontNumberImage);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }
        
        
    }
}


