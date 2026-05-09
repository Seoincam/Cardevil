using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.InStage;
using Cardevil.Card.Visual.Controller;
using Cardevil.Core.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Card.Common
{
    /// <summary>
    /// 대부분의 상황에서 공용적으로 사용할 카드 오브젝트. 각 사용처에서 이벤트를 구독하는 방식을 사용함.
    /// </summary>
    [RequireComponent(typeof(CardVisualController))]
    public class InteractionCard : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [field: SerializeField] public CardVisualController VisualController { get; private set; }
        
        [Header("Settings")]
        [field: SerializeField] public bool FollowTargetPosition { get; set; }
        
        public event Action<InteractionCard> PointerEnter;
        public event Action<InteractionCard> PointerDown;
        public event Action<InteractionCard> PointerUp;
        public event Action<InteractionCard> PointerExit;

        public float TargetLocalX { private get; set; }
        public float TargetLocalY { private get; set; }

        private Vector3 TargetLocalPosition => new(TargetLocalX, TargetLocalY);

        private void Reset()
        {
            VisualController = GetComponent<CardVisualController>();
        }

        private void LateUpdate()
        {
            if (FollowTargetPosition)
                transform.localPosition = Vector3.Lerp(transform.localPosition, TargetLocalPosition, Time.deltaTime * 10);
        }

        public void Initialize(CardVisualInput visualInput, bool followTargetPosition = false, int? layerMask = null)
        {
            VisualController.SetLayout(visualInput);
            FollowTargetPosition = followTargetPosition;

            if (layerMask.HasValue)
            {
                gameObject.SetLayerRecursively(layerMask.Value);
            }
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

        public HandBarCard ConvertToHandCard(INewCardState cardState, Camera cardCamera)
        {
            var handBarCard = gameObject.AddComponent<HandBarCard>();
            handBarCard.Initialize(cardState, cardCamera, false);
            
            Destroy(this);
            return handBarCard;
        }
    }
}