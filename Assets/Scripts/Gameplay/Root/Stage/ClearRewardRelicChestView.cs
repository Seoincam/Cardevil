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
            int relicButtonCount = relicButtons?.Length ?? 0;
            int rerollButtonCount = rerollButtons?.Length ?? 0;
            int buttonCount = Mathf.Min(relicButtonCount, rerollButtonCount);
            for (int i = 0; i < buttonCount; i++)
            {
                var index = i;
                if (relicButtons[i])
                {
                    relicButtons[i].onClick.AddListener(() => RelicClicked?.Invoke(index));
                }

                if (rerollButtons[i])
                {
                    rerollButtons[i].onClick.AddListener(() => RerollClicked?.Invoke(index));
                }
            }

            if (handRect)
            {
                initialHandY = handRect.anchoredPosition.y;
            }

            if (entireCanvasGroup)
            {
                entireCanvasGroup.alpha = 0f;
                entireCanvasGroup.interactable = false;
                entireCanvasGroup.blocksRaycasts = false;
            }
        }

        
        public async UniTask PlayShowAnimationAsync(IReadOnlyList<RelicDefinition> relics)
        {
            if (relics == null || relics.Count == 0)
            {
                LogEx.LogWarning("표시할 유물이 없습니다.");
                return;
            }

            // 초기화
            if (entireCanvasGroup)
            {
                entireCanvasGroup.alpha = 1f;
                entireCanvasGroup.blocksRaycasts = true;
            }
            
            if (backgroundCanvasGroup)
            {
                backgroundCanvasGroup.alpha = 0f;
            }

            if (handRect)
            {
                handRect.anchoredPosition = Vector2.up * handInitialYOffset;
            }

            if (containerCanvasGroup)
            {
                containerCanvasGroup.alpha = 0f;
            }
            
            LogEx.Log($"뽑힌 유물: {string.Join(", ", relics)}");
            
            int count = Mathf.Min(relics.Count, relicButtons?.Length ?? 0);
            int buttonCount = Mathf.Max(relicButtons?.Length ?? 0, rerollButtons?.Length ?? 0);
            for (int i = 0; i < buttonCount; i++)
            {
                SetButtonPairActive(i, i < count);
            }

            for (int i = 0; i < count; i++)
            {
                if (relics[i] == null)
                {
                    LogEx.LogWarning("RelicDefinition is null.");
                    continue;
                }
                
                RefreshRelic(i, relics[i]);
            }
            
            
            // 배경화면
            if (backgroundCanvasGroup)
            {
                await backgroundCanvasGroup.DOFade(1f, backgroundFadeDuration);
            }
            
            // 손
            if (handRect)
            {
                await handRect.DOAnchorPosY(initialHandY, handMoveDuration);
            }
            
            // 콘텐츠
            if (containerCanvasGroup)
            {
                await containerCanvasGroup.DOFade(1f, containerFadeDuration);
            }
            
            if (entireCanvasGroup)
            {
                entireCanvasGroup.interactable = true;
            }
        }
        
        public void RefreshRelic(int index, RelicDefinition def)
        {
            if (def == null || relicButtons == null || index < 0 || index >= relicButtons.Length || !relicButtons[index])
            {
                return;
            }

            var icon = def.DisplayIcon;
            relicButtons[index].image.sprite = icon;
        }

        private void SetButtonPairActive(int index, bool active)
        {
            if (relicButtons != null && index >= 0 && index < relicButtons.Length && relicButtons[index])
            {
                relicButtons[index].gameObject.SetActive(active);
            }

            if (rerollButtons != null && index >= 0 && index < rerollButtons.Length && rerollButtons[index])
            {
                rerollButtons[index].gameObject.SetActive(active);
            }
        }

        public async UniTask PlayHideAnimationAsync()
        {
            if (!entireCanvasGroup)
            {
                return;
            }

            entireCanvasGroup.interactable = false;
            
            await entireCanvasGroup.DOFade(0f, containerFadeDuration);

            entireCanvasGroup.blocksRaycasts = false;
        }
    }
}
