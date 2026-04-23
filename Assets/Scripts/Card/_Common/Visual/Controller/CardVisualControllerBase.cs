using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Visual;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public enum CardLayer
    {
        Default,
        PopUp
    }

    public abstract class CardVisualControllerBase : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] protected GameObject singlePrefab;
        [SerializeField] protected GameObject dualPrefab;
        [SerializeField] protected GameObject triplePrefab;
        [SerializeField] protected ColorJewelDecoration colorJewelPrefab;

        [Header("References")] 
        [SerializeField] protected GameObject backgroundObj;
        protected ICardRenderer _background;
        protected ICardRenderer Background => _background ??= backgroundObj?.GetComponent<ICardRenderer>();

        [SerializeField] protected GameObject innerFrameObj;
        protected ICardRenderer _innerFrame;
        protected ICardRenderer InnerFrame => _innerFrame ??= innerFrameObj?.GetComponent<ICardRenderer>();

        protected ICardLayoutGraphic _currentLayout;
        protected ColorJewelDecoration _currentColorJewel;

        protected const int LastSortingOrder = 100;
        protected static Dictionary<CardLayer, int> _layerIdMap;

        protected virtual void Awake()
        {
            if (_layerIdMap == null)
            {
                _layerIdMap = new Dictionary<CardLayer, int>(2);
                _layerIdMap.TryAdd(CardLayer.Default, SortingLayer.NameToID("Card Default"));
                _layerIdMap.TryAdd(CardLayer.PopUp, SortingLayer.NameToID("Card PopUp"));
            }
        }

        public void SetLayout(ICardState state)
        {
            var visualInput = CardVisualInput.From(state);
            SetLayout(visualInput);
        }

        public virtual void SetLayout(CardVisualInput visualInput)
        {
            var layoutData = CardLayoutResolver.Resolve(visualInput);

            if (_currentLayout?.GameObject)
            {
                Destroy(_currentLayout.GameObject);
            }

            if (InnerFrame != null)
            {
                InnerFrame.Sprite = layoutData.InnerFrame;
            }

            _currentLayout = layoutData.LayoutType switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<ICardLayoutGraphic>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform).GetComponent<ICardLayoutGraphic>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<ICardLayoutGraphic>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<ICardLayoutGraphic>(),
                
                _ => throw new System.NotImplementedException()
            };

            _currentLayout.SetBackground(Background);
            _currentLayout.Apply(in layoutData);

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

        public virtual void SetSortingOrder(int sortingOrder, CardLayer layer = CardLayer.Default)
        {
            var layerId = _layerIdMap[layer];
            
            InnerFrame?.SetSortingOrder(sortingOrder, 10, layerId);
            Background?.SetSortingOrder(sortingOrder, 0, layerId);
            
            _currentLayout?.SetSortingOrder(sortingOrder, layerId);
            _currentColorJewel?.SetSortingOrder(sortingOrder, layerId); // Need to handle jewel too later if it has SpriteRenderers
        }

        public void SetSortingOrderLast(CardLayer layer = CardLayer.Default) => 
            SetSortingOrder(LastSortingOrder, layer);

        public virtual Tween DoFade(float targetAlpha, float duration, Ease ease)
        {
            var sq = DOTween.Sequence();

            if (InnerFrame != null)
                sq.Join(InnerFrame.DOFade(targetAlpha, duration).SetEase(ease));

            if (_currentLayout != null)
                sq.Join(_currentLayout.SetAlpha(targetAlpha, duration, ease));

            if (_currentColorJewel)
                sq.Join(_currentColorJewel.SetAlpha(targetAlpha, duration, ease));
            
            return sq;
        }
    }
}
