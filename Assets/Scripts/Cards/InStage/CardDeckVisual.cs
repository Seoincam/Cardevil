using Cardevil.Cards.ScriptableObjects;
using Cardevil.Utils;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.InStage
{
    public class CardDeckVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private List<Transform> cards;
        [SerializeField] private CardVisualSettingSO visualSetting;
        
        public event Action PointerEnter;
        public event Action PointerExit;
        public event Action PointerUp;

        private static CardDeckVisual _instance;
        
        public static CardDeckVisual Instance => _instance;
        public Transform Front => cards[0];

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        /// <summary>
        /// Draw, Discard 등의 상호작용이 있을 때 scale tween.
        /// </summary>
        public void OnInteraction()
        {
            transform.DOScale(visualSetting.DeckInteractionScale, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase)
                .OnComplete(() => transform.DOScale(1f, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            LogEx.Log("OnPointerEnter");
            PointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LogEx.Log("OnPointerExit");
            PointerExit?.Invoke();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            LogEx.Log("OnPointerDown");
            transform.DOScale(visualSetting.DeckInteractionScale, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LogEx.Log("OnPointerUp");
            PointerUp?.Invoke();
            transform.DOScale(1f, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase);
        }
    }
}