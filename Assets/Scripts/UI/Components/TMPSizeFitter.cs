using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.Components
{
    [RequireComponent(typeof(LayoutElement))]
    [ExecuteAlways]
    public class TMPSizeFitter : MonoBehaviour
    {
        [Serializable]
        public class Option
        {
            public bool UseOption;
            public float Value;
        }

        [Header("너비 제한")]
        [SerializeField] private Option minWidthOption;
        [SerializeField] private Option maxWidthOption;

        [Header("높이 제한")]
        [SerializeField] private Option minHeightOption;
        [SerializeField] private Option maxHeightOption;

        [Header("갱신 옵션")]
        [SerializeField, Tooltip("에디터 모드에서도 자동 갱신합니다.")] private bool applyInEditMode = true;
        [SerializeField, Tooltip("OnEnable 시 1회 자동 갱신합니다.")] private bool refreshOnEnable = true;
        [SerializeField, Tooltip("이 값 이하의 크기 변화는 무시합니다."), Range(0f, 1f)]
        private float epsilon = 0.1f;

        public float MinWidth => minWidthOption.UseOption ? minWidthOption.Value : 0f;
        public float MaxWidth => maxWidthOption.UseOption ? maxWidthOption.Value : float.PositiveInfinity;
        public float MinHeight => minHeightOption.UseOption ? minHeightOption.Value : 0f;
        public float MaxHeight => maxHeightOption.UseOption ? maxHeightOption.Value : float.PositiveInfinity;

        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        private bool isRefreshing;
        private float lastWidth = -1f;
        private float lastHeight = -1f;

        private void Reset()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
            Refresh(true);
        }

        private void OnEnable()
        {
            CacheComponents();
            if (refreshOnEnable)
            {
                Refresh(true);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            Refresh(false);
        }

        public void Refresh(bool force)
        {
            if (!CanRefresh())
            {
                return;
            }

            if (isRefreshing)
            {
                return;
            }

            isRefreshing = true;

            // TMP의 preferred 값이 최신이 되도록 먼저 메쉬를 갱신합니다.
            textMeshProUGUI.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: false);

            float preferredWidth = textMeshProUGUI.preferredWidth;
            float preferredHeight = textMeshProUGUI.preferredHeight;

            float clampedWidth = Mathf.Clamp(preferredWidth, MinWidth, MaxWidth);
            float clampedHeight = Mathf.Clamp(preferredHeight, MinHeight, MaxHeight);

            bool changed = force ||
                           Mathf.Abs(clampedWidth - lastWidth) > epsilon ||
                           Mathf.Abs(clampedHeight - lastHeight) > epsilon;

            if (changed)
            {
                layoutElement.minWidth = clampedWidth;
                layoutElement.preferredWidth = clampedWidth;
                layoutElement.minHeight = clampedHeight;
                layoutElement.preferredHeight = clampedHeight;

                lastWidth = clampedWidth;
                lastHeight = clampedHeight;

                LayoutRebuilder.MarkLayoutForRebuild(layoutElement.transform as RectTransform);
            }

            isRefreshing = false;
        }

        private bool CanRefresh()
        {
            if (layoutElement == null || textMeshProUGUI == null)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                return true;
            }

            return applyInEditMode;
        }

        private void CacheComponents()
        {
            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
            }

            if (textMeshProUGUI == null)
            {
                textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void OnValidate()
        {
            CacheComponents();
            Refresh(true);
        }
    }
}