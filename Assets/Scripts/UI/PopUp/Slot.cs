using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems;
using Cardevil.Gameplay.Items;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cardevil.Core.Utils;

namespace Cardevil.UI.PopUp
{
    public class Slot : MonoBehaviour
    {

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

        public void SetData(Item findItem)
        {
            if (findItem == null) return;
            if (animController == null)
            {
                Debug.LogError("SlotMachineAnimation 참조가 Slot에 할당되지 않았습니다.");
                return;
            }

            // [변경점] DB에서 찾지 않고, SlotMachineAnimation에 할당된 스프라이트를 직접 가져옴
            Sprite targetSprite = animController.GetItemSprite(findItem);

            if (targetSprite != null)
            {
                itemIconImage.sprite = targetSprite;
                itemIconImage.color = Color.white;
            }
            else
            {
                // 할당된게 없을 경우 투명 처리
                itemIconImage.sprite = null;
                itemIconImage.color = Color.clear;
                Debug.LogWarning($"[Slot] {findItem.itemName}에 맞는 이미지를 SlotMachineAnimation에서 찾을 수 없습니다.");
            }
            itemIconImage.SetNativeSize();
        }
        /// <summary>
        /// 아이템의 등급과 이름을 분석하여 DB에 등록된 실제 '이미지 이름(Key)'을 반환합니다.
        /// </summary>
        private string GetImageNameKey(Item item)
        {
            // DB(Resources 또는 어드레서블 등)에 등록된 실제 스프라이트 파일명이나 Key값과 
            // 정확히 일치하는 문자열을 반환하도록 리턴값을 수정해 주세요.

            switch (item.rareType)
            {
                case Define.RareType.Legend:
                    if (item.itemName.Contains("Exact")) return "Legend_Exact_Upgrade"; // 예: DB에 있는 실제 이미지 이름
                    if (item.itemName.Contains("JackPot")) return "Legend_Jack_Pot";
                    if (item.itemName.Contains("Locked")) return "Legend_Relic_Locked";
                    if (item.itemName.Contains("Unlocked")) return "Legend_Relic_Unlocked";
                    break;

                case Define.RareType.Epic:
                    if (item.itemName.Contains("DarkUpgrade2")) return "Epic_Dark_Upgrade_2";
                    if (item.itemName.Contains("DarkUpgrade3")) return "Epic_Dark_Upgrade_3";
                    if (item.itemName.Contains("Gold")) return "Epic_Gold";
                    if (item.itemName.Contains("Reroll")) return "Epic_Start_Reroll";
                    break;

                case Define.RareType.Rare:
                    if (item.itemName.Contains("DarkUpgrade2")) return "Rare_Dark_Upgrade_2";
                    if (item.itemName.Contains("DarkUpgrade3")) return "Rare_Dark_Upgrade_3";
                    if (item.itemName.Contains("Gold")) return "Rare_Gold";
                    if (item.itemName.Contains("Heal")) return "Rare_Heal";
                    if (item.itemName.Contains("Reroll")) return "Rare_Start_Reroll";
                    break;
            }

            // 매칭되는 조건이 없을 경우 기본적으로 아이템 이름을 그대로 Key로 사용 시도
            return item.itemName;
        }
        #endregion
    }
}