using UnityEngine;

namespace Cardevil.UI
{
    public class CanvasLetterbox : MonoBehaviour
    {
        private const float TargetAspectRatio = 16f / 9f;
        private RectTransform _rectTransform;
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            UpdateLetterbox();
        }
        
        private void UpdateLetterbox()
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float scaleFactor = screenAspect / TargetAspectRatio;

            if (scaleFactor < 1f)
            {
                // 세로 레터박스
                _rectTransform.anchorMin = new Vector2(0f, (1f - scaleFactor) / 2f);
                _rectTransform.anchorMax = new Vector2(1f, 1f - (1f - scaleFactor) / 2f);
            }
            else
            {
                // 가로 레터박스
                float horizontalScale = 1f / scaleFactor;
                _rectTransform.anchorMin = new Vector2((1f - horizontalScale) / 2f, 0f);
                _rectTransform.anchorMax = new Vector2(1f - (1f - horizontalScale) / 2f, 1f);
            }

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
        
        private void OnRectTransformDimensionsChange()
        {
            UpdateLetterbox();
        }
        
        
    }
}