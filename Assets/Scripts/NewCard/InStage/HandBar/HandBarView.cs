using Cardevil.Attributes;
using Cardevil.NewCard.Common.Core;
using System;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.NewCard.InStage
{
    public class HandBarView : MonoBehaviour
    {
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Camera cardCamera;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Transform anchor;
        
        [Header("Buttons")]
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;
        
        [Header("Config")] 
        [SerializeField] private HandBarConfig config;

        [Header("Prefabs")] 
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private GameObject cardPrefab;

        public event UnityAction SortByNumberClicked;
        public event UnityAction SortByIconClicked;

        public event Action<ICardState> CardPointerEnter;
        public event Action<ICardState> CardPointerDown;
        public event Action<ICardState> CardDragStart;
        public event Action<ICardState> CardDragging;
        public event Action<ICardState> CardPointerUp;
        public event Action<ICardState> CardDragEnd;
        public event Action<ICardState> CardPointerExit;

        private readonly Dictionary<ICardState, HandBarCard> _cardMap = new();

        private Rect _safeArea;
        private Vector2 _viewportMin;
        private Vector2 _viewportMax;

        private int _cachedWidth;
        private int _cachedHeight;
        private float _cachedAnchorY;
        private float _cachedCardSpacing;

        private void Awake()
        {
            sortByNumberButton.onClick.AddListener(SortByNumberClicked);
            sortByIconButton.onClick.AddListener(SortByIconClicked);
        }

        private void LateUpdate()
        {
            if (Screen.width != _cachedWidth || Screen.height != _cachedHeight)
            {
                _cachedWidth = Screen.width;
                _cachedHeight = Screen.height;

                RefreshSafeArea();
                UpdateAnchorPosition();
            }

            if (!Mathf.Approximately(_cachedAnchorY, config.AnchorY))
            {
                _cachedAnchorY = config.AnchorY;
                UpdateAnchorPosition();
            }

            if (!Mathf.Approximately(_cachedCardSpacing, _cachedAnchorY))
            {
                // _cachedCardSpacing = cardSpacing;
                // ArrangeCards();
            }
        }

        public void CreateCard(ICardState state)
        {
            var card = Instantiate(cardPrefab, anchor).GetComponent<HandBarCard>();
            card.Initialize(state, cardCamera);
            card.FollowTarget = HandBarCard.FollowTargetType.LocalPosition;
            _cardMap.Add(state, card);

            // TODO: 구독 정리하기
            card.PointerEnter += c => CardPointerEnter?.Invoke(c.State);
            card.PointerDown += c => CardPointerDown?.Invoke(c.State);
            card.DragStart += c => CardDragStart?.Invoke(c.State);
            card.Dragging += c => CardDragging?.Invoke(c.State);
            card.PointerUp += c => CardPointerUp?.Invoke(c.State);
            card.DragEnd += c => CardDragEnd?.Invoke(c.State);
            card.PointerExit += c => CardPointerExit?.Invoke(c.State);
        }

        public void DestroyCard(ICardState state)
        {
            if (_cardMap.Remove(state, out var card))
            {
                Destroy(card.gameObject);
            }
        }

        public void ArrangeCards(IReadOnlyList<ICardState> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var card = InternalGetCard(cards[i]);
                var curve = config.GetCurve(i, cards.Count);
                
                card.TargetLocalX = GetSlotX(i, cards.Count);
                card.TargetCurveAngleZ = curve.rotation;
                card.TargetLocalCurveY = curve.yOffset;
                    
                card.SetSortingOrder(i);
            }
        }

        public HandBarCard GetCard(ICardState state) => InternalGetCard(state);

        public float GetCurrentX(ICardState state)
        {
            var card = InternalGetCard(state);
            return card.CurrentX;
        }

        public Vector2 GetViewportPoint(ICardState state)
        {
            var card = InternalGetCard(state);
            return cardCamera.WorldToViewportPoint(card.transform.position);
        }

        public void StartDrag(ICardState state)
        {
            var card = InternalGetCard(state);
            card.FollowTarget = HandBarCard.FollowTargetType.Pointer;
        }

        public void EndDrag(ICardState state)
        {
            var card = InternalGetCard(state);
            card.FollowTarget = HandBarCard.FollowTargetType.LocalPosition;
        }

        public void SetWorldPosition(ICardState state, Vector3 worldPosition)
        {
            var card = InternalGetCard(state);
            card.TargetWorldPosition = worldPosition;
            card.FollowTarget = HandBarCard.FollowTargetType.WorldPosition;
        }

        public void UnsetWorldPosition(ICardState state)
        {
            var card = InternalGetCard(state);
            card.FollowTarget = HandBarCard.FollowTargetType.LocalPosition;
        }

        public float GetSlotX(int index, int maxHand)
        {
            return (index - (maxHand - 1) * 0.5f) * config.CardSpacing;
        }

        /// <summary>
        /// 뷰포트 Y 기준으로 핸드 영역 내인지 판단함.
        /// </summary>
        public bool IsInHandZone(ICardState state)
        {
            var viewport = GetViewportPoint(state);
            return viewport.y < config.HandZoneMaxY;
        }
        
        /// <summary>
        /// 카드의 월드 포지션을 반환함.
        /// </summary>
        public Vector3 GetWorldPosition(ICardState state)
        {
            var card = InternalGetCard(state);
            return card.transform.position;
        }

        public void SelectCard(ICardState state, int maxHand, int index)
        {
            var card = InternalGetCard(state);
            card.TargetSelectionY = 0.5f;
        }

        public void DeselectCard(ICardState state, int maxHand, int index)
        {
            var card = InternalGetCard(state);
            card.TargetSelectionY = 0f;
        }

        public void UpdateVisual(ICardState state)
        {
            var card = InternalGetCard(state);
            card.UpdateVisual();
        }

        private void RefreshSafeArea()
        {
            _safeArea = Screen.safeArea;

            _viewportMin = new(
                _safeArea.xMin / _cachedWidth,
                _safeArea.yMin / _cachedHeight
            );

            _viewportMax = new(
                _safeArea.xMax / _cachedWidth,
                _safeArea.yMax / _cachedHeight
            );
        }

        private void UpdateAnchorPosition()
        {
            float y = Mathf.Lerp(_viewportMin.y, _viewportMax.y, config.AnchorY);

            Vector3 viewport = new(0.5f, y, 0f);
            Vector3 anchorPosition = cardCamera.ViewportToWorldPoint(viewport);
            anchorPosition.z = 0f;

            anchor.position = anchorPosition;
        }

        private HandBarCard InternalGetCard(ICardState state)
        {
            if (!_cardMap.TryGetValue(state, out var card))
            {
                throw new Exception("[HandBarView] Card not found");
            }
            return card;
        }
    }
}