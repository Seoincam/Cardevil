using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Gameplay.Root.Stage
{
    public class ClearRewardRelicChestView : MonoBehaviour
    {
        /// <summary>
        /// 유물이 선택됐을 때 발행되는 이벤트. 선택된 유물의 인덱스를 인자로 함.
        /// </summary>
        public event UnityAction<int> RelicClicked;

        /// <summary>
        /// 유물 리롤 버튼이 눌렸을 때 발행되는 이벤트. 해당 유물의 인덱스를 인자로 함.
        /// </summary>
        public event UnityAction<int> RerollClicked;
        
        [Header("Settings")]
        [SerializeField] private float backgroundFadeDuration = 0.5f;
        
        [Space, SerializeField] private float handInitialYOffset = 100f;
        [SerializeField] private float handMoveDuration = 0.5f;

        [Space, SerializeField] private float containerFadeDuration = 0.5f;
         
        [Header("References")]
        [SerializeField] private Button[] relicButtons;
        [SerializeField] private Button[] rerollButtons;

        [Space, SerializeField] private CanvasGroup entireCanvasGroup;
        
        [Space, SerializeField] private CanvasGroup backgroundCanvasGroup;
        [SerializeField] private RectTransform handRect;

        [Space, SerializeField] private CanvasGroup containerCanvasGroup;

        private float initialHandY;

        private void Awake()
        {
            for (int i = 0; i < 3; i++)
            {
                var index = i;
                relicButtons[i].onClick.AddListener(() => RelicClicked?.Invoke(index));
                rerollButtons[i].onClick.AddListener(() => RerollClicked?.Invoke(index));
            }

            initialHandY = handRect.anchoredPosition.y;

            entireCanvasGroup.alpha = 0f;
            entireCanvasGroup.interactable = false;
            entireCanvasGroup.blocksRaycasts = false;
        }

        
        public async UniTask PlayShowAnimationAsync(IReadOnlyList<RelicDefinition> relics)
        {
            // 초기화
            entireCanvasGroup.alpha = 1f;
            entireCanvasGroup.blocksRaycasts = true;
            
            backgroundCanvasGroup.alpha = 0f;
            handRect.anchoredPosition = Vector2.up * handInitialYOffset;
            containerCanvasGroup.alpha = 0f;
            
            LogEx.Log($"뽑힌 유물: {relics[0].DisplayName}, {relics[1].DisplayName}, {relics[2].DisplayName}");
            
            for (int i = 0; i < relics.Count; i++)
            {
                if (relics[i] == null)
                {
                    LogEx.LogWarning("RelicDefinition is null.");
                    continue;
                }
                
                RefreshRelic(i, relics[i]);
            }
            
            
            // 배경화면
            await backgroundCanvasGroup.DOFade(1f, backgroundFadeDuration);
            
            // 손
            await handRect.DOAnchorPosY(initialHandY, handMoveDuration);
            
            // 콘텐츠
            await containerCanvasGroup.DOFade(1f, containerFadeDuration);
            
            entireCanvasGroup.interactable = true;
        }
        
        public void RefreshRelic(int index, RelicDefinition def)
        {
            var icon = def.DisplayIcon;
            relicButtons[index].image.sprite = icon;
        }

        public async UniTask PlayHideAnimationAsync()
        {
            entireCanvasGroup.interactable = false;
            
            await entireCanvasGroup.DOFade(0f, containerFadeDuration);

            entireCanvasGroup.blocksRaycasts = false;
        }
    }
}