using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Cardevil.Gameplay.Items; // Item 클래스 참조용
using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;
using TMPro;
using Database.Generated;
using Cardevil.Gameplay;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using Database;
using Cardevil.UI.VFX;


namespace Cardevil.UI.PopUp
{
    public class SlotMachineAnimation : MonoBehaviour
    {
        private Item _selectedItem = null;

        [Header("VFX Connect")]
        [SerializeField] private CoinRainManager _coinRainManager;

        [Header("Connects")]
        [SerializeField] private RectTransform _slotmachine_rectTransform;
        [SerializeField] private Image _reroll_image;
        [SerializeField] private float offScreenY = -1500f; // 퇴장 시 내려갈 Y 좌표
        [SerializeField] private float onScreenY = 0f;    // 등장 시 목표 Y 좌표

        // ==========================================
        // 하이라이트(Blur)용 스프라이트 13종
        // ==========================================
        [Header("Highlight Sprites - Legend")]
        [SerializeField] public Sprite _legend_Exact_Upgrade_Highlight;
        [SerializeField] public Sprite _legend_Jack_Pot_Highlight;
        [SerializeField] public Sprite _legend_Relic_Locked_Highlight;
        [SerializeField] public Sprite _legend_Relic_Unlocked_Highlight;

        [Header("Highlight Sprites - Epic")]
        [SerializeField] public Sprite _epic_Dark_Upgrade_2_Highlight;
        [SerializeField] public Sprite _epic_Dark_Upgrade_3_Highlight;
        [SerializeField] public Sprite _epic_Gold_Highlight;
        [SerializeField] public Sprite _epic_Start_Reroll_Highlight;

        [Header("Highlight Sprites - Rare (Normal)")]
        [SerializeField] public Sprite _rare_Dark_Upgrade_2_Highlight;
        [SerializeField] public Sprite _rare_Dark_Upgrade_3_Highlight;
        [SerializeField] public Sprite _rare_Gold_Highlight;
        [SerializeField] public Sprite _rare_Heal_Highlight;
        [SerializeField] public Sprite _rare_Start_Reroll_Highlight;


        [Header("HighLight Sprites (배경 이미지 배열)")]
        [SerializeField] private List<Image> _slotHighLight_Images = new List<Image>();


        [Header("Item Icons (아이템 본체 이미지)")]
        [SerializeField] public Sprite _legend_Exact_Upgrade;
        [SerializeField] public Sprite _legend_Jack_Pot;
        [SerializeField] public Sprite _legend_Relic;
        [SerializeField] public Sprite _normal_Dark_Upgrade_2;
        [SerializeField] public Sprite _normal_Dark_Upgrade_3;
        [SerializeField] public Sprite _normal_Gold;
        [SerializeField] public Sprite _normal_Heal;
        [SerializeField] public Sprite _normal_Start_Reroll;



        [Header("Legend 6 Placeholder")]
        [SerializeField] public Sprite _legend_6_Icon; // 전설 등급이 뜨기 전 보여줄 6 이미지


        [Header("Jackpot Effects")]
        [SerializeField] private GameObject _jackpotAlertBorder; // 붉은색 경고등 테두리 (사전 연출용)
        [SerializeField] private GameObject _jackpotSuccessBorder; // 황금색 빛나는 테두리
        [SerializeField] private GameObject _celebrationLayer; // 골드 떨어지는 파티클
        [SerializeField] private Image _flashActiveBackground; // 붉은색 번쩍이는 배경

        [Header("아이템 클릭 및 하이라이트 호버 애니메이션 관련")]
        [SerializeField] private RectTransform _itemPanel_1;
        [SerializeField] private RectTransform _itemPanel_2;
        [SerializeField] private RectTransform _itemPanel_3;

        [SerializeField] private Image _itemClickHover_1;
        [SerializeField] private Image _itemClickHover_2;
        [SerializeField] private Image _itemClickHover_3;

        [SerializeField] private Image _itemTextPanel_1;
        [SerializeField] private Image _itemTextPanel_2;
        [SerializeField] private Image _itemTextPanel_3;

        [SerializeField] private TMP_Text _itemText_1;
        [SerializeField] private TMP_Text _itemText_2;
        [SerializeField] private TMP_Text _itemText_3;

        private bool _canInteract = false; // 슬롯머신이 멈춘 후 true가 됨
        private bool _isJackpot = false;   // 잭팟 여부
        private int _selectedIndex = -1;    // 현재 선택된 슬롯 번호 (0, 1, 2)

        /// <summary>
        /// SetActive된 슬롯머신을 등장시킬때의 애니메이션 
        /// </summary>
        public void SlotMachine_GetUpAnimation()
        {
            // 초기 위치를 화면 아래로 설정 후 띠용(OutBack)하며 올라옴
            _slotmachine_rectTransform.anchoredPosition = new Vector2(_slotmachine_rectTransform.anchoredPosition.x, offScreenY);
            _slotmachine_rectTransform.DOAnchorPosY(onScreenY, 0.6f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        /// <summary>
        /// 슬롯머신을 아래로 퇴장시킬때 애니메이션
        /// </summary>
        public async UniTask SlotMachine_GetDownAnimation(System.Action onCompleteCallback)
        {
            // 아래로 띠용(InBack)하면서 내려간 후 콜백으로 UI 비활성화
            await _slotmachine_rectTransform.DOAnchorPosY(offScreenY, 0.5f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => onCompleteCallback?.Invoke());

        }

        /// <summary>
        /// 슬롯이 회전을 시작할 때 기존 하이라이트 이펙트를 초기화합니다.
        /// </summary>
        public void ClearHighlight(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotHighLight_Images.Count) return;

            Image highlightImg = _slotHighLight_Images[slotIndex];
            highlightImg.DOKill(); // 기존 DOTween 제거
            highlightImg.color = new Color(1, 1, 1, 0); // 투명하게 숨김
            highlightImg.gameObject.SetActive(false);
        }

        /// <summary>
        /// 슬롯이 추락 완료된 후 호출되어 배경 하이라이트 깜빡임 이펙트를 실행합니다.
        /// </summary>
        public void PlayHighlightEffect(int slotIndex, Item item)
        {
            if (slotIndex < 0 || slotIndex >= _slotHighLight_Images.Count || item == null) return;

            Image highlightImg = _slotHighLight_Images[slotIndex];

            // 1. 아이템 정보에 맞는 하이라이트 스프라이트 설정
            highlightImg.sprite = GetMatchingBlurSprite(item);
            highlightImg.gameObject.SetActive(true);
            if (highlightImg.sprite == null) // Normal이라 없을때
            {
                highlightImg.gameObject.SetActive(false);
            }

            // 2. 깜빡깜빡 이펙트 (알파값 0 -> 1 등장 후 0.4 ~ 1 사이를 Yoyo 루프)
            highlightImg.DOKill();
            highlightImg.color = new Color(1, 1, 1, 0);

            Sequence seq = DOTween.Sequence();
            seq.Append(highlightImg.DOFade(1f, 0.3f)); // 처음에 선명하게 등장
            seq.Append(highlightImg.DOFade(0.4f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)); // 이후 깜빡임
            highlightImg.SetNativeSize();
        }


        /// <summary>
        /// 아이템의 등급과 이름을 분석하여 SlotMachineAnimation에 할당된 '아이템 스프라이트'를 반환합니다.
        /// </summary>
        public Sprite GetItemSprite(Item item)
        {
            if (item == null) return null;
            // 등급(RareType)과 이름(itemName)을 조합하여 매칭
            switch (item.rareType)
            {
                case Define.RareType.Legend:
                    if (item.itemName.Contains("Exact")) return _legend_Exact_Upgrade;
                    if (item.itemName.Contains("JackPot")) return _legend_Jack_Pot;
                    if (item.itemName.Contains("Relic")) return _legend_Relic;
                    break;

                default: // Normal(Rare) 및 기타
                    // TODO : DarkUpgrade 3도 나오게끔 이의 파싱방향을 점검할 필요성있음.
                    if (item.itemName.Contains("DarkUpgrade")) return _normal_Dark_Upgrade_2;
                    if (item.itemName.Contains("DarkUpgrade")) return _normal_Dark_Upgrade_3;
                    if (item.itemName.Contains("Gold")) return _normal_Gold;
                    if (item.itemName.Contains("Heal")) return _normal_Heal;
                    if (item.itemName.Contains("Reroll")) return _normal_Start_Reroll;
                    break;
            }

            return null;
        }

        /// <summary>
        /// Item의 데이터(등급, 이름)를 확인하여 알맞은 Highlight(Blur) 이미지를 반환합니다.
        /// </summary>
        private Sprite GetMatchingBlurSprite(Item item)
        {
            if (item == null) return null;

            switch (item.rareType)
            {
                case Define.RareType.Legend:
                    if (item.itemName.Contains("Exact")) return _legend_Exact_Upgrade_Highlight;
                    if (item.itemName.Contains("JackPot")) return _legend_Jack_Pot_Highlight;
                    if (item.itemName.Contains("Locked")) return _legend_Relic_Locked_Highlight;
                    if (item.itemName.Contains("Unlocked")) return _legend_Relic_Unlocked_Highlight;
                    break;

                case Define.RareType.Epic:
                    if (item.itemName.Contains("DarkUpgrade")) return _epic_Dark_Upgrade_2_Highlight;
                    if (item.itemName.Contains("DarkUpgrade")) return _epic_Dark_Upgrade_3_Highlight;
                    if (item.itemName.Contains("Gold")) return _epic_Gold_Highlight;
                    if (item.itemName.Contains("Reroll")) return _epic_Start_Reroll_Highlight;
                    break;

                case Define.RareType.Rare:
                    if (item.itemName.Contains("DarkUpgrade")) return _rare_Dark_Upgrade_2_Highlight;
                    if (item.itemName.Contains("DarkUpgrade")) return _rare_Dark_Upgrade_3_Highlight;
                    if (item.itemName.Contains("Gold")) return _rare_Gold_Highlight;
                    if (item.itemName.Contains("Heal")) return _rare_Heal_Highlight;
                    if (item.itemName.Contains("Reroll")) return _rare_Start_Reroll_Highlight;
                    break;
                case Define.RareType.Normal:
                    return null;
                    break;
            }

            Debug.LogWarning($"[Highlight] 매칭되는 Highlight 이미지가 없습니다! ItemName: {item.itemName}, RareType: {item.rareType}");
            return _rare_Gold_Highlight; // 기본값
        }



        /// <summary>
        /// 잭팟 기회 사전 연출 (붉은색 경고등 깜빡임)
        /// </summary>
        public void SetJackpotAlertMode(bool isOn)
        {
            if (_jackpotAlertBorder != null)
            {
                _jackpotAlertBorder.SetActive(isOn);
                // DOTween 등으로 깜빡이는 효과를 주면 더욱 좋습니다.
            }
        }

        /// <summary>
        /// 잭팟 성공 연출 (황금 테두리, 배경 플래시, 파티클)
        /// </summary>
        public void PlayJackpotSuccessEffect()
        {
            if (_jackpotAlertBorder != null) _jackpotAlertBorder.SetActive(false);

            // 황금색 테두리 켜기
            if (_jackpotSuccessBorder != null) _jackpotSuccessBorder.SetActive(true);

            // 파티클 (50개 골드 회전하며 떨어짐) 켜기
            if (_celebrationLayer != null) _celebrationLayer.SetActive(true);

            // 0.5초 주기로 붉은색 번쩍임 3초 유지 후 자연스럽게 사라짐
            if (_flashActiveBackground != null)
            {
                _flashActiveBackground.gameObject.SetActive(true);
                _flashActiveBackground.color = new Color(1, 0, 0, 0); // 투명한 빨강

                Sequence flashSeq = DOTween.Sequence();
                flashSeq.Append(_flashActiveBackground.DOFade(0.3f, 0.25f))
                        .Append(_flashActiveBackground.DOFade(0f, 0.25f))
                        .SetLoops(6, LoopType.Restart) // 0.5초 x 6회 = 3초
                        .OnComplete(() => _flashActiveBackground.gameObject.SetActive(false));
            }

            _coinRainManager.PlayJackpotEffect(5.0f);
        }


        #region 슬롯머신 호버 앤 클릭 아이템 정보 관련

        public void InitializeInteraction(bool isJackpot)
        {
            _canInteract = false;
            _isJackpot = isJackpot;
            _selectedIndex = -1;
            _selectedItem = null;

            // 모든 UI 초기화
            ResetAllUI();
        }

        public void SetInteractable(bool value)
        {
            _canInteract = value;
        }

        private void ResetAllUI()
        {
            // 모든 패널 스케일 1, 텍스트 패널 투명도 0, 클릭 하이라이트 오프
            ResetSlotUI(1, _itemPanel_1, _itemTextPanel_1, _itemClickHover_1);
            ResetSlotUI(2, _itemPanel_2, _itemTextPanel_2, _itemClickHover_2);
            ResetSlotUI(3, _itemPanel_3, _itemTextPanel_3, _itemClickHover_3);
        }

        private void ResetSlotUI(int index, RectTransform panel, Image textPanel, Image clickHover)
        {
            panel.DOKill();
            panel.localScale = Vector3.one;

            textPanel.DOKill();
            textPanel.DOFade(0, 0); // CanvasGroup이 있다면 alpha, Image라면 color.a 조절 (여기선 Image로 되어있어 CanvasGroup 권장)
            textPanel.gameObject.SetActive(false);

            clickHover.gameObject.SetActive(false);
        }

        // ==========================================
        // 호버 액션 (Slot.cs에서 호출)
        // ==========================================
        public void OnHoverEnter(int index, Item item)
        {
            if (!_canInteract || _selectedIndex == index) return;

            RectTransform targetPanel = GetPanel(index);
            Image targetTextPanel = GetTextPanel(index);
            TMP_Text targetText = GetText(index);

            // 1. 스케일 키우기 (1.15f)
            targetPanel.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack);

            // 2. 텍스트 설정 및 보이기 (살짝 딜레이)
            targetText.text = item.itemName;
            targetTextPanel.gameObject.SetActive(true);
            targetTextPanel.DOKill();
            targetTextPanel.DOFade(1f, 0.3f).From(0f).SetDelay(0.1f);
        }

        public void OnHoverExit(int index)
        {
            if (!_canInteract || _selectedIndex == index) return;

            RectTransform targetPanel = GetPanel(index);
            Image targetTextPanel = GetTextPanel(index);

            targetPanel.DOScale(1.0f, 0.2f);
            targetTextPanel.DOFade(0f, 0.2f).OnComplete(() => targetTextPanel.gameObject.SetActive(false));
        }

        // ==========================================
        // 클릭 액션 (Slot.cs에서 호출)
        // ==========================================
        public void OnClickSlot(int index, Item clickedItem)
        {
            // 잭팟 상황이면 클릭 자체가 아예 안 먹히게 처리
            if (!_canInteract || _isJackpot) return;

            // 이미 선택된 거라면 리턴
            if (_selectedIndex == index) return;

            // 이전 선택 해제
            if (_selectedIndex != -1)
            {
                DeselectSlot(_selectedIndex);
            }

            // 새로운 선택
            _selectedIndex = index;
            _selectedItem = clickedItem;

            RectTransform targetPanel = GetPanel(index);
            Image clickHover = GetClickHover(index);
            Image targetTextPanel = GetTextPanel(index);

            targetPanel.DOKill();
            targetPanel.localScale = Vector3.one; // 스케일 고정

            clickHover.gameObject.SetActive(true); // 클릭 하이라이트 온
            clickHover.DOFade(1, 0);
            targetTextPanel.gameObject.SetActive(true);
            targetTextPanel.color = new Color(targetTextPanel.color.r, targetTextPanel.color.g, targetTextPanel.color.b, 1f);
        }
        public void OnConfirmButtonClicked()
        {
            if (_selectedItem == null)
            {
                Debug.LogWarning("아이템이 선택되지 않았습니다.");
                return;
            }

            // 캐싱해둔 아이템의 보상 데이터를 PlayerStatus에 적용
            ApplyRewardToPlayerStatus(_selectedItem.macinRewardData);

            // TODO: 보상 획득 완료 후 슬롯머신 UI 닫기 연출 등을 호출하세요.
            // 예: SlotMachine_GetDownAnimation(() => gameObject.SetActive(false));
        }

        // [추가] 아이템 데이터를 읽어 PlayerStatus 스탯에 적용하는 로직
        private void ApplyRewardToPlayerStatus(MachineReward rewardData)
        {
            if (rewardData == null) return;

            // Define.SlotRewardType 값에 따라 PlayerStatus 스탯을 증가시킵니다.
            // (보상 타입명은 프로젝트 내부 열거형(Define.SlotRewardType) 기준에 맞춰 매칭하시면 됩니다.)
            switch (rewardData.ItemName.ToString())
            {
                case "Gold":
                    CardevilCore.PlayerStatus.ModifyBaseValue(StatType.Gold, rewardData.Value);
                    break;
                case "Heal":
                    CardevilCore.PlayerStatus.Heal(rewardData.Value); // 체력은 Heal 메서드 사용
                    break;
                case "MaxHp":
                    CardevilCore.PlayerStatus.ModifyBaseValue(StatType.MaxHp, rewardData.Value);
                    break;
                case "Shield":
                    CardevilCore.PlayerStatus.ModifyBaseValue(StatType.Shield, rewardData.Value);
                    break;
                case "RerollTicket":
                    CardevilCore.PlayerStatus.ModifyBaseValue(StatType.RerollTicket, rewardData.Value);
                    break;
                // 업그레이드나 유물 같은 특수 처리가 필요한 경우
                case "Relic":
                case "DarkUpgrade":
                    // 유물 획득 시스템이나 업그레이드 시스템 호출 로직 작성
                    Debug.Log($"[{rewardData.ItemName}] 유물/업그레이드 획득 처리");
                    break;
                default:
                    Debug.LogWarning($"[{rewardData.ItemName}] 처리되지 않은 보상 타입입니다.");
                    break;
            }

            Debug.Log($"보상 획득 완료: {rewardData.ItemName} (Value: {rewardData.Value})");
        }


        private void DeselectSlot(int index)
        {
            GetClickHover(index).gameObject.SetActive(false);
            OnHoverExit(index);
        }

        // 유틸리티 메서드들
        private RectTransform GetPanel(int i) => i == 0 ? _itemPanel_1 : (i == 1 ? _itemPanel_2 : _itemPanel_3);
        private Image GetTextPanel(int i) => i == 0 ? _itemTextPanel_1 : (i == 1 ? _itemTextPanel_2 : _itemTextPanel_3);
        private TMP_Text GetText(int i) => i == 0 ? _itemText_1 : (i == 1 ? _itemText_2 : _itemText_3);
        private Image GetClickHover(int i) => i == 0 ? _itemClickHover_1 : (i == 1 ? _itemClickHover_2 : _itemClickHover_3);
        #endregion


    }



}

