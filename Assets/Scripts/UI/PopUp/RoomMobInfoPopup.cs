using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Cardevil.UI.PopUp
{
    public class RoomMobInfoPopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup popupMainGroup;
        
        [SerializeField] private CanvasGroup buttonGroup;
        [SerializeField] private Button confirmButton;
        [Header("Template")]
        [SerializeField] private RoomMobInfoPanel templatePanel;
        
        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float confirmDelay = 0.5f;

        
        private ObjectPool<RoomMobInfoPanel> _panelPool;
        private UniTaskCompletionSource _showCompletionSource;
        
        private List<MobInfo> _mobInfos = new List<MobInfo>();
        private List<RoomMobInfoPanel> _activePanels = new List<RoomMobInfoPanel>();
        
        
        private void Awake()
        {
            buttonGroup.alpha = 0;
            
            templatePanel.gameObject.SetActive(false);
            _panelPool = new ObjectPool<RoomMobInfoPanel>(
                createFunc: () => Instantiate(templatePanel, popupMainGroup.transform),
                actionOnGet: panel => panel.gameObject.SetActive(true),
                actionOnRelease: panel => panel.gameObject.SetActive(false),
                actionOnDestroy: panel => Destroy(panel.gameObject),
                collectionCheck: false,
                defaultCapacity: 5,
                maxSize: 10);
            
            confirmButton.onClick.AddListener(OnConfirm);
        }

        private void OnDisable()
        { 
            foreach (var panel in _activePanels)
            {
                _panelPool.Release(panel);
            }
        }

        public void AddMobInfo(MobInfo mobInfo)
        {
            _mobInfos.Add(mobInfo);
        }
        
        public void ClearMobInfos()
        {
            _mobInfos.Clear();
        }
        
        public async UniTask ShowAsync(List<MobInfo> mobInfos)
        {
            _mobInfos = mobInfos;
            await ShowAsync();
        }

        public async UniTask ShowAsync()
        {
            gameObject.SetActive(true);
            foreach (var mobInfo in _mobInfos)
            {
                var panel = _panelPool.Get();
                panel.SetMobInfo(mobInfo);
                _activePanels.Add(panel);
            }

            
            canvasGroup.alpha = 0;
            popupMainGroup.alpha = 1;
            buttonGroup.alpha = 0;
            await canvasGroup.DOFade(1, fadeDuration).ToUniTask();
            await UniTask.WaitForSeconds(confirmDelay);
            await buttonGroup.DOFade(1, fadeDuration).ToUniTask();
        }
        
        public async UniTask HideAsync()
        {
            await canvasGroup.DOFade(0, fadeDuration).ToUniTask();
            gameObject.SetActive(false);
        }
        
        public void SetCompleteSource(UniTaskCompletionSource showCompletionSource)
        {
            _showCompletionSource = showCompletionSource;
        }

        private void OnConfirm()
        {
            HideAsync().ContinueWith(() =>
            {
                if (_showCompletionSource != null)
                {
                    _showCompletionSource.TrySetResult();
                }
            }).Forget();
        }
    }
}