using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Gameplay.Root.Stage
{
    public class ClearRewardTableView : MonoBehaviour
    {
        /// <summary>
        /// 클릭되었을 때 발행되는 이벤트.
        /// </summary>
        public event UnityAction Tapped;

        [Header("첫 등장 설정")]
        [SerializeField] private float initialYOffset = 50f;
        [SerializeField] private float initialShowDuration = 0.5f;
        
        [Header("아이콘 등장 설정")]
        [SerializeField] private float iconYOffset = 25f;
        [SerializeField] private float iconShowDuration = 0.3f;

        [Header("전체")]
        [SerializeField] private RectTransform rect;
        [SerializeField] private CanvasGroup entireCanvasGroup;
        [SerializeField] private RectTransform handAndTableRect;
        [SerializeField] private RectTransform iconLayoutGroupRect;

        [Space, SerializeField] private Button button;
        
        [Header("유물 상자")]
        [SerializeField] private GameObject chestContainer;
        [SerializeField] private RectTransform chestRect;
        [SerializeField] private CanvasGroup chestCanvasGroup;
        
        [Header("돈")]
        [SerializeField] private RectTransform coinRect;
        [SerializeField] private CanvasGroup coinCanvasGroup;
        [SerializeField] private TextMeshProUGUI coinCountText;
        
        [Header("사라짐 설정")]
        [SerializeField, Min(0f)] private float hideY = 700f;
        [SerializeField] private float hideDuration = 1f;
        [SerializeField] private Ease hideEase;
        
        private void Awake()
        {
            button.interactable = false;
            button.onClick.AddListener(() => Tapped?.Invoke());
            
            entireCanvasGroup.alpha = 0f;
            entireCanvasGroup.interactable = false;
            entireCanvasGroup.blocksRaycasts = false;
        }

        public async UniTask PlayShowAnimationAsync(int count, bool isBossStage)
        {
            // 초기화
            rect.anchoredPosition = Vector2.up * initialYOffset;
            entireCanvasGroup.alpha = 0f;
            entireCanvasGroup.blocksRaycasts = true;

            coinRect.anchoredPosition = Vector2.up * iconYOffset;
            coinCountText.text = count.ToString();
            coinCanvasGroup.alpha = 0f;
            
            chestRect.anchoredPosition = Vector2.up * iconYOffset;
            chestContainer.SetActive(isBossStage);
            chestCanvasGroup.alpha = 0f;

            var seq = DOTween.Sequence();
            
            // 처음 등장 애니메이션
            seq.Append(rect.DOAnchorPosY(0f, initialShowDuration))
                .Join(entireCanvasGroup.DOFade(1f, initialShowDuration));
            
            // 상자 등장
            if (isBossStage)
            {
                seq.Append(chestRect.DOAnchorPosY(0f, iconShowDuration))
                    .Join(chestCanvasGroup.DOFade(1f, iconShowDuration));
            }
            
            // 돈 등장
            seq.Append(coinRect.DOAnchorPosY(0f, iconShowDuration))
                .Join(coinCanvasGroup.DOFade(1f, iconShowDuration));

            await seq;
            
            entireCanvasGroup.interactable = true;
            button.interactable = true;
        }

        public void HideChestIcon()
        {
            chestContainer.gameObject.SetActive(false);   
        }

        public async UniTask PlayHideAnimationAsync()
        {
            var seq = DOTween.Sequence();
            
            // 테이블 위로 올라가기
            seq.Join(handAndTableRect.DOAnchorPosY(hideY, hideDuration)
                .SetEase(hideEase));
            
            // 돈 아래로 내려가기
            seq.Join(iconLayoutGroupRect.DOAnchorPosY(-hideY, hideDuration)
                .SetEase(hideEase));

            await seq;
        }
    }
}