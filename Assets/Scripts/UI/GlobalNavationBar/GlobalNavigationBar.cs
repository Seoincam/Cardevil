using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavationBar
{
    public class GlobalNavigationBar : MonoBehaviour
    {
        private static GlobalNavigationBar _instance;

        public static GlobalNavigationBar Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GlobalNavigationBar>();
                }
                return _instance;
            }
        }
        
        [Header("References")]
        [Header("Player")]
        [SerializeField] private Image PlayerAvatar;
        [SerializeField] private TextMeshProUGUI PlayerLevel;
        [SerializeField] private TextMeshProUGUI PlayerName;
        [SerializeField] private TextMeshProUGUI HitPointAmount;
        [SerializeField] private TextMeshProUGUI GoldAmount;
        
        [SerializeField] private List<ConsumableIcon> consumableIcons;
        
        
        [Space]
        [Header("Relics")]
        [SerializeField] private RelicBar relicBar;
        
        [Space]
        [Header("Menu")]
        [SerializeField] private Button deckButton;
        [SerializeField] private Button ranksButton;
        [SerializeField] private Button settingsButton;
        
        [Space(2f)]
        [Header("Settings")]
        [SerializeField] private ConsumableIcon consumableIconPrefab;
        // [SerializeField] private RelicIcon relicIconPrefab;
        [SerializeField] private RectTransform hidePositionTransform;
        
        private RectTransform _rectTransform;
        private Vector2 _initialPosition;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                LogEx.LogWarning("Multiple instances detected.");
                _instance = this;
            }
            _rectTransform = GetComponent<RectTransform>();
            _initialPosition = _rectTransform.anchoredPosition;
            Hide();
        }

        void Start()
        {
        
        }


        public void OnDeckButtonClicked()
        {
            
        }
        
        public void OnSettingsButtonClicked()
        {
            
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 위로 숨기기
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public UniTask HideAsync(float time = 0.3f, CancellationToken cancellationToken = default)
        {
            _rectTransform.anchoredPosition = _initialPosition;
            var task = _rectTransform.DOAnchorPos(hidePositionTransform.anchoredPosition, time).SetEase(Ease.InOutCubic)
                .ToUniTask(cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete);
            return task;
        }
        
        /// <summary>
        /// 아래로 나타내기
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public UniTask ShowAsync(float time = 0.3f, CancellationToken cancellationToken = default)
        {
            _rectTransform.anchoredPosition = hidePositionTransform.anchoredPosition;
            var task = _rectTransform.DOAnchorPos(_initialPosition, time).SetEase(Ease.InOutCubic).ToUniTask(cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete);
            return task;
        }
        
        [ContextMenu("Test Show")]
        public void TestShow()
        {
            ShowAsync().Forget();
        }

        [ContextMenu("Test Hide")]
        public void TestHide()
        {
            HideAsync().Forget();
        }
    }
}