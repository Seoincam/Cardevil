using Cardevil.NewCard.Common;
using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage.ValueSelection
{
    public delegate void ValueSelectAction(in ValueSelectionView.Values values);
    public class ValueSelectionView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject interactionCardPrefab;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer zoneSpriteRenderer;
        [SerializeField] private Camera cardCamera;

        [Header("Settings")] 
        [SerializeField] private float spacing = 2f;
        
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
        }

        public void OpenValueSelectionZone()
        {
            zoneSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }

        public void CloseValueSelectionZone()
        {
            zoneSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
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
        }

        public void ArrangeCards(CardColor[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var card = _colorToCard[colors[i]];
                card.SetSortingOrder(i);
                card.TargetLocalX = GetSlotX(i, colors.Length);
                card.TargetLocalY = GetSlotY(i, colors.Length);
            }
        }

        public void ArrangeCards(int[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                var card = _numberToCard[numbers[i]];
                card.SetSortingOrder(i);
                card.TargetLocalX = GetSlotX(i, numbers.Length);
                card.TargetLocalY = GetSlotY(i, numbers.Length);
            }
        }

        public void ArrangeCards(Direction[] directions)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                var card = _directionToCard[directions[i]];
                card.SetSortingOrder(i);
                card.TargetLocalX = GetSlotX(i, directions.Length);
                card.TargetLocalY = GetSlotY(i, directions.Length);
            }
        }

        public void Clear()
        {
            if (_cardToColor != null)
            {
                foreach (var card in _cardToColor.Keys)
                {
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
            
            ValueSelected?.Invoke(values);
        }
    }
}