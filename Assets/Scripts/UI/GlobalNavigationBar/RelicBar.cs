using Cardevil.Core.Attributes;
using Cardevil.Core.Systems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavigationBar
{
    public class RelicBar : MonoBehaviour
    {
        [SerializeField] private RelicIcon relicIconPrefab;
        
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private RectTransform relicsParent;
        [SerializeField] private HorizontalLayoutGroup relicsLayoutGroup;
        [SerializeField] private List<RelicIcon> relicIcons;

        [SerializeField,VisibleOnly(EditableIn.EditMode),Min(0)] private int currentPage = 0;
        [SerializeField,VisibleOnly] private int maxIconCount = 5;
        
        [HideInInspector,SerializeField] RectTransform _rectTransform;

        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = Mathf.Max(0, value);
                UpdateRelicIcons();
                UpdateButtons();
            }
        }

        private void UpdateRelicIcons()
        {
            int startIndex = currentPage * maxIconCount;
            int endIndex = Mathf.Min(startIndex + maxIconCount, relicIcons.Count);
            for (int i = 0; i < relicIcons.Count; i++)
            {
                bool isActive = i >= startIndex && i < endIndex;
                relicIcons[i].gameObject.SetActive(isActive);
            }
        }

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            if (relicIcons == null)
            {
                relicIcons = new List<RelicIcon>();
            }
            if (relicsParent == null)
            {
                relicsParent = _rectTransform;
            }
            if (relicsLayoutGroup == null)
            {
                relicsLayoutGroup = relicsParent.GetComponent<HorizontalLayoutGroup>();
            }
        }

        private void OnEnable()
        {
            previousButton.onClick.AddListener(OnPreviousButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
            ValidateRelicBar();
            ScreenManager.OnScreenResolutionChanged += OnResolutionChanged;
        }
        
        public void OnDisable()
        {
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
            ScreenManager.OnScreenResolutionChanged -= OnResolutionChanged;
        }
        
        private void OnPreviousButtonClicked()
        {
            CurrentPage--;
        }

        private void OnNextButtonClicked()
        {
            CurrentPage++;
        }
        
        
        private void OnResolutionChanged(int width, int height)
        {
            ValidateRelicBar();
        }

        private void OnRectTransformDimensionsChange()
        {
            ValidateRelicBar();
        }
        
        public void ValidateRelicBar()
        {
            RecalculateMaxIconCount();
            if (currentPage * maxIconCount >= relicIcons.Count)
            {
                currentPage = Mathf.Max(0, (relicIcons.Count - 1) / maxIconCount);
            }
            UpdateRelicIcons();
            UpdateButtons();
        }

        private void RecalculateMaxIconCount()
        {
            float relicContainerWidth = relicsParent.rect.width;
            float spacing = relicsLayoutGroup.spacing;
            float iconWidth = relicIconPrefab.GetComponent<RectTransform>().rect.width;
            maxIconCount = Mathf.FloorToInt((relicContainerWidth + spacing) / (iconWidth + spacing));
        }

        private void UpdateButtons()
        {
            if (relicIcons.Count <= maxIconCount)
            {
                nextButton.gameObject.SetActive(false);
                previousButton.gameObject.SetActive(false);
                return;
            }
            
            if (currentPage <= 0)
            {
                previousButton.gameObject.SetActive(false);
            }
            else
            {
                previousButton.gameObject.SetActive(true);
            }
            
            // 다음 페이지가 있는지 확인
            if (relicIcons.Count > (currentPage+1) * maxIconCount)
            {
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                nextButton.gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            if (relicIcons == null)
            {
                relicIcons = new List<RelicIcon>();
            }
            
            int childCount = relicsParent.childCount;
            if (childCount != relicIcons.Count)
            {
                relicIcons.Clear();
                for (int i = 0; i < childCount; i++)
                {
                    RelicIcon icon = relicsParent.GetChild(i).GetComponent<RelicIcon>();
                    if (icon != null)
                    {
                        relicIcons.Add(icon);
                    }
                }
            }
            
            ValidateRelicBar();
            UpdateRelicIcons();
        }
    }
}