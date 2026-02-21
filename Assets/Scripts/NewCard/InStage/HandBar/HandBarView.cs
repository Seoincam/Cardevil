using Cardevil.Attributes;
using Cardevil.NewCard.Common.Core;
using Cysharp.Threading.Tasks;
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
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Transform handBarAnchor;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Transform drawAnchor;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private Transform discardAnchor;
        
        [Header("Buttons")]
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;
        
        [Header("Config")] 
        [SerializeField] private HandBarConfig config;
        [SerializeField] private DiscardParams discardParams;
        
        [Header("Prefabs")] 
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private GameObject cardPrefab;

        public event UnityAction SortByNumberClicked;
        public event UnityAction SortByIconClicked;

        public event Action<ICardState> CardPointerEnter;
        public event Action<ICardState> CardPointerDown;
        public event Action<ICardState> CardDragStart;
        public event Action<ICardState> CardPointerUp;
        public event Action<ICardState> CardDragEnd;
        public event Action<ICardState> CardPointerExit;

        private readonly Dictionary<ICardState, HandBarCard> _cardMap = new();
        private readonly Dictionary<HandBarCard, ICardState> _reverseCardMap = new();

        private Rect _safeArea;
        private Vector2 _viewportMin;
        private Vector2 _viewportMax;

        private int _cachedWidth;
        private int _cachedHeight;
        private float _cachedAnchorY;
        private float _cachedCardSpacing;

        private void Awake()
        {
            sortByNumberButton.onClick.AddListener(() => SortByNumberClicked?.Invoke());
            sortByIconButton.onClick.AddListener(() => SortByIconClicked?.Invoke());
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
            var card = Instantiate(cardPrefab, handBarAnchor).GetComponent<HandBarCard>();
            card.transform.position = drawAnchor.position;
            
            card.Initialize(state, cardCamera);
            card.SetMode(HandBarCard.Mode.InHand);
            
            AddMap(state, card);
            BindCard(card);
            CardRegistry.Register(card);
        }
        
        /// <summary>
        /// Interaction Card를 HandBarCard로 전환합니다.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="interactionCardId"></param>
        public void ConvertToHandCard(ICardState state, uint interactionCardId)
        {
            var interactionCard = CardRegistry.GetInteractionCard(interactionCardId);
            CardRegistry.Unregister(interactionCard);
            
            var handBarCard = interactionCard.ConvertToHandCard(state);
            handBarCard.transform.SetParent(handBarAnchor);
            
            AddMap(state, handBarCard);
            BindCard(handBarCard);
            CardRegistry.Register(handBarCard);
        }

        public void DestroyCard(ICardState state)
        {
            if (_cardMap.Remove(state, out var card))
            {
                _reverseCardMap.Remove(card);
                CardRegistry.Unregister(card);
                Destroy(card.gameObject);
            }
        }

        public void ArrangeCards(IReadOnlyList<ICardState> cards, ICardState excluded = null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var card = InternalGetCard(cards[i]);

                if (excluded != cards[i])
                {
                    card.VisualController.SetSortingOrder(i);   
                }
                
                var slotX = GetSlotX(i, cards.Count);
                card.LocalTargetPosition.SetOffset(OffsetKey.HandSlot, new Vector3(slotX, 0f));
                
                var curve = config.GetCurve(i, cards.Count);

                var curveY = curve.yOffset;
                card.LocalTargetPosition.SetOffset(OffsetKey.HandCurve, new Vector3(0f, curveY));
                
                card.TargetCurveAngleZ = curve.rotation;
            }
        }
        
        public uint GetCardId(ICardState state)
        {
            var card = InternalGetCard(state);
            return CardRegistry.GetId(card);
        }

        /// <summary>
        /// 카드의 핸드바 내 LocalPosition X를 반환. 
        /// </summary>
        public float GetCurrentX(ICardState state)
        {
            var card = InternalGetCard(state);
            return card.CurrentX;
        }

        /// <summary>
        /// 카드의 추적 타겟을 포인터로 설정함.
        /// </summary>
        public void StartDrag(ICardState state)
        {
            var card = InternalGetCard(state);
            card.SetMode(HandBarCard.Mode.Dragging);
            card.VisualController.SetSortingOrderLast();
        }

        /// <summary>
        /// 카드의 추적 타겟을 슬롯(Local Position)으로 설정함.
        /// </summary>
        public void EndDrag(ICardState state)
        {
            var card = InternalGetCard(state);
            card.SetMode(HandBarCard.Mode.InHand);
        }

        /// <summary>
        /// 카드의 추적 타겟을 WorldPosition으로 설정함.
        /// </summary>
        public void StartValueSelection(ICardState state, Vector3 worldPosition)
        {
            var card = InternalGetCard(state);
            
            card.WorldTargetPosition = worldPosition;
            card.SetMode(HandBarCard.Mode.WorldTarget);
        }

        /// <summary>
        /// 카드의 추적 타겟을 슬롯(Local Position)으로 설정함.
        /// </summary>
        /// <param name="state"></param>
        public void EndValueSelection(ICardState state)
        {
            var card = InternalGetCard(state);
            card.SetMode(HandBarCard.Mode.InHand);
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

        /// <summary>
        /// 카드의 SelectionY를 설정함.
        /// </summary>
        public void SelectCard(ICardState state)
        {
            var card = InternalGetCard(state);
            card.LocalTargetPosition.SetOffset(OffsetKey.Selection, new Vector3(0f, 0.5f));
        }

        /// <summary>
        /// 카드의 SelectionY를 해제함.
        /// </summary>
        public void DeselectCard(ICardState state)
        {
            var card = InternalGetCard(state);
            card.LocalTargetPosition.ClearOffset(OffsetKey.Selection);
        }

        /// <summary>
        /// 카드의 모습을 최신 상태에 맞춰 갱신함.
        /// </summary>
        public void UpdateCardVisual(ICardState state)
        {
            var card = InternalGetCard(state);
            card.VisualController.SetLayout(state);
        }

        public async UniTask MoveCardToDiscardAnchor(ICardState state)
        {
            var card = InternalGetCard(state);
            card.VisualController.SetSortingOrderLast();
             
            float distance = Vector3.Distance(card.transform.position, discardAnchor.position);
            float duration = distance / discardParams.Speed;

            card.SetMode(HandBarCard.Mode.Unmanaged);
            await card.PlayDiscardAsync(discardAnchor.position, duration, discardParams);
            await UniTask.Delay(TimeSpan.FromSeconds(card.VisualController.TrailTime));
            
            card.VisualController.ClearTrail();
            DestroyCard(state);
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

            handBarAnchor.position = anchorPosition;
        }

        private HandBarCard InternalGetCard(ICardState state)
        {
            if (!_cardMap.TryGetValue(state, out var card))
            {
                throw new Exception("[HandBarView] Card not found");
            }
            return card;
        }
        
        private float GetSlotX(int index, int maxHand)
        {
            return (index - (maxHand - 1) * 0.5f) * config.CardSpacing;
        }
        
        private Vector2 GetViewportPoint(ICardState state)
        {
            var card = InternalGetCard(state);
            return cardCamera.WorldToViewportPoint(card.transform.position);
        }

        private void AddMap(ICardState state, HandBarCard card)
        {
            _cardMap.Add(state, card);
            _reverseCardMap.Add(card, state);
        }

        private void BindCard(HandBarCard card)
        {
            // TODO: 구독 정리하기
            card.PointerEnter += c => CardPointerEnter?.Invoke(_reverseCardMap[c]);
            card.PointerDown += c => CardPointerDown?.Invoke(_reverseCardMap[c]);
            card.DragStart += c => CardDragStart?.Invoke(_reverseCardMap[c]);
            card.PointerUp += c => CardPointerUp?.Invoke(_reverseCardMap[c]);
            card.DragEnd += c => CardDragEnd?.Invoke(_reverseCardMap[c]);
            card.PointerExit += c => CardPointerExit?.Invoke(_reverseCardMap[c]);
        }
    }
}