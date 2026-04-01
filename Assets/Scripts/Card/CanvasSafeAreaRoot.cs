using System;
using UnityEngine;

namespace Cardevil.Card
{
    [RequireComponent(typeof(RectTransform))]
    public class CanvasSafeAreaRoot : MonoBehaviour
    {
        private int _cachedWidth;
        private int _cachedHeight;
        
        private void LateUpdate()
        {
            if (_cachedWidth != Screen.width || _cachedHeight != Screen.height)
            {
                _cachedWidth = Screen.width;
                _cachedHeight = Screen.height;
                
                Rect safe = Screen.safeArea;

                Vector2 anchorMin = safe.position;
                Vector2 anchorMax = safe.position + safe.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                var rect = GetComponent<RectTransform>();
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
            }
        }
    }
}