using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems;
using Cardevil.Core.Utils;
using Cardevil.Gameplay;
using Cardevil.Gameplay.Items;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cardevil.Core.Bootstrap;
using Database;
using Database.Generated;

namespace Cardevil.UI.PopUp
{
    public class SlotMachine : UI_Popup
    {

        // 슬롯머신 끝났을때의 액션 접근
        public event Action OnSlotMachineClear;

        // 확대 설정을 위해
        [SerializeField]
        private Camera _renderCamera;
        private Vector3 _originCamPos;
        private float _originCamSize;
        private Tween _cameraTween; // 중복 실행 방지 및 관리용
        private int slotMachineLevel;

        //----
        [Header("Animation Controller")]
        [SerializeField] private SlotMachineAnimation _animationController; 

        [Header("UI 설정")]
        public GameObject probabilityPanel;
        [Tooltip("각 박스 이미지 사이의 간격(픽셀)")]
        public float spacing = 10f;

        [Header("비율을 계산할 수치 데이터")]
        public List<int> values = new List<int> { 75, 50, 125, 250 };

        [Header("크기를 조절할 UI 이미지들")]
        public List<Image> boxImages;

        [Header("아이템이 나올 확률")]
        public int[] probalityList = new int[] { 30, 20, 10, 5 };

        public List<Slot> slots;

        private bool isSetting = false;

        [Header("카메라 액션 관련")]
        [SerializeField] private float dropTiming = 0.8f;
        [SerializeField] private float zoomInTime = 2f;

        // 캐싱용 시각 효과 컨트롤러
        private SlotButtonVisual _rerollVisual;
        private SlotButtonVisual _upgradeVisual;
        private SlotButtonVisual _selectVisual;

        [Header("Probability UI (일반, 레어, 에픽, 잭팟 순서)")]
        [Tooltip("각 색상 바(Image)에 LayoutElement 컴포넌트가 붙어있어야 합니다.")]
        [SerializeField] private List<LayoutElement> _probabilityBars = new List<LayoutElement>();
        void Start()
        {
            Init();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnRerollClicked(null);
            }

        }

        /// <summary>
        /// 슬롯머신 호출
        /// </summary>
        public void ActiveSlotMachine()
        {
            this.gameObject.SetActive(true);
            _animationController.SlotMachine_GetUpAnimation();
            UpdateLayout();

            // UI 활성화 시 등장 애니메이션 재생
            if (_animationController != null)
            {
                _animationController.SlotMachine_GetUpAnimation();
            }

            // 슬롯 초기 세팅 (인덱스와 애니메이션 컨트롤러 주입)
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].slotIndex = i;
                slots[i].animController = _animationController;
            }
        }

        public override void Init()
        {

            Bind<Button>(typeof(ItemButtons));

            GetButton((int)ItemButtons.Item_1).gameObject.AddUIEvent(OnItem1Clicked);
            GetButton((int)ItemButtons.Item_2).gameObject.AddUIEvent(OnItem2Clicked);
            GetButton((int)ItemButtons.Item_3).gameObject.AddUIEvent(OnItem3Clicked);
            GetButton((int)ItemButtons.Reroll).gameObject.AddUIEvent(OnRerollClicked);
            GetButton((int)ItemButtons.Select).gameObject.AddUIEvent(OnSelectClicked);
            GetButton((int)ItemButtons.Upgrade).gameObject.AddUIEvent(OnUpGradeClicked);

            // 초기화 시 컴포넌트 캐싱
            _rerollVisual = GetButton((int)ItemButtons.Reroll).GetComponent<SlotButtonVisual>();
            _upgradeVisual = GetButton((int)ItemButtons.Upgrade).GetComponent<SlotButtonVisual>();
            _selectVisual = GetButton((int)ItemButtons.Select).GetComponent<SlotButtonVisual>();


            if (_renderCamera != null)
            {
                _originCamPos = _renderCamera.transform.position;
                _originCamSize = _renderCamera.orthographicSize;

            }
            // 슬롯머신 레벨 index 벗어남 처리
            slotMachineLevel = Math.Min(CardevilCore.PlayerStatus.GetFinalValue(PlayerStatType.SlotMachineLevel), CardevilCore.Database.Database.MachineProbabillityList.Count);
            // machineLevel을 통한 probalityList받기
            probalityList = CardevilCore.Database.Database.MachineProbabillityList[slotMachineLevel - 1].RankWeight.ToArray();


            // 레이아웃 업데이트
            UpdateLayout();

        }


        #region Slot에 관련

        /// <summary>
        /// 슬롯머신이 돌아갑니다.
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid SettingSlots()
        {
            if (isSetting) { return; }
            isSetting = true;

            // 리롤 시작: Reroll 버튼 상태를 "구동 중(Rolling)"으로 고정
            if (_rerollVisual != null) _rerollVisual.SetRolling(true);

            // 안전장치: 도중에 에러가 나거나 객체가 파괴되어도 isSetting을 해제하기 위해 try-finally 사용
            try
            {
                foreach (var slot in slots)
                {
                    // SettingSlot이 설정되기 까지 무한대로 돌아갑니다.
                    slot.StartSpinning(probalityList);
                }   

                // 1.5초 대기 (Realtime)
                // CancellationToken을 넣어주면 씬 이동 등으로 오브젝트 파괴 시 에러 방지
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

                // 미리 아이템을 결정합니다.
                List<Item> preDeterminedItemRandomList = new List<Item>();
                for (int i = 0; i < 3; i++)
                {
                    preDeterminedItemRandomList.Add(SettingItem(probalityList));
                }

                // 레어한 순서대로 정렬
                preDeterminedItemRandomList = preDeterminedItemRandomList
                    .OrderBy(item => (int)item.rareType)
                    .ToList();

                int count = 0;
                foreach (var slot in slots)
                {
                    Item item = preDeterminedItemRandomList[count];
                    int typeNumber = (((int)item.rareType) + 1);
                    RectTransform targetRect = slot.GetComponent<RectTransform>();

                    // CameraAction 호출 (콜백은 그대로 유지)
                    Tween cameraTween = CameraAction(typeNumber, targetRect, () =>
                    {
                        // 이 코드는 카메라가 줌인된 상태(Sequence 중간)에서 실행됨
                        slot.SettingSlot(probalityList, item);
                    });

                    if (cameraTween != null)
                    {
                        // DOTween을 UniTask로 대기 (Awaiter)
                        await cameraTween.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

                        // 0.2초 대기 (일반 시간 기준)
                        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: this.GetCancellationTokenOnDestroy());
                    }
                    else
                    {
                        // 카메라 액션이 없는 경우(Common 등) 바로 실행
                        // dropTiming * typeNumber 만큼 대기 (Realtime 기준)
                        float delayTime = dropTiming * typeNumber;
                        await UniTask.Delay(TimeSpan.FromSeconds(delayTime), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

                        slot.SettingSlot(probalityList, item);
                    }
                    count++;
                }
            }
            finally
            {
                // 로직 완료 또는 취소 시 반드시 실행됨
                isSetting = false;

                // 리롤 종료: Reroll 버튼 상태 원상복구
                if (_rerollVisual != null) _rerollVisual.SetRolling(false);
            }
        }
        #endregion

        #region Probability

        public void UpdateLayout()
        {
            // 유효성 검사
            if (boxImages == null || boxImages.Count == 0 || values.Count == 0) return;

            values = probalityList.ToList();
            RectTransform containerRect = probabilityPanel.GetComponent<RectTransform>();
            float containerWidth = containerRect.rect.width;

            float totalValue = values.Sum();
            if (totalValue <= 0) return;

            int gapCount = boxImages.Count > 1 ? boxImages.Count - 1 : 0;
            float totalSpacing = spacing * gapCount;
            float widthForBoxes = containerWidth - totalSpacing;

            float currentX = 0f;

            for (int i = 0; i < boxImages.Count; i++)
            {
                if (i >= values.Count) break;

                RectTransform boxRect = boxImages[i].GetComponent<RectTransform>();

                float proportion = values[i] / totalValue;
                float boxWidth = widthForBoxes * proportion;

                boxRect.anchorMin = new Vector2(0, 0.5f);
                boxRect.anchorMax = new Vector2(0, 0.5f);
                boxRect.pivot = new Vector2(0, 0.5f);

                boxRect.anchoredPosition = new Vector2(currentX, 0);
                boxRect.sizeDelta = new Vector2(boxWidth, containerRect.rect.height);

                currentX += boxWidth + spacing;
            }
        }

        #endregion

        #region OnClicked
        enum ItemButtons
        {
            Item_1, Item_2, Item_3, Reroll, Upgrade, Select
        }

        private void OnRerollClicked(PointerEventData eventData)
        {
            SettingSlots().Forget();
        }
        /// <summary>
        /// 업그레이드 버튼을 클릭했을때
        /// </summary>
        /// <param name="eventData"></param>
        private void OnUpGradeClicked(PointerEventData eventData)
        {
            // 1. 현재 레벨의 머신 데이터 가져오기 (DB 클래스명에 맞게 수정 필요)
            // 예: CardevilCore.Database.MachineProbability.TryGetValue(slotMachineLevel, out var currentData)
            var currentData = GetMachineData(slotMachineLevel);

            if (currentData == null) return;

            // 2. 만렙 체크 (시트를 보면 레벨 6의 LevelUpCost가 비어있으므로 0이거나 없을 때 만렙 처리)
            if (currentData.LevelUpCost <= 0)
            {
                Debug.Log("최대 레벨입니다!");
                if (_upgradeVisual != null) _upgradeVisual.SetBlocked(true);
                return;
            }

            int gold = (CardevilCore.PlayerStatus.GetFinalValue(PlayerStatType.Gold));
            // 3. 골드 체크 및 차감 (PlayerStatus는 실제 관리하시는 플레이어 데이터 스크립트로 대체)
            if (gold < currentData.LevelUpCost)
            {
                Debug.Log("골드가 부족합니다!");
                if (_upgradeVisual != null) _upgradeVisual.SetBlocked(true);
                return;
            }


            // 4. 업그레이드 실행
            CardevilCore.PlayerStatus.SetBaseValue(PlayerStatType.Gold, gold - currentData.LevelUpCost);
            slotMachineLevel++;
            CardevilCore.PlayerStatus.SetBaseValue(PlayerStatType.SlotMachineLevel, slotMachineLevel);
            Debug.Log($"슬롯머신 레벨업! 현재 레벨: {slotMachineLevel}");


            if (_upgradeVisual != null) _upgradeVisual.SetBlocked(false);

            // 5. 확률 바 UI 갱신
            UpdateProbabilityPanel();
        }

        private void OnSelectClicked(PointerEventData eventData)
        {
            // TODO: 선택하면 모든 아이템을 획득하는 로직 여기
   
            _animationController.SlotMachine_GetDownAnimation(OnSlotMachineClear).Forget();
        
        }

        private void OnItem1Clicked(PointerEventData eventData) { slots[0].item.OnClicked(); }
        private void OnItem2Clicked(PointerEventData eventData) { slots[1].item.OnClicked(); }
        private void OnItem3Clicked(PointerEventData eventData) { slots[2].item.OnClicked(); }

        #endregion

        #region Tool



        private Tween CameraAction(int index, RectTransform targetRect, System.Action onMiddleAction = null)
        {
            if (_renderCamera == null || targetRect == null) return null;

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 targetCenterPos = (corners[0] + corners[2]) / 2f;

            // 💡 카메라가 이동할 X, Y 좌표 (Z축은 기존 카메라의 Z 유지)
            Vector3 finalCamPos = new Vector3(targetCenterPos.x, targetCenterPos.y, _originCamPos.z);

            // 💡 줌인 될 때의 카메라 사이즈 (숫자가 작을수록 더 크게 확대됨. 필요에 따라 조절하세요)
            float targetOrthoSize = _originCamSize * 0.4f;

            float duration = dropTiming * index;
            Sequence seq = DOTween.Sequence();

            // 연계 전 초기화
            _renderCamera.transform.position = _originCamPos;
            _renderCamera.orthographicSize = _originCamSize;

            // 1. 줌인 (Zoom In) 및 이동 (Pan)
            switch (index)
            {
                case 2:
                case 3:
                    // 위치 이동과 Size 변경을 동시에(Join) 실행합니다.
                    seq.Append(_renderCamera.transform.DOMove(finalCamPos, zoomInTime).SetEase(Ease.OutQuad));
                    seq.Join(_renderCamera.DOOrthoSize(targetOrthoSize, zoomInTime).SetEase(Ease.OutQuad));
                    break;

                case 4:
                    // 4단계는 더 과격하게 줌인하는 연출 (예시)
                    targetOrthoSize = _originCamSize * 0.25f;

                    seq.Append(_renderCamera.transform.DOMove(finalCamPos, zoomInTime).SetEase(Ease.OutCubic));
                    seq.Join(_renderCamera.DOOrthoSize(targetOrthoSize, zoomInTime).SetEase(Ease.OutCubic));

                    // 흔들림 연출 (Z축 흔들림을 빼고 X, Y만 흔들리게 조정하는 것이 좋습니다)
                    seq.Join(_renderCamera.transform.DOShakePosition(duration, new Vector3(0.3f, 0.3f, 0f), 20, 90, false, true).SetDelay(zoomInTime));
                    duration = duration * 2;
                    break;

                default:
                    return null;
            }

            // 2. 머무르는 시간 계산
            float stayTime = Mathf.Max(0f, duration - zoomInTime);
            seq.AppendInterval(stayTime * 0.2f);

            // DataSet 진행
            seq.AppendCallback(() =>
            {
                if (onMiddleAction != null) onMiddleAction.Invoke();
            });

            seq.AppendInterval(stayTime * 0.5f);

            // 3. 복귀 (Zoom Out)
            seq.Append(_renderCamera.transform.DOMove(_originCamPos, 0.4f).SetEase(Ease.OutQuad));
            seq.Join(_renderCamera.DOOrthoSize(_originCamSize, 0.4f).SetEase(Ease.OutQuad));

            _cameraTween = seq;

            
            return seq;
        }

        /// <summary>
        // 랜덤한 아이템을 결정합니다.
        /// </summary>
        /// <param name="probList"></param>
        /// <returns></returns>
        public Item SettingItem(int[] probList)
        {
            return Managers.Item.GetRandomItem(probList);
        }

        #endregion
        #region UI Update

        /// <summary>
        /// 머신 레벨업 시 DB의 RankWeight를 파싱하여 확률 바 UI 비율을 조절합니다.
        /// </summary>
        public void UpdateProbabilityPanel()
        {
            // 현재 레벨의 데이터 다시 가져오기
            var currentData = GetMachineData(slotMachineLevel);
            if (currentData == null || currentData.RankWeight == null) return;

            List<int> weights = currentData.RankWeight; // [8600, 1000, 399, 1]
            float totalWeight = 0;

            // 총 가중치 합산 (통상 10000이겠지만, 만약의 경우를 위해 계산)
            foreach (int weight in weights)
            {
                totalWeight += weight;
            }

            // 각 LayoutElement의 flexibleWidth 비율을 조절하여 길이 설정
            for (int i = 0; i < _probabilityBars.Count; i++)
            {
                if (i < weights.Count)
                {
                    // flexibleWidth에 비율을 넣어주면 Layout Group이 알아서 길이를 분배합니다.
                    float ratio = weights[i] / totalWeight;
                    _probabilityBars[i].flexibleWidth = ratio;

                    // 가중치가 0이면 아예 안 보이게 처리
                    _probabilityBars[i].gameObject.SetActive(weights[i] > 0);
                }
            }

        }

        private MachineProbabillity GetMachineData(int level)
        {
            return CardevilCore.Database.Database.MachineProbabillityList.Find(x => x.MachineLevel == level);
        }

        #endregion

    }
}