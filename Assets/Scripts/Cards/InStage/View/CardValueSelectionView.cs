using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.InStage.View
{
    public class CardValueSelectionView : MonoBehaviour
    {
        private const string SlotPath = "Cards/Slot";
        private readonly Dictionary<int, float> _frameWidths = new()
        {
            {2, 308f}, {3, 426f}, {4, 544f}, {9, 1134f}
        };
        
        private RectTransform _rect;
        private readonly List<RectTransform> _slots = new();
        private readonly List<CardVisualBaseLight> _visuals = new();
        
        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            
            gameObject.SetActive(false);
        }
        
        public void Open(CardData cardData, Vector2 position)
        {
            // TODO 데이터 해석
            int count = cardData.Kind switch
            {
                CardKind.Attack => cardData.NumberSelectState.Selectables.Count,
                CardKind.Move => cardData.DirectionSelectState.Selectables.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            ConfigureFrame(count);
            ConfigureSlots(count);
            ConfigureCards(cardData, count);

            _rect.anchoredPosition = position; 
            gameObject.SetActive(true);
        }
        
        private void ConfigureFrame(int slotCount)
        {
            if (!_frameWidths.TryGetValue(slotCount, out var width))
            {
                LogEx.LogWarning("지정되지 않은 선택 가능 개수: " + slotCount);
                width = slotCount * 140f;
            }

            _rect.sizeDelta = new Vector2(width, _rect.rect.height);
        }

        /// <summary>
        /// 슬롯 개수를 조정.  
        /// 부족한 슬롯은 새로 생성, 초과한 슬롯은 제거.
        /// </summary>
        private void ConfigureSlots(int slotCount)
        {
            while (_slots.Count < slotCount)
            {
                var slot = Managers.Resource.Instantiate(SlotPath, _rect).GetComponent<RectTransform>();
                _slots.Add(slot);
            }

            while (_slots.Count > slotCount)
            {
                var last = _slots[^1];
                _slots.RemoveAt(_slots.Count - 1);
                Managers.Resource.Destroy(last.gameObject);
            }
        }

        private void ConfigureCards(CardData data, int count)
        {
            const string path = "Cards/CardVisualBaseLight";

            for (int i = 0; i < count; i++)
            {
                var visual = Managers.Resource.Instantiate(path, _slots[i]).GetComponent<CardVisualBaseLight>();

                if (data.Kind == CardKind.Attack)
                {
                    var num = data.NumberSelectState.Selectables[i].value;
                    visual.Setup(data.Color, num);
                }
                else if (data.Kind == CardKind.Move)
                {
                    var dir = data.DirectionSelectState.Selectables[i].value;
                    visual.Setup(dir);
                }

                _visuals.Add(visual);

            }
        }
    }
}