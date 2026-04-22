using UnityEditor;
using UnityEngine;

namespace Cardevil.Editor
{

    public class AnchorsToCorners : UnityEditor.Editor
    {
        [MenuItem("RectTransform/Anchors to Corners %l")] // 단축키 Ctrl + L (또는 메뉴 사용)
        static void UpdateAnchors()
        {
            foreach (Transform transform in Selection.transforms)
            {
                RectTransform t = transform as RectTransform;
                
                if (t == null || t.parent == null) continue;
                RectTransform pt = t.parent as RectTransform;
                if (pt == null) continue;

                // 현재 위치를 부모의 크기 대비 비율로 계산
                Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
                Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);

                // 앵커 값 적용
                t.anchorMin = newAnchorsMin;
                t.anchorMax = newAnchorsMax;
            
                // 앵커를 옮겼으니 오프셋(픽셀 값)은 0으로 초기화
                t.offsetMin = t.offsetMax = Vector2.zero;
            }
        }
    }
}