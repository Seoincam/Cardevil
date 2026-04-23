using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems;
using Cardevil.Gameplay.Items;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;

namespace Cardevil.UI.PopUp
{
    public class Slot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public SlotMachine slotMachineManager;

        [Header("Tag UI References")]
        [SerializeField] private Image tagImage;         // Tag 오브젝트의 Image 컴포넌트
        [SerializeField] private CanvasGroup tagCanvasGroup; // Tag 오브젝트의 CanvasGroup
        [SerializeField] private Text tagText; // Tag 하위의 Text 컴포넌트

        [Header("Tag Sprites")]
        [SerializeField] private Sprite normalTag;
        [SerializeField] private Sprite rareTag;
        [SerializeField] private Sprite epicTag;
        [SerializeField] private Sprite legendTag;


        public int slotIndex; // 0, 1, 2 (SlotMachine에서 할당)
        public SlotMachineAnimation animController; // 하이라이트 호출을 위한 참조

        public Item item; // 슬롯에 등장할 아이템
        public Image itemIconImage;
        public Text itemNameText;

        public GameObject slotItem;


        [Header("Animation Settings")]
        private float startYPosition = 400f;   // 떨어질 Y위치 (시작점)
        private float endYPosition = -400f;    // 지나갈 Y위치 (끝점 - 회전 효과용)
        private float dropDuration = 0.5f;     // 최종 드롭 시간 (바운스 포함)
        private float spinSpeed = 0.15f;        // 회전 시 아이템 하나가 지나가는 속도

        // 회전 애니메이션용 트윈 저장
        private Tween spinTween;

        private Tween glitchSpinTween;

        // 슬롯이 돌아가기 시작할 때 태그를 초기화하는 함수
        public void ResetTag()
        {
            if (tagCanvasGroup != null)
                tagCanvasGroup.alpha = 0f;
        }
        // 아이템 확정 시 태그를 업데이트하고 보여주는 함수
        public void ShowTag(Define.RareType rareType)
        {
            if (tagImage == null || tagCanvasGroup == null) return;

            // 1. 등급에 따른 스프라이트 결정
            Sprite targetSprite = normalTag;
            switch (rareType)
            {
                case Define.RareType.Normal: targetSprite = normalTag; break;
                case Define.RareType.Rare: targetSprite = rareTag; break;
                case Define.RareType.Epic: targetSprite = epicTag; break;
                case Define.RareType.Legend: targetSprite = legendTag; break;
            }

            // 2. 이미지 및 텍스트 갱신
            tagImage.sprite = targetSprite;
            tagImage.SetNativeSize();
            if (tagText != null)
            {
                tagText.text = rareType.ToString().ToUpper(); // 예: "EPIC"
                                                              // 등급에 따라 텍스트 색상을 변경하고 싶다면 여기서 추가 가능
            }

            // 3. CanvasGroup Fade In 연출 (0.3초)
            tagCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
        }


        /// <summary>
        /// 슬롯 회전을 시작합니다 (결과가 나오기 전 대기 상태).
        /// </summary>
        public void StartSpinning(int[] probList)
        {
            slotItem.SetActive(true);
            RectTransform rt = slotItem.GetComponent<RectTransform>();

            // 스핀 시작 시 이전 하이라이트 이펙트 제거
            if (animController != null) animController.ClearHighlight(slotIndex);

            // 기존 애니메이션 제거
            rt.DOKill();

            // 위치 초기화
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

            // 무한 반복 회전 애니메이션
            // 위에서 아래로 빠르게 이동하며, 한 사이클이 끝나면 다시 위로 올리고 이미지를 바꿉니다.
            spinTween = rt.DOAnchorPosY(endYPosition, spinSpeed)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart) // 무한 반복
                .OnStepComplete(() =>
                {
                    // 한 바퀴 돌 때마다(Step 완료 시) 랜덤 아이템 이미지를 잠깐 보여줌
                    // 실제로 아이템 객체를 바꾸진 않고 이미지만 교체하여 시각적 효과만 줍니다.
                    Item randomTempItem = Managers.Item.GetRandomItem(probList);
                    SetData(randomTempItem);
                });
        }

        /// <summary>
        /// 회전을 멈추고 최종 아이템을 보여줍니다.
        /// </summary>
        public void SettingSlot(int[] probList, Item itemtmp)
        {
            // 슬롯 아이템이 활성화 상태인지 확인
            item = itemtmp;
            slotItem.SetActive(true);

            RectTransform rt = slotItem.GetComponent<RectTransform>();

            // 1. 이전 애니메이션(회전 루프)을 모두 중지하고 시작 위치로 리셋
            if (spinTween != null) spinTween.Kill();
            rt.DOKill();

            // 회전 중 이미지가 바뀌었을 수 있으므로, 최종 확정된 아이템 데이터로 세팅
            SetData(itemtmp);

            // 확정된 아이템이 위에서 툭 떨어지는 연출 시작
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);
            // 낙하 애니메이션 실행 후, OnComplete에서 배경 하이라이트 이펙트 트리거
            rt.DOAnchorPosY(0, dropDuration)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    if (animController != null)
                    {
                        animController.PlayHighlightEffect(slotIndex, itemtmp);
                    }
                });
        }




        #region tool
        /// <summary>
        /// 슬롯의 이미지를 갱신합니다. (레전드는 기본적으로 잭팟 아이콘으로 가립니다)
        /// </summary>
        public void SetData(Item findItem, bool forceReveal = false)
        {
            if (findItem == null) return;
            if (animController == null)
            {
                Debug.LogError("SlotMachineAnimation 참조가 Slot에 할당되지 않았습니다.");
                return;
            }

            Sprite targetSprite = null;

            // 🔥 1. 레전드 등급인데 아직 까기 전(Reveal 전)이라면 무조건 잭팟(6) 아이콘으로 덮어씌움
            if (findItem.rareType == Define.RareType.Legend && !forceReveal)
            {
                // 인스펙터에 등록한 6 아이콘(또는 잭팟 아이콘)
                targetSprite = animController._legend_6_Icon;
            }
            else
            {
                // 일반 아이템이거나, 레전드인데 이제 돌아서 까서 보여주는 경우 진짜 아이콘 적용
                targetSprite = animController.GetItemSprite(findItem);
            }

            if (targetSprite != null)
            {
                itemIconImage.sprite = targetSprite;
                itemIconImage.color = Color.white;
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.color = Color.clear;
                Debug.LogWarning($"[Slot] {findItem.itemName}에 맞는 이미지를 찾을 수 없습니다.");
            }
            itemIconImage.SetNativeSize();
        }

        /// <summary>
        /// 전설(6/잭팟) 아이콘이 실제 보상 아이콘으로 좌우 회전하며 공개됨
        /// </summary>
        public async UniTask RevealLegendItem()
        {
            // 1. 좌측으로 90도 회전해서 숨김
            await transform.DORotate(new Vector3(0, 90, 0), 0.2f).SetEase(Ease.InQuad).ToUniTask();

            // 🔥 2. 이제 숨김 해제(forceReveal = true)하여 진짜 아이템 스프라이트로 교체
            SetData(this.item, true);

            // 3. 반대편에서 나타나며 0도로 복귀
            transform.rotation = Quaternion.Euler(0, -90, 0);
            await transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.OutQuad).ToUniTask();
        }

        /// <summary>
        /// 잭팟 연출 중 낮은 등급에서 전설(6)로 강제 변경될 때 호출
        /// </summary>
        public void ForceChangeToLegend(Item trueLegendItem)
        {
            RectTransform rt = slotItem.GetComponent<RectTransform>();
            if (glitchSpinTween != null) glitchSpinTween.Kill();
            DOTween.Kill(rt);

            this.item = trueLegendItem;

            // 🔥 잭팟으로 강제 변환될 때도 마스킹 처리된 SetData 사용
            SetData(trueLegendItem, false);

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

            rt.DOAnchorPosY(0, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (animController != null) animController.PlayHighlightEffect(slotIndex, trueLegendItem);
                ShowTag(Define.RareType.Legend);
            });
        }

        #endregion

        /// <summary>
        /// 역전 연출 시 강제로 초고속 회전하는 애니메이션 (Glitch 효과)
        /// </summary>
        public void StartFastGlitchSpin(int[] probList)
        {
            RectTransform rt = slotItem.GetComponent<RectTransform>();

            if (animController != null) animController.ClearHighlight(slotIndex);
            ResetTag();

            //  이 오브젝트에서 돌아가고 있는 모든 DOTween 애니메이션 완벽하게 강제 종료
            if (spinTween != null) spinTween.Kill();
            if (glitchSpinTween != null) glitchSpinTween.Kill();
            DOTween.Kill(rt);

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

            glitchSpinTween = rt.DOAnchorPosY(endYPosition, 0.08f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() =>
                {
                    Item randomTempItem = Managers.Item.GetRandomItem(probList);
                    SetData(randomTempItem);
                });
        }

        /// <summary>
        /// 초고속 회전을 멈추고 지정된 아이템(페이크 시 원본 아이템)으로 강제 고정합니다.
        /// </summary>
        public void StopGlitchSpin(Item originalItem)
        {
            RectTransform rt = slotItem.GetComponent<RectTransform>();

            // 확실한 애니메이션 킬 (무한 루프 차단)
            if (glitchSpinTween != null) glitchSpinTween.Kill();
            DOTween.Kill(rt);

            this.item = originalItem;
            SetData(originalItem);

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

            //  위아래로 심하게 요동치던 OutBounce 제거 -> 살짝만 반동을 주고 즉시 멈추는 OutBack 적용 (시간도 0.2초로 짧고 굵게)
            rt.DOAnchorPosY(0, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (animController != null) animController.PlayHighlightEffect(slotIndex, originalItem);
                ShowTag(originalItem.rareType);
            });
        }

        #region 호버 관련
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item == null) return;
            animController.OnHoverEnter(slotIndex, item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            animController.OnHoverExit(slotIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            animController.OnClickSlot(slotIndex, item);

            // 2. 실제 데이터 캐싱 (SlotMachine 매니저에게 선택된 아이템 전달)
            if (slotMachineManager != null)
            {
                slotMachineManager.SetSelectedItem(this.item);
            }
        }

        #endregion
    }
}