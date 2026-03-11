using Cardevil.Card.Common;
using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.Visual.Controller;
using Cardevil.Core.Utils;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public delegate void ValueSelectAction(in ValueSelectionView.Values values, uint cardId);
    public class ValueSelectionView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject interactionCardPrefab;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer zoneSpriteRenderer;
        [SerializeField] private Camera cardCamera;

        [Header("Settings")] 
        [SerializeField] private float spacing = 2f;

        [SerializeField] private GameObject debugObject;
        [SerializeField] private GameObject dim;
        
        public Vector3 ZoneWorldPosition => zoneSpriteRenderer.bounds.center;
        public event ValueSelectAction ValueSelected;

        private Dictionary<InteractionCard, CardColor> _cardToColor;
        private Dictionary<CardColor, InteractionCard> _colorToCard;
        private Dictionary<InteractionCard, int> _cardToNumber;
        private Dictionary<int, InteractionCard> _numberToCard;
        private Dictionary<InteractionCard, Direction> _cardToDirection;
        private Dictionary<Direction, InteractionCard> _directionToCard;

        private float _pointerDownTime;
        
        private void Awake()
        {
            CloseValueSelectionZone();
            dim.SetActive(false);
        }

        public void OpenValueSelectionZone()
        {
            zoneSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }

        public void CloseValueSelectionZone()
        {
            zoneSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        public void SetDimActive(bool active)
        {
            dim.SetActive(active);
        }

        /// <summary>
        /// 카드가 값 선택 존 내에 있는지 여부를 반환.
        /// </summary>
        public bool IsOnValueSelectionZone(Vector3 worldPosition)
        {
            if (!zoneSpriteRenderer.gameObject.activeSelf) return false;
            
            return zoneSpriteRenderer.bounds.Contains(worldPosition);
        }
        
        public void AddColorSelectable(CardColor color, int number)
        {
            _cardToColor ??= new Dictionary<InteractionCard, CardColor>();
            _colorToCard ??= new Dictionary<CardColor, InteractionCard>();

            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Attack(color, number);

            card.Initialize(visualInput, cardCamera);
            BindCard(card);
            _cardToColor.Add(card, color);
            _colorToCard.Add(color, card);

            CardRegistry.Register(card);
        }

        public void AddNumberSelectable(CardColor color, int number)
        {
            _cardToNumber ??= new Dictionary<InteractionCard, int>();
            _numberToCard ??= new Dictionary<int, InteractionCard>();

            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Attack(color, number);

            card.Initialize(visualInput, cardCamera);
            BindCard(card);
            _cardToNumber.Add(card, number);
            _numberToCard.Add(number, card);
            
            CardRegistry.Register(card);
        }

        public void AddDirectionSelectable(Direction direction)
        {
            _cardToDirection ??= new Dictionary<InteractionCard, Direction>();
            _directionToCard ??= new Dictionary<Direction, InteractionCard>();

            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Move(direction);

            card.Initialize(visualInput, cardCamera);
            BindCard(card);
            _cardToDirection.Add(card, direction);
            _directionToCard.Add(direction, card);
            
            CardRegistry.Register(card);
        }
        
        private struct EllipseConfig
        {
            public float RadiusX;
            public float RadiusY;
            public float RadiusJitter;
            public float MinGapRatio;
            public float StartAngleOffset;
            public float MinDistance;  // 추가
        }

        private static readonly Dictionary<int, EllipseConfig> Configs = new()
        {
            { 2, new EllipseConfig { RadiusX = 5f,  RadiusY = 3f, RadiusJitter = 0.05f, MinGapRatio = 0.6f, MinDistance = 2.0f } },
            { 3, new EllipseConfig { RadiusX = 6f,  RadiusY = 4f, RadiusJitter = 0.08f, MinGapRatio = 0.5f, MinDistance = 2.0f } },
            { 4, new EllipseConfig { RadiusX = 6.5f,RadiusY = 4f, RadiusJitter = 0.08f, MinGapRatio = 0.5f, MinDistance = 2.0f, StartAngleOffset = Mathf.PI / 4f } },
            { 9, new EllipseConfig { RadiusX = 9f,  RadiusY = 6f, RadiusJitter = 0.03f, MinGapRatio = 0.4f, MinDistance = 1.5f } },
        };

        private Vector3[] MakeCirclePoints(Vector3 center, int count)
        {
            if (!Configs.TryGetValue(count, out var config))
                config = new EllipseConfig { RadiusX = 8f, RadiusY = 5f, RadiusJitter = 0.08f, MinGapRatio = 0.3f };

            var points = new Vector3[count];
            float baseAngle = Mathf.PI * 2 / count;
            float startAngle = Mathf.PI / 2 + config.StartAngleOffset;
            float maxAngleJitter = baseAngle * 0.5f * (1f - config.MinGapRatio);

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i * baseAngle + Random.Range(-maxAngleJitter, maxAngleJitter));

                // rx, ry 독립 랜덤 → 단일 jitter로 통일 (타원 형태 유지)
                float jitter = Random.Range(1f - config.RadiusJitter, 1f + config.RadiusJitter);
                points[i] = center + new Vector3(
                    Mathf.Cos(angle) * config.RadiusX * jitter * 0.6f,
                    Mathf.Sin(angle) * config.RadiusY * jitter * 0.6f
                );
            }

            // 겹침 방지: 너무 가까운 점들을 밀어냄
            SeparatePoints(points, config.MinDistance, iterations: 5);

            return points;
        }

        private void SeparatePoints(Vector3[] points, float minDistance, int iterations)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = i + 1; j < points.Length; j++)
                    {
                        Vector3 delta = points[j] - points[i];
                        float dist = delta.magnitude;

                        if (dist < minDistance && dist > 0.0001f)
                        {
                            // 두 점을 서로 반대 방향으로 밀어냄
                            Vector3 push = delta.normalized * (minDistance - dist) * 0.5f;
                            points[i] -= push;
                            points[j] += push;
                        }
                    }
                }
            }
        }
        
        private Vector3[] MakeCirclePoints(Vector3 center, float radiusX, float radiusY, int count)
        {
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2 / count;
                points[i] = center + new Vector3(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
            }
    
            return points;
        }

        public void ArrangeCards(CardColor[] colors, uint handBarCardId)
        {
            var center = Vector3.zero;
            var points = MakeCirclePoints(center, colors.Length);
            
            for (int i = 0; i < colors.Length; i++)
            {
                var card = _colorToCard[colors[i]];
                card.VisualController.SetSortingOrder(i, CardLayer.PopUp);
                // card.TargetLocalX = GetSlotX(i, colors.Length);
                // card.TargetLocalY = GetSlotY(i, colors.Length);
                
                card.TargetLocalX = points[i].x;
                card.TargetLocalY = points[i].y;
            }
            
            var handBarCard = CardRegistry.GetHandBarCard(handBarCardId);
            
            handBarCard.SetMode(HandBarCard.Mode.Unmanaged);
            handBarCard.transform.DOLocalRotate(Vector3.zero, 0.5f);
            handBarCard.transform.DOMove(center, 10f).SetSpeedBased();
            handBarCard.VisualController.SetSortingOrderLast(CardLayer.PopUp);
        }

        public void ArrangeCards(int[] numbers)
        {
            // var points = MakeCirclePoints(Vector3.zero, 5.4f, 3.6f, numbers.Length);
            var points = MakeCirclePoints(Vector3.zero, numbers.Length);

            for (int i = 0; i < numbers.Length; i++)
            {
                var card = _numberToCard[numbers[i]];
                card.VisualController.SetSortingOrder(i, CardLayer.PopUp);
                // card.TargetLocalX = GetSlotX(i, numbers.Length);
                // card.TargetLocalY = GetSlotY(i, numbers.Length);
                
                card.TargetLocalX = points[i].x;
                card.TargetLocalY = points[i].y;
            }
        }

        public void ArrangeCards(Direction[] directions)
        {
            var points = MakeCirclePoints(Vector3.zero, directions.Length);

            for (int i = 0; i < directions.Length; i++)
            {
                var card = _directionToCard[directions[i]];
                card.VisualController.SetSortingOrder(i, CardLayer.PopUp);
                card.TargetLocalX = GetSlotX(i, directions.Length);
                card.TargetLocalY = GetSlotY(i, directions.Length);
                
                card.TargetLocalX = points[i].x;
                card.TargetLocalY = points[i].y;
            }
        }

        public void Clear(uint except = 0)
        {
            if (_cardToColor != null)
            {
                foreach (var card in _cardToColor.Keys)
                {
                    if (CardRegistry.GetId(card) == except) continue;
                    
                    CardRegistry.Unregister(card);
                    
                    UnbindCard(card);
                    Destroy(card.gameObject);
                }
                _cardToColor = null;
                _colorToCard = null;
            }

            if (_cardToNumber != null)
            {
                foreach (var card in _cardToNumber.Keys)
                {
                    if (CardRegistry.GetId(card) == except) continue;
                    
                    CardRegistry.Unregister(card);
                    
                    UnbindCard(card);
                    Destroy(card.gameObject);
                }
                _cardToNumber = null;
                _numberToCard = null;
            }

            if (_cardToDirection != null)
            {
                foreach (var card in _cardToDirection.Keys)
                {
                    if (CardRegistry.GetId(card) == except) continue;
                    
                    CardRegistry.Unregister(card);
                    
                    UnbindCard(card);
                    Destroy(card.gameObject);
                }
                _cardToDirection = null;
                _directionToCard = null;
            }
        }

        public readonly struct Values
        {
            public readonly CardColor Color;
            public readonly int Number;
            public readonly Direction Direction;

            public static Values CreateColor(CardColor color)
            {
                return new Values(color, -1, Direction.None);
            }

            public static Values CreateNumber(int number)
            {
                return new Values(CardColor.None, number, Direction.None);
            }

            public static Values CreateDirection(Direction direction)
            {
                return new Values(CardColor.None, -1, direction);
            }

            private Values(CardColor color, int number, Direction direction)
            {
                Color = color;
                Number = number;
                Direction = direction;
            }
        }
        
        private float GetSlotX(int index, int cardCount)
        {
            return (index - (cardCount - 1) * 0.5f) * spacing;
        }

        private float GetSlotY(int index, int cardCount)
        {
            return 0f;
        }

        private void BindCard(InteractionCard card)
        {
            card.PointerDown += OnPointerDown;
            card.PointerUp += OnPointerUp;
        }

        private void UnbindCard(InteractionCard card)
        {
            card.PointerDown -= OnPointerDown;
            card.PointerUp -= OnPointerUp;
        }

        private void OnPointerDown(InteractionCard card)
        {
            _pointerDownTime = Time.time;
        }

        private void OnPointerUp(InteractionCard card)
        {
            if (Time.time - _pointerDownTime > 0.2f) return;
            
            OnValueSelected(card);
        }

        private void OnValueSelected(InteractionCard card)
        {
            Values values = default;
            
            if (_cardToColor != null)
            {
                values = Values.CreateColor(_cardToColor[card]);
            }

            if (_cardToNumber != null)
            {
                values = Values.CreateNumber(_cardToNumber[card]);
            }

            if (_cardToDirection != null)
            {
                values = Values.CreateDirection(_cardToDirection[card]);
            }
            
            var id = CardRegistry.GetId(card);
            ValueSelected?.Invoke(values, id);
        }
    }
}