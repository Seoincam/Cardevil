using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using Cardevil.NewCard.InStage;
using Cardevil.NewCard.Visual.Controller;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.NewCard.Common
{
    [RequireComponent(typeof(CardVisualController))]
    public class InteractionCard : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private CardVisualController visualController;

        private Camera _cardCamera;

        public event Action<InteractionCard> PointerEnter;
        public event Action<InteractionCard> PointerDown;
        public event Action<InteractionCard> PointerUp;
        public event Action<InteractionCard> PointerExit;

        public float TargetLocalX { private get; set; }
        public float TargetLocalY { private get; set; }

        private Vector3 TargetLocalPosition => new(TargetLocalX, TargetLocalY);

        private void Reset()
        {
            visualController = GetComponent<CardVisualController>();
        }

        private void LateUpdate()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, TargetLocalPosition, Time.deltaTime * 10);
        }

        public void Initialize(CardVisualInput visualInput, Camera cardCamera)
        {
            visualController.SetLayout(visualInput);
            _cardCamera = cardCamera;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            visualController.SetSortingOrder(sortingOrder);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter");
            PointerEnter?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
            PointerDown?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
            PointerUp?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit");
            PointerExit?.Invoke(this);
        }

        public HandBarCard ConvertToHandCard(ICardState cardState)
        {
            var handBarCard = gameObject.AddComponent<HandBarCard>();
            handBarCard.Initialize(cardState, _cardCamera, false);
            
            Destroy(this);
            return handBarCard;
        }
    }
}