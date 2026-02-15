using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Dungeon.UI
{
    public class DungeonTransitionUI : MonoBehaviour
    {
        [SerializeField] private Image environmentImage;
        [SerializeField] private RectTransform transitionPanel;

        private RectTransform _initialEnvironmentImagePosition;
        private RectTransform _initialPanelPosition;
        

        private void Awake()
        {
            _initialEnvironmentImagePosition = Instantiate(environmentImage, transform).rectTransform;
            _initialPanelPosition = Instantiate(transitionPanel, transform);
            
            _initialPanelPosition.gameObject.SetActive(false);
            _initialEnvironmentImagePosition.gameObject.SetActive(false);
        }

        public async UniTask ShowTransition(CancellationToken cancellationToken = default)
        {
            // 트랜지션 패널 초기 위치 위쪽으로
            transitionPanel.anchoredPosition = new Vector2(0, transitionPanel.rect.height);
        }
    }
}