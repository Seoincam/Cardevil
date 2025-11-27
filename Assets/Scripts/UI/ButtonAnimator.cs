using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool hover = true;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private bool press = true;
        [SerializeField] private float pressedScale = .95f;
        [SerializeField] private float duration = .1f;

        private RectTransform _target;
        private Button _button;
        private Vector3 _baseScale;
        private Tween _tween;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _target = transform as RectTransform;
            
            _baseScale = _target.localScale;
        }

        private void SetScale(Vector3 scale)
        {
            _tween?.Kill();
            _tween = _target.DOScale(scale, duration)
                .SetUpdate(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable || !hover)
                return;
            SetScale(_baseScale * hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!hover)
                return;
            SetScale(_baseScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable || !press)
                return;
            SetScale(_baseScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable || !press)
                return;
            SetScale(_baseScale * hoverScale);
        }
    }
}