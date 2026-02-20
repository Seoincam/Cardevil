using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Common.Visual;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.NewCard.Visual.Controller
{
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
        
        public float TrailTime => _currentTrail?.time ?? 0f;

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
        /// 모든 요소의 Sorting Order를 설정.
        /// </summary>
        public void SetSortingOrder(int sortingOrder)
        {
            innerFrame.sortingOrder = 100 * sortingOrder + 10;
            background.sortingOrder = 100 * sortingOrder + 0;
            
            _currentLayout?.SetSortingOrder(sortingOrder);
            _currentColorJewel?.SetSortingOrder(sortingOrder);
        }
        
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
