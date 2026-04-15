using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Cardevil.Gameplay.Items; // Item 클래스 참조용
using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;


namespace Cardevil.UI.PopUp
{
    public class SlotMachineAnimation : MonoBehaviour
    {
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
            if(highlightImg.sprite==null) // Normal이라 없을때
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
        }

    }


}