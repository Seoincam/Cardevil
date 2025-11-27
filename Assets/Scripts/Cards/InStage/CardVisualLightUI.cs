using Cardevil.Cards.Data;
using Cardevil.Cards.Visual;
using Cardevil.Utils.Directions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
    public class CardVisualLightUI : MonoBehaviour
    {
        public event Action<(int, Direction)> Selected;
        
        private Button _button;
        private CardVisualBaseLight _base;
        
        private (CardColor color, int numValue, Direction dirValue) _values;
        private bool _initialized;

        public RectTransform Rect { get; private set; }

        public void Init(float scale, CardColor color, int numberValue)
        {
            Init(scale);
            
            _values.color = color;
            _values.numValue = numberValue;
            _base.UpdateVisual(color, numberValue);
        }
        public void Init(float scale, Direction directionValue)
        {
            Init(scale);
            
            _values.dirValue = directionValue;
            _base.UpdateVisual(directionValue);
        }
        
        private void Init(float scale)
        {
            if (!_initialized)
            {
                _button = GetComponentInChildren<Button>();
                _base = GetComponentInChildren<CardVisualBaseLight>();
                Rect = GetComponent<RectTransform>();
                
                _initialized = true;
            }
            
            SetScale(scale);
            RemoveAllListeners();
            Bind();
        }

        private void SetScale(float scale)
        {
            Rect.localScale = new Vector3(scale, scale, scale);
        }

        private void Bind()
        {
            _button.onClick.AddListener(() =>
            {
                Selected?.Invoke((_values.numValue, _values.dirValue));
            });
        }
        
        private void RemoveAllListeners()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}