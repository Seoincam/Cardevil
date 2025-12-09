using Cardevil.Cards.Data;
using Cardevil.Cards.Visual.Base;
using Cardevil.Utils.Directions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual.Handler
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CardVisualValueSelectionView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private CardVisualCore baseVisual;
        [SerializeField] private Button button;
        
        public CanvasGroup CanvasGroup { get; private set; }
        public RectTransform Rect { get; private set; }
        
        public event Action<(int, Direction)> Selected;
        
        private (CardColor color, int numValue, Direction dirValue) _values;
        private bool _initialized;

        public void Init(float scale, CardColor color, int numberValue)
        {
            Init(scale);
            
            _values.color = color;
            _values.numValue = numberValue;
            UpdateVisual(color, numberValue);
        }
        
        public void Init(float scale, Direction directionValue)
        {
            Init(scale);
            
            _values.dirValue = directionValue;
            UpdateVisual(directionValue);
        }
        
        private void Init(float scale)
        {
            if (!_initialized)
            {
                Rect = GetComponent<RectTransform>();
                CanvasGroup = GetComponent<CanvasGroup>();
                
                _initialized = true;
            }
            
            void SetScale(float scale)
            {
                Rect.localScale = new Vector3(scale, scale, scale);
            }
            void RemoveAllListeners()
            {
                button.onClick.RemoveAllListeners();
            }
            void Bind()
            {
                button.onClick.AddListener(() =>
                {
                    Selected?.Invoke((_values.numValue, _values.dirValue));
                });
            }
            
            SetScale(scale);
            RemoveAllListeners();
            Bind();
        }

        private void UpdateVisual(CardColor color, int numberValue)
        {
            baseVisual.InnerFrame.sprite = CardSpriteCache.GetInnerFrame(color);
            baseVisual.MainValue.sprite = CardSpriteCache.GetNumber(color, numberValue);
            baseVisual.SmallValue.sprite = CardSpriteCache.GetSmallNumber(color, numberValue);
        }
        
        private void UpdateVisual(Direction direction)
        {
            baseVisual.InnerFrame.sprite = CardSpriteCache.GetInnerFrame(direction);
            baseVisual.MainValue.sprite = CardSpriteCache.GetArrow(direction);
        }
    }
}