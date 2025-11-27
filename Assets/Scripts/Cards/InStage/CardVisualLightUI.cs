using Cardevil.Cards.Data;
using Cardevil.Cards.Visual;
using Cardevil.Utils.Directions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
    public class CardVisualLightUI : MonoBehaviour
    {
        private Button _button;
        private CardVisualBaseLight _base;

        public RectTransform Rect { get; private set; }
        public CardVisualBaseLight Base
        {
            get
            {
                if (!_base)
                    _base = GetComponentInChildren<CardVisualBaseLight>();
                return _base;
            }
        }

        public void SetScale(float scale)
        {
            if (!Rect)
                Rect = GetComponent<RectTransform>();
            Rect.localScale = new Vector3(scale, scale, scale);
        }

        public void Bind(UnityAction action)
        {
            if (!_button)
                _button = GetComponent<Button>();
            _button.onClick.AddListener(action);
        }

        public void RemoveAllListeners()
        {
            if (!_button)
                _button = GetComponent<Button>();
            _button.onClick.RemoveAllListeners();
        }
    }
}