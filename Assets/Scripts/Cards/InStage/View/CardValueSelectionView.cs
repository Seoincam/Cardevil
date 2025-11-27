using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage.View
{
    public class CardValueSelectionView : MonoBehaviour
    {
        private const float CardScale = .6f;
        private const string SlotPath = "Cards/Slot";
        private readonly Dictionary<int, float> _frameWidths = new()
        {
            {2, 391}, {3, 542}, {4, 691}, {9, 1300}
        };
        
        private Image _bar;
        private RectTransform _rect;
        private readonly List<RectTransform> _slots = new();
        private readonly List<CardVisualLightUI> _visuals = new();

        public void Init()
        {
            _bar = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }
        
        public void Open(CardData cardData, Vector2 position)
        {
            // 구성
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
            
            // 애니메이션
            DoAnim().Forget();
        }

        private async UniTaskVoid DoAnim()
        {
            // 외관 초기화
            _bar.color -= new Color(0, 0, 0, _bar.color.a);
            foreach (var visual in _visuals)
                visual.Rect.localScale = Vector3.zero;
            
            // 애니메이션 처리
            var dur = .4f;
            var dur2 = .2f;
            var interval = .06f;

            await _bar.DOFade(1f, dur);

            foreach (var visual in _visuals)
            {
                visual.Rect.DOScale(CardScale, dur2)
                    .SetEase(Ease.OutBack);
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
            }
            
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
            const string path = "UI/CardUI/CardVisual_Light_UI";

            for (int i = 0; i < count; i++)
            {
                var go = Managers.Resource.Instantiate(path, _slots[i]);
                var visual = go.GetComponent<CardVisualLightUI>();
                visual.SetScale(CardScale);

                if (data.Kind == CardKind.Attack)
                {
                    var num = data.NumberSelectState.Selectables[i].value;
                    visual.Base.Setup(data.Color, num);
                }
                else if (data.Kind == CardKind.Move)
                {
                    var dir = data.DirectionSelectState.Selectables[i].value;
                    visual.Base.Setup(dir);
                }

                _visuals.Add(visual);

            }
        }
    }
}