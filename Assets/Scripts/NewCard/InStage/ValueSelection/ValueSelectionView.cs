using Cardevil.NewCard.Common;
using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage.ValueSelection
{
    public class ValueSelectionView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject interactionCardPrefab;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer zoneSpriteRenderer;
        [SerializeField] private Camera cardCamera;

        [Header("Settings")] 
        [SerializeField] private float spacing = 2f;

        private Dictionary<CardColor, InteractionCard> _colorMap;
        private Dictionary<int, InteractionCard> _numberMap;
        private Dictionary<Direction, InteractionCard> _directionMap;

        public void OpenValueSelectionZone()
        {
            
        }

        public void CloseValueSelectionZone()
        {
            
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
            _colorMap ??= new Dictionary<CardColor, InteractionCard>();

            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Attack(color, number);
            
            card.Initialize(visualInput, cardCamera);
            _colorMap.Add(color, card);
        }

        public void AddNumberSelectable(CardColor color, int number)
        {
            _numberMap ??= new Dictionary<int, InteractionCard>();

            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Attack(color, number);
            
            card.Initialize(visualInput, cardCamera);
            _numberMap.Add(number, card);
        }

        public void AddDirectionSelectable(Direction direction)
        {
            _directionMap ??= new Dictionary<Direction, InteractionCard>();
            
            var card = Instantiate(interactionCardPrefab).GetComponent<InteractionCard>();
            var visualInput = CardVisualInput.Move(direction);
            
            card.Initialize(visualInput, cardCamera);
            _directionMap.Add(direction, card);
        }

        public void ArrangeCards(CardColor[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var card = _colorMap[colors[i]];
                card.TargetLocalX = GetSlotX(i, colors.Length);
                card.TargetLocalY = GetSlotY(i, colors.Length);
            }
        }

        public void ArrangeCards(int[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                var card = _numberMap[numbers[i]];
                card.TargetLocalX = GetSlotX(i, numbers.Length);
                card.TargetLocalY = GetSlotY(i, numbers.Length);
            }
        }

        public void ArrangeCards(Direction[] directions)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                var card = _directionMap[directions[i]];
                card.TargetLocalX = GetSlotX(i, directions.Length);
                card.TargetLocalY = GetSlotY(i, directions.Length);
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
    }
}