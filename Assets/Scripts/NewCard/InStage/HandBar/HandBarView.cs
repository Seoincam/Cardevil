using Cardevil.Attributes;
using Cardevil.NewCard.Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public class HandBarView : MonoBehaviour
    {
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Camera cardCamera;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Transform anchor;

        [Header("Settings")] 
        [SerializeField, Range(0f, 0.5f)] private float anchorY = 0.08f;
        [SerializeField, Range(0f, 0.5f)] private float handZoneMaxY = 0.25f;
        [SerializeField, Range(0f, 1.5f)] private float cardSpacing = 0.75f;

        [Header("Prefabs")] 
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private GameObject cardPrefab;

        public event Action<ICardState> CardPointerEnter;
        public event Action<ICardState> CardPointerDown;
        public event Action<ICardState> CardDragStart;
        public event Action<ICardState> CardDragging;
        public event Action<ICardState> CardPointerUp;
        public event Action<ICardState> CardDragEnd;
        public event Action<ICardState> CardPointerExit;

        private readonly Dictionary<ICardState, NewStageCard> _cardMap = new();

        private Rect _safeArea;
        private Vector2 _viewportMin;
        private Vector2 _viewportMax;

        private int _cachedWidth;
        private int _cachedHeight;
        private float _cachedAnchorY;
        private float _cachedCardSpacing;

        private void LateUpdate()
        {
            if (Screen.width != _cachedWidth || Screen.height != _cachedHeight)
            {
                _cachedWidth = Screen.width;
                _cachedHeight = Screen.height;

                RefreshSafeArea();
                UpdateAnchorPosition();
            }

            if (!Mathf.Approximately(_cachedAnchorY, anchorY))
            {
                _cachedAnchorY = anchorY;
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
            var card = Instantiate(cardPrefab, anchor).GetComponent<NewStageCard>();
            card.Initialize(state, cardCamera);
            _cardMap.Add(state, card);

            // TODO: 구독 정리하기
            card.PointerEnter += c => CardPointerEnter?.Invoke(c.State);
            card.PointerDown += c => CardPointerDown?.Invoke(c.State);
            card.DragStart += c => CardDragStart?.Invoke(c.State);
            card.Dragging += c => CardDragging?.Invoke(c.State);
            card.PointerUp += c => CardPointerUp?.Invoke(c.State);
            card.DragEnd += c => CardDragEnd?.Invoke(c.State);
            card.PointerExit += c => CardPointerExit?.Invoke(c.State);

            card.GetComponentInChildren<TextMeshPro>().text = state.Id.ToString();
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
                var x = GetSlotX(cards.Count, i);
                
                var card = GetCardInternal(cards[i]);
                card.LocalTarget = new Vector3(x, card.LocalTarget.y, 0f);
                
                card.SetSortingOrder(i);
            }
        }

        public NewStageCard GetCard(ICardState state) => GetCardInternal(state);

        public float GetCurrentX(ICardState state)
        {
            var card = GetCardInternal(state);
            return card.CurrentX;
        }

        public Vector2 GetViewportPoint(ICardState state)
        {
            var card = GetCardInternal(state);
            return cardCamera.WorldToViewportPoint(card.transform.position);
        }

        public void StartDrag(ICardState state)
        {
            var card = GetCardInternal(state);
            card.IsDragging = true;
        }

        public void EndDrag(ICardState state)
        {
            var card = GetCardInternal(state);
            card.IsDragging = false;
        }

        public float GetSlotX(int maxHand, int index)
        {
            return (index - (maxHand - 1) * 0.5f) * cardSpacing;
        }

        /// <summary>
        /// 뷰포트 Y 기준으로 핸드 영역 내인지 판단함.
        /// </summary>
        public bool IsInHandZone(ICardState state)
        {
            var viewport = GetViewportPoint(state);
            return viewport.y < handZoneMaxY;
        }

        public void SelectCard(ICardState state, int maxHand, int index)
        {
            var card = GetCardInternal(state);
            
            var x = GetSlotX(maxHand, index);
            card.LocalTarget = new Vector3(x, 0.5f, 0f);
        }

        public void DeselectCard(ICardState state, int maxHand, int index)
        {
            var card = GetCardInternal(state);
            
            var x = GetSlotX(maxHand, index);
            card.LocalTarget = new Vector3(x, 0f, 0f);
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
            float y = Mathf.Lerp(_viewportMin.y, _viewportMax.y, anchorY);

            Vector3 viewport = new(0.5f, y, 0f);
            Vector3 anchorPosition = cardCamera.ViewportToWorldPoint(viewport);
            anchorPosition.z = 0f;

            anchor.position = anchorPosition;
        }

        private NewStageCard GetCardInternal(ICardState state)
        {
            if (!_cardMap.TryGetValue(state, out var card))
            {
                throw new Exception("[HandBarView] Card not found");
            }
            return card;
        }
    }
}