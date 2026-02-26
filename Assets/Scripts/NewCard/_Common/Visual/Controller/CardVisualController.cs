using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Visual.Controller
{
    public enum CardLayer
    {
        Default,
        PopUp
    }
    
    public class CardVisualController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CardSingleLayout singlePrefab;
        [SerializeField] private CardDualLayout dualPrefab;
        [SerializeField] private CardTripleLayout triplePrefab;
        [SerializeField] private ColorJewelDecoration colorJewelPrefab;
        [SerializeField] private GameObject trailPrefab;

        [Header("References")] 
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SpriteRenderer innerFrame;

        private ICardLayoutSpriteRenderer _currentLayout;
        private ColorJewelDecoration _currentColorJewel;
        private TrailRenderer _currentTrail;

        private const int LastSortingOrder = 100;
        private static Dictionary<CardLayer, int> _layerIdMap;
        
        public float TrailTime => _currentTrail?.time ?? 0f;

        private void Awake()
        {
            _layerIdMap ??= new Dictionary<CardLayer, int>();
            _layerIdMap.TryAdd(CardLayer.Default, SortingLayer.NameToID("Card Default"));
            _layerIdMap.TryAdd(CardLayer.PopUp, SortingLayer.NameToID("Card PopUp"));
        }

        /// <summary>
        /// ICardState를 VisualInput으로 변환한 후, 레이아웃을 적용.
        /// </summary>
        public void SetLayout(ICardState state)
        {
            var visualInput = CardVisualInput.From(state);
            SetLayout(visualInput);
        }

        /// <summary>
        /// 레이아웃을 적용.
        /// </summary>
        public void SetLayout(CardVisualInput visualInput)
        {
            // Layout
            var layoutData = CardLayoutResolver.Resolve(visualInput);

            if (_currentLayout?.GameObject)
            {
                Destroy(_currentLayout.GameObject);
            }

            innerFrame.sprite = layoutData.InnerFrame;

            _currentLayout = layoutData.LayoutType switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<CardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform).GetComponent<CardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<CardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<CardTripleLayout>(),
                
                _ => throw new System.NotImplementedException()
            };

            _currentLayout.SetBackground(background);
            _currentLayout.Apply(in layoutData);

            // Decoration
            if (_currentColorJewel)
            {
                Destroy(_currentColorJewel);
            }
            
            var decorationData = CardDecorationResolver.Resolve(visualInput);
            if (decorationData.Decorations.HasFlag(CardDecorations.ColorJewel))
            {
                _currentColorJewel = Instantiate(colorJewelPrefab, transform).GetComponent<ColorJewelDecoration>();
                _currentColorJewel.Apply(in decorationData);
            }
        }

        /// <summary>
        /// 모든 하위 요소의 Sorting Order를 설정.
        /// </summary>
        public void SetSortingOrder(int sortingOrder, CardLayer layer = CardLayer.Default)
        {
            var layerId = _layerIdMap[layer];
            Debug.Log($"솔팅오더: {layerId}");
            
            innerFrame.sortingLayerID = layerId;
            innerFrame.sortingOrder = 100 * sortingOrder + 10;
            
            background.sortingLayerID = layerId;
            background.sortingOrder = 100 * sortingOrder + 0;
            
            _currentLayout?.SetSortingOrder(sortingOrder, layerId);
            _currentColorJewel?.SetSortingOrder(sortingOrder, layerId);
        }

        /// <summary>
        /// Sorting Order를 가장 위로 설정.
        /// </summary>
        public void SetSortingOrderLast(CardLayer layer = CardLayer.Default) => 
            SetSortingOrder(LastSortingOrder, layer);
        
        public void SetTrail()
        {
            _currentTrail = Instantiate(trailPrefab, transform).GetComponent<TrailRenderer>();
        }

        public void ClearTrail()
        {
            if (_currentTrail)
            {
                Destroy(_currentTrail);
            }
        }

        /// <summary>
        /// 카드 내부 요소의 알파값을 변경하는 트윈을 반환.
        /// </summary>
        public Tween DoFade(float targetAlpha, float duration, Ease ease)
        {
            var innerFrameTween = innerFrame
                .DOFade(targetAlpha, duration)
                .SetEase(ease);

            var sequence = DOTween.Sequence()
                .Join(innerFrameTween);

            if (_currentLayout != null)
            {
                sequence.Join(_currentLayout.SetAlpha(targetAlpha, duration, ease));
            }

            if (_currentColorJewel)
            {
                sequence.Join(_currentColorJewel.SetAlpha(targetAlpha, duration, ease));
            }
            
            return sequence;
        }
    }
}
