using Cardevil.Core.Utils;
using Cardevil.UI.Components;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI
{
    [Serializable]
    public class TooltipData
    {
        [field: SerializeField]
        public string Title { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [SerializeReference] private List<TooltipData> subTooltips = new();

        public List<TooltipData> SubTooltips => subTooltips;
    }

    public enum TooltipSide
    {
        Auto,
        Top,
        Bottom,
        Left,
        Right
    }

    public enum TooltipAlign
    {
        Start,
        Center,
        End
    }

    public class HoverTooltip : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TMPSizeFitter _descriptionSizeFitter;
        [SerializeField] private LayoutGroup _subTooltipLayoutGroup;

        [Header("Layout")]
        [SerializeField, Range(1, 3)] private int _layoutRebuildPasses = 1;
        [SerializeField] private float _tooltipGap = 12f;
        [SerializeField] private float _subTooltipGap = 8f;
        [SerializeField] private float _edgePadding = 12f;

        private Canvas _rootCanvas;
        private Canvas _placementCanvas;
        private RectTransform _placementRoot;
        private RectTransform _canvasRect;
        private RectTransform _subTooltipContainer;
        private VerticalLayoutGroup _subTooltipVerticalLayoutGroup;

        private readonly List<GameObject> _runtimeSubTooltips = new();
        private readonly Vector3[] _worldCorners = new Vector3[4];

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            CacheReferences();
        }

        private void Awake()
        {
            CacheReferences();
        }

        public void ShowTooltip(
            TooltipData data,
            RectTransform target = null,
            TooltipSide side = TooltipSide.Auto,
            TooltipAlign align = TooltipAlign.Center,
            bool createSubTooltips = true)
        {
            CacheReferences();
            gameObject.SetActive(true);

            ApplyContent(data);
            ClearSubTooltips();

            bool hasSubTooltips = createSubTooltips && data != null && data.SubTooltips.Count > 0;
            if (hasSubTooltips)
            {
                CreateSubTooltips(data);
            }
            else if (_subTooltipContainer != null)
            {
                _subTooltipContainer.gameObject.SetActive(false);
            }

            ApplyLayoutNow();

            if (target == null || _placementRoot == null || _canvasRect == null)
            {
                // 서브 툴팁만 있는 프리뷰/중첩 호출은 타겟 없이도 내용 생성까지는 진행한다.
                if (_subTooltipContainer != null && hasSubTooltips)
                {
                    ConfigureSubTooltipLayout(TooltipSide.Bottom, new Rect());
                    ApplyLayoutNow();
                }

                _rectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            Camera targetCamera = GetTargetCamera(target);
            Camera placementCamera = GetPlacementCamera();
            Rect targetScreenRect = GetScreenRect(target, targetCamera);
            Rect canvasScreenRect = GetCanvasScreenRect();

            // 타겟의 screen rect는 "타겟이 속한 캔버스" 기준으로 읽고,
            // 툴팁의 anchoredPosition은 "툴팁이 붙은 캔버스" 기준으로 계산한다.
            TooltipSide resolvedSide = ResolvePlacement(targetScreenRect, canvasScreenRect, side, align, placementCamera);
            SetMainPlacement(targetScreenRect, resolvedSide, align, placementCamera);
            ConfigureSubTooltipLayout(resolvedSide, canvasScreenRect);
            ApplyLayoutNow();
            ClampCombinedToCanvas(canvasScreenRect, placementCamera);
        }

        private TooltipSide ResolvePlacement(
            Rect targetScreenRect,
            Rect canvasScreenRect,
            TooltipSide preferredSide,
            TooltipAlign align,
            Camera placementCamera)
        {
            // 선호 방향부터 시도하고, 화면 밖이면 반대편/직교 방향으로 fallback 한다.
            foreach (TooltipSide candidate in GetCandidateSides(preferredSide))
            {
                SetMainPlacement(targetScreenRect, candidate, align, placementCamera);
                ConfigureSubTooltipLayout(candidate, canvasScreenRect);
                ApplyLayoutNow();

                if (CombinedRectFitsInCanvas(canvasScreenRect, placementCamera))
                {
                    return candidate;
                }
            }

            return preferredSide == TooltipSide.Auto ? TooltipSide.Bottom : preferredSide;
        }

        private void SetMainPlacement(Rect targetScreenRect, TooltipSide side, TooltipAlign align, Camera eventCamera)
        {
            float alignValue = GetAlignValue(side, align);
            Vector2 pivot = GetPivot(side, alignValue);
            Vector2 anchorScreenPoint = GetAnchorScreenPoint(targetScreenRect, side, alignValue);

            // side/align 조합으로 pivot과 목표 screen point를 정한 뒤,
            // 실제 부모 RectTransform 로컬 좌표로 변환해 anchoredPosition에 넣는다.
            _rectTransform.pivot = pivot;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_placementRoot, anchorScreenPoint, eventCamera, out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        private void ConfigureSubTooltipLayout(TooltipSide side, Rect canvasScreenRect)
        {
            if (_subTooltipContainer == null)
            {
                return;
            }

            bool hasRuntimeSubTooltips = _runtimeSubTooltips.Count > 0;
            _subTooltipContainer.gameObject.SetActive(hasRuntimeSubTooltips);
            if (!hasRuntimeSubTooltips)
            {
                return;
            }

            bool horizontalExpansion = side == TooltipSide.Top || side == TooltipSide.Bottom;
            Camera placementCamera = GetPlacementCamera();
            Rect mainRect = GetScreenRect(_rectTransform, placementCamera);
            Rect subRect = GetScreenRect(_subTooltipContainer, placementCamera);

            if (horizontalExpansion)
            {
                // 메인이 위/아래에 붙으면 서브는 좌우로 펼쳐 타겟을 다시 덮지 않게 한다.
                bool placeRight = canvasScreenRect.width <= 0f
                    || mainRect.xMax + _subTooltipGap + subRect.width <= canvasScreenRect.xMax - _edgePadding;
                float anchorY = side == TooltipSide.Top ? 1f : 0f;
                _subTooltipContainer.anchorMin = new Vector2(placeRight ? 1f : 0f, anchorY);
                _subTooltipContainer.anchorMax = _subTooltipContainer.anchorMin;
                _subTooltipContainer.pivot = new Vector2(placeRight ? 0f : 1f, anchorY);
                _subTooltipContainer.anchoredPosition = new Vector2(placeRight ? _subTooltipGap : -_subTooltipGap, 0f);
                ConfigureHorizontalSubTooltips(side == TooltipSide.Top, placeRight);
            }
            else
            {
                // 메인이 좌우에 붙으면 서브는 기존 세로 스택을 유지한다.
                bool placeBelow = canvasScreenRect.height <= 0f
                    || mainRect.yMin - _subTooltipGap - subRect.height >= canvasScreenRect.yMin + _edgePadding;
                float anchorX = side == TooltipSide.Left ? 0f : 1f;
                _subTooltipContainer.anchorMin = new Vector2(anchorX, placeBelow ? 0f : 1f);
                _subTooltipContainer.anchorMax = _subTooltipContainer.anchorMin;
                _subTooltipContainer.pivot = new Vector2(anchorX, placeBelow ? 1f : 0f);
                _subTooltipContainer.anchoredPosition = new Vector2(0f, placeBelow ? -_subTooltipGap : _subTooltipGap);
                ConfigureVerticalSubTooltips(side == TooltipSide.Left, placeBelow);
            }
        }

        private void ConfigureHorizontalSubTooltips(bool topAligned, bool placeRight)
        {
            if (_subTooltipContainer == null)
            {
                return;
            }

            if (_subTooltipVerticalLayoutGroup != null)
            {
                _subTooltipVerticalLayoutGroup.enabled = false;
            }

            // 기존 VerticalLayoutGroup과 충돌하지 않도록,
            // 가로 확장일 때만 런타임 배치를 직접 계산한다.
            float offsetX = 0f;
            float maxHeight = 0f;
            for (int i = 0; i < _runtimeSubTooltips.Count; i++)
            {
                if (_runtimeSubTooltips[i] == null)
                {
                    continue;
                }

                RectTransform child = _runtimeSubTooltips[i].GetComponent<RectTransform>();
                if (child == null)
                {
                    continue;
                }

                child.anchorMin = new Vector2(0f, topAligned ? 1f : 0f);
                child.anchorMax = child.anchorMin;
                child.pivot = new Vector2(placeRight ? 0f : 1f, topAligned ? 1f : 0f);
                child.anchoredPosition = new Vector2(placeRight ? offsetX : -offsetX, 0f);

                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
                offsetX += child.rect.width + _subTooltipGap;
                maxHeight = Mathf.Max(maxHeight, child.rect.height);
            }

            float totalWidth = Mathf.Max(0f, offsetX - _subTooltipGap);
            _subTooltipContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
            _subTooltipContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
        }

        private void ConfigureVerticalSubTooltips(bool leftAligned, bool placeBelow)
        {
            if (_subTooltipContainer == null || _subTooltipVerticalLayoutGroup == null)
            {
                return;
            }

            // 세로 배치일 때는 prefab에 이미 붙어 있는 VerticalLayoutGroup을 그대로 사용한다.
            _subTooltipVerticalLayoutGroup.enabled = true;
            _subTooltipVerticalLayoutGroup.childAlignment = placeBelow
                ? (leftAligned ? TextAnchor.UpperLeft : TextAnchor.UpperRight)
                : (leftAligned ? TextAnchor.LowerLeft : TextAnchor.LowerRight);

            _subTooltipContainer.anchorMin = new Vector2(leftAligned ? 0f : 1f, _subTooltipContainer.anchorMin.y);
            _subTooltipContainer.anchorMax = _subTooltipContainer.anchorMin;
            _subTooltipContainer.pivot = new Vector2(leftAligned ? 0f : 1f, _subTooltipContainer.pivot.y);
        }

        private void CreateSubTooltips(TooltipData data)
        {
            if (_subTooltipContainer == null)
            {
                return;
            }

            foreach (TooltipData subTooltipData in data.SubTooltips)
            {
                GameObject subTooltipObject = AssetUtil.Instantiate("UI/HoverTooltip", _subTooltipContainer);
                if (subTooltipObject == null)
                {
                    continue;
                }

                _runtimeSubTooltips.Add(subTooltipObject);
                HoverTooltip subTooltip = subTooltipObject.GetComponent<HoverTooltip>();
                if (subTooltip == null)
                {
                    AssetUtil.Destroy(subTooltipObject);
                    continue;
                }

                subTooltip.ShowTooltip(subTooltipData, null, TooltipSide.Bottom, TooltipAlign.Center, false);
            }
        }

        private void ClearSubTooltips()
        {
            for (int i = 0; i < _runtimeSubTooltips.Count; i++)
            {
                if (_runtimeSubTooltips[i] != null)
                {
                    AssetUtil.Destroy(_runtimeSubTooltips[i]);
                }
            }

            _runtimeSubTooltips.Clear();
        }

        private void ApplyContent(TooltipData data)
        {
            string title = data?.Title ?? string.Empty;
            string description = data?.Description ?? string.Empty;

            bool hasTitle = !string.IsNullOrEmpty(title);
            bool hasDescription = !string.IsNullOrEmpty(description);

            _titleText.gameObject.SetActive(hasTitle);
            _descriptionText.gameObject.SetActive(hasDescription);
            _titleText.text = title;
            _descriptionText.text = description;
        }

        private void ApplyLayoutNow()
        {
            Canvas.ForceUpdateCanvases();
            _descriptionText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            _descriptionSizeFitter?.Refresh(true);

            // TMP + LayoutGroup + ContentSizeFitter가 한 프레임 안에 섞여 있어서
            // 좌표 계산 전에 강제로 레이아웃을 확정한다.
            int passCount = Mathf.Max(1, _layoutRebuildPasses);
            for (int i = 0; i < passCount; i++)
            {
                if (_contentRoot != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);
                }

                if (_subTooltipContainer != null && _subTooltipContainer.gameObject.activeSelf)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_subTooltipContainer);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
                _descriptionSizeFitter?.Refresh(false);
            }
        }

        private bool CombinedRectFitsInCanvas(Rect canvasScreenRect, Camera eventCamera)
        {
            Rect combinedRect = GetCombinedScreenRect(eventCamera);
            return combinedRect.xMin >= canvasScreenRect.xMin + _edgePadding
                && combinedRect.xMax <= canvasScreenRect.xMax - _edgePadding
                && combinedRect.yMin >= canvasScreenRect.yMin + _edgePadding
                && combinedRect.yMax <= canvasScreenRect.yMax - _edgePadding;
        }

        private void ClampCombinedToCanvas(Rect canvasScreenRect, Camera eventCamera)
        {
            if (_placementRoot == null)
            {
                return;
            }

            // 메인과 서브를 합친 실제 화면 rect를 기준으로 최종 보정한다.
            Rect combinedRect = GetCombinedScreenRect(eventCamera);
            float deltaX = 0f;
            float deltaY = 0f;

            if (combinedRect.xMin < canvasScreenRect.xMin + _edgePadding)
            {
                deltaX = canvasScreenRect.xMin + _edgePadding - combinedRect.xMin;
            }
            else if (combinedRect.xMax > canvasScreenRect.xMax - _edgePadding)
            {
                deltaX = canvasScreenRect.xMax - _edgePadding - combinedRect.xMax;
            }

            if (combinedRect.yMin < canvasScreenRect.yMin + _edgePadding)
            {
                deltaY = canvasScreenRect.yMin + _edgePadding - combinedRect.yMin;
            }
            else if (combinedRect.yMax > canvasScreenRect.yMax - _edgePadding)
            {
                deltaY = canvasScreenRect.yMax - _edgePadding - combinedRect.yMax;
            }

            if (Mathf.Approximately(deltaX, 0f) && Mathf.Approximately(deltaY, 0f))
            {
                return;
            }

            Vector2 pivotScreenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, _rectTransform.position);
            if (!IsFinite(pivotScreenPoint))
            {
                return;
            }

            Vector2 screenAnchor = pivotScreenPoint + new Vector2(deltaX, deltaY);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_placementRoot, screenAnchor, eventCamera, out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        private Rect GetCombinedScreenRect(Camera eventCamera)
        {
            Rect mainRect = GetScreenRect(_rectTransform, eventCamera);
            if (_subTooltipContainer == null || !_subTooltipContainer.gameObject.activeSelf)
            {
                return mainRect;
            }

            Rect subRect = GetScreenRect(_subTooltipContainer, eventCamera);
            return Rect.MinMaxRect(
                Mathf.Min(mainRect.xMin, subRect.xMin),
                Mathf.Min(mainRect.yMin, subRect.yMin),
                Mathf.Max(mainRect.xMax, subRect.xMax),
                Mathf.Max(mainRect.yMax, subRect.yMax));
        }

        private Vector2 GetAnchorScreenPoint(Rect targetScreenRect, TooltipSide side, float alignValue)
        {
            return side switch
            {
                TooltipSide.Top => new Vector2(
                    Mathf.Lerp(targetScreenRect.xMin, targetScreenRect.xMax, alignValue),
                    targetScreenRect.yMax + _tooltipGap),
                TooltipSide.Bottom => new Vector2(
                    Mathf.Lerp(targetScreenRect.xMin, targetScreenRect.xMax, alignValue),
                    targetScreenRect.yMin - _tooltipGap),
                TooltipSide.Left => new Vector2(
                    targetScreenRect.xMin - _tooltipGap,
                    Mathf.Lerp(targetScreenRect.yMax, targetScreenRect.yMin, alignValue)),
                _ => new Vector2(
                    targetScreenRect.xMax + _tooltipGap,
                    Mathf.Lerp(targetScreenRect.yMax, targetScreenRect.yMin, alignValue))
            };
        }

        private static Vector2 GetPivot(TooltipSide side, float alignValue)
        {
            return side switch
            {
                TooltipSide.Top => new Vector2(alignValue, 0f),
                TooltipSide.Bottom => new Vector2(alignValue, 1f),
                TooltipSide.Left => new Vector2(1f, 1f - alignValue),
                _ => new Vector2(0f, 1f - alignValue)
            };
        }

        private static float GetAlignValue(TooltipSide side, TooltipAlign align)
        {
            return align switch
            {
                TooltipAlign.Start => 0f,
                TooltipAlign.End => 1f,
                _ => 0.5f
            };
        }

        private static IEnumerable<TooltipSide> GetCandidateSides(TooltipSide preferredSide)
        {
            return preferredSide switch
            {
                TooltipSide.Top => new[] { TooltipSide.Top, TooltipSide.Bottom, TooltipSide.Right, TooltipSide.Left },
                TooltipSide.Bottom => new[] { TooltipSide.Bottom, TooltipSide.Top, TooltipSide.Right, TooltipSide.Left },
                TooltipSide.Left => new[] { TooltipSide.Left, TooltipSide.Right, TooltipSide.Top, TooltipSide.Bottom },
                TooltipSide.Right => new[] { TooltipSide.Right, TooltipSide.Left, TooltipSide.Top, TooltipSide.Bottom },
                _ => new[] { TooltipSide.Bottom, TooltipSide.Top, TooltipSide.Right, TooltipSide.Left }
            };
        }

        private void CacheReferences()
        {
            _rectTransform ??= GetComponent<RectTransform>();
            _placementRoot ??= _rectTransform != null ? _rectTransform.parent as RectTransform : null;
            _placementCanvas ??= _placementRoot != null ? _placementRoot.GetComponentInParent<Canvas>()?.rootCanvas : null;
            _rootCanvas ??= _placementCanvas ?? GetComponentInParent<Canvas>()?.rootCanvas;
            _canvasRect ??= _rootCanvas != null ? _rootCanvas.transform as RectTransform : null;

            if (_contentRoot == null && transform.childCount > 0)
            {
                _contentRoot = transform.GetChild(0) as RectTransform;
            }

            if (_subTooltipLayoutGroup != null)
            {
                _subTooltipContainer = _subTooltipLayoutGroup.transform as RectTransform;
                EnsureSubTooltipComponents();
            }
        }

        private void EnsureSubTooltipComponents()
        {
            if (_subTooltipContainer == null)
            {
                return;
            }

            _subTooltipVerticalLayoutGroup ??= _subTooltipLayoutGroup as VerticalLayoutGroup;
            _subTooltipVerticalLayoutGroup ??= _subTooltipContainer.GetComponent<VerticalLayoutGroup>();
        }

        private Camera GetTargetCamera(RectTransform target)
        {
            // target rect의 world corners를 screen point로 바꿀 때는
            // 그 target이 속한 캔버스의 이벤트 카메라를 써야 한다.
            Canvas targetCanvas = target != null ? target.GetComponentInParent<Canvas>()?.rootCanvas : null;
            if (targetCanvas == null || targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return targetCanvas.worldCamera;
        }

        private Camera GetPlacementCamera()
        {
            // anchoredPosition 변환은 "툴팁이 실제로 붙어 있는 캔버스" 기준이다.
            Canvas referenceCanvas = _placementCanvas != null ? _placementCanvas : _rootCanvas;
            if (referenceCanvas == null || referenceCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return referenceCanvas.worldCamera;
        }

        private Rect GetScreenRect(RectTransform rectTransform, Camera eventCamera)
        {
            if (rectTransform == null)
            {
                return new Rect();
            }

            // 회전/스케일 영향을 포함하려고 사각형 추정 대신 4개 코너 전체를 본다.
            rectTransform.GetWorldCorners(_worldCorners);
            float xMin = float.PositiveInfinity;
            float yMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            float yMax = float.NegativeInfinity;

            for (int i = 0; i < _worldCorners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, _worldCorners[i]);
                if (!IsFinite(screenPoint))
                {
                    continue;
                }

                xMin = Mathf.Min(xMin, screenPoint.x);
                yMin = Mathf.Min(yMin, screenPoint.y);
                xMax = Mathf.Max(xMax, screenPoint.x);
                yMax = Mathf.Max(yMax, screenPoint.y);
            }

            if (float.IsInfinity(xMin) || float.IsInfinity(yMin) || float.IsInfinity(xMax) || float.IsInfinity(yMax))
            {
                return new Rect();
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private Rect GetCanvasScreenRect()
        {
            if (_rootCanvas == null || _canvasRect == null)
            {
                return new Rect(0f, 0f, Screen.width, Screen.height);
            }

            // Overlay는 Screen 전체를 기준으로 보면 되고,
            // Camera/World 계열은 실제 캔버스 rect를 다시 screen rect로 바꿔 쓴다.
            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return new Rect(0f, 0f, Screen.width, Screen.height);
            }

            return GetScreenRect(_canvasRect, GetPlacementCamera());
        }

        private static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y);
        }
    }
}
