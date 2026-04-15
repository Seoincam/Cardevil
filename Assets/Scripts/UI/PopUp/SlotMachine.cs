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
        public bool isTest = false;
        public enum ZoomType { None, Epic, Legend } // 줌 연출 타입 구분

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
        public async UniTask ActiveSlotMachine(float waitSeconds)
        {
            await UniTask.WaitForSeconds(waitSeconds);
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
            if (isSetting) return;
            isSetting = true;
            if (_rerollVisual != null) _rerollVisual.SetRolling(true);

            try
            {
                // 🔥 0. 시작 전 모든 슬롯의 태그를 미리 숨김 처리
                foreach (var slot in slots) slot.ResetTag();

                // 모두 회전 시작
                foreach (var slot in slots) slot.StartSpinning(probalityList);
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

                // 1. 실제 결과 결정 및 오름차순 정렬
                List<Item> trueResults = new List<Item>();
                for (int i = 0; i < 3; i++) trueResults.Add(SettingItem(probalityList));
                trueResults = trueResults.OrderBy(item => (int)item.rareType).ToList();

                // 2. 잭팟/페이크를 위한 '보여주기용' 리스트 생성 (조작)
                List<Item> displayResults = CreateDisplayList(trueResults);
                int legendCount = trueResults.Count(x => x.rareType == Define.RareType.Legend);

                // -----------------------------------------------------
                // [타이밍 제어] 1, 2번째 슬롯 순차적 정지 및 태그 노출
                // -----------------------------------------------------

                // 1번째 슬롯 정지
                slots[0].SettingSlot(probalityList, displayResults[0]);
                // 🔥 1번 슬롯 바운스 대기 (0.6초) 후 태그 표시
                await UniTask.Delay(TimeSpan.FromSeconds(0.6f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                slots[0].ShowTag(displayResults[0].rareType);

                // 기존 dropTiming 리듬 유지 (이미 바운스로 0.6초를 소모했으므로 남은 시간만 대기)
                float waitExtra = Mathf.Max(0f, dropTiming - 0.6f);
                if (waitExtra > 0) await UniTask.Delay(TimeSpan.FromSeconds(waitExtra), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());

                // 2번째 슬롯 정지
                slots[1].SettingSlot(probalityList, displayResults[1]);
                // 🔥 2번째 슬롯 바운스 대기 (0.6초) 후 태그 표시
                await UniTask.Delay(TimeSpan.FromSeconds(0.6f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                slots[1].ShowTag(displayResults[1].rareType);

                // -----------------------------------------------------
                // [타이밍 제어] 3번째 슬롯 줌인 및 대기 연출
                // -----------------------------------------------------

                ZoomType zType = DetermineZoomType(trueResults[2].rareType);
                Tween cameraTween = CameraActionForSlot3(zType);

                if (cameraTween != null)
                {
                    // 확률표에 따른 줌인 패닝이 완료될 때까지 대기 (화면이 3번 슬롯으로 빨려 들어감)
                    await cameraTween.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                else
                {
                    // 줌이 없는 경우에도 3번 슬롯이 혼자 돌아가는 텐션을 주기 위해 대기
                    await UniTask.Delay(TimeSpan.FromSeconds(0.8f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                }

                // 3번째 슬롯 최종 멈춤
                slots[2].SettingSlot(probalityList, displayResults[2]);
                // 🔥 3번째 슬롯도 바닥에 완전히 내려오도록 대기 후 태그 표시
                await UniTask.Delay(TimeSpan.FromSeconds(0.6f), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                slots[2].ShowTag(displayResults[2].rareType);

                // -----------------------------------------------------
                // [분기] 잭팟, 페이크, 혹은 정상 종료
                // -----------------------------------------------------
                bool isFakeLegend = (legendCount == 1 && UnityEngine.Random.Range(0, 100) < 20); // 20% 확률 발동

                if (legendCount >= 2 || isFakeLegend)
                {
                    // 잭팟 역전 연출 실행 (카메라가 1.6배 줌을 유지한 채 2번, 1번 슬롯으로 이동합니다 )
                    await PlayJackpotSequence(trueResults, legendCount, isFakeLegend);
                }
                else
                {
                    // 역전 연출이 없었다면, 3번 슬롯 결과를 보여준 후 카메라를 중앙으로 원상 복귀시킵니다.
                    if (zType != ZoomType.None)
                    {
                        _renderCamera.transform.DOMove(_originCamPos, 0.5f).SetEase(Ease.OutQuad);
                        _renderCamera.DOOrthoSize(_originCamSize, 0.5f).SetEase(Ease.OutQuad);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
                    }
                }

                // 6. 잭팟 성공 연출 파티클 (최종 달성)
                if (legendCount >= 2)
                {
                    _animationController.PlayJackpotSuccessEffect();
                }

                // 7. 전설(6 아이콘) 카드 최종 공개 (회전 효과)
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
                foreach (var slot in slots)
                {
                    if (slot.item.rareType == Define.RareType.Legend) await slot.RevealLegendItem();
                }

                // 8. 잭팟일 경우 3개 자동 선택
                if (legendCount == 3)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f));
                    OnSelectClicked(null);
                }
            }
            finally
            {
                isSetting = false;
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
            if (!isTest) // 테스트중이라면 골드와 관계없이 설정
            {
                // 3. 골드 체크 및 차감 (PlayerStatus는 실제 관리하시는 플레이어 데이터 스크립트로 대체)
                if (gold < currentData.LevelUpCost)
                {
                    Debug.Log("골드가 부족합니다!");
                    if (_upgradeVisual != null) _upgradeVisual.SetBlocked(true);
                    return;
                }

            }

            // 4. 업그레이드 실행
            CardevilCore.PlayerStatus.SetBaseValue(PlayerStatType.Gold, gold - currentData.LevelUpCost);
            slotMachineLevel++;
            CardevilCore.PlayerStatus.SetBaseValue(PlayerStatType.SlotMachineLevel, slotMachineLevel);
            Debug.Log($"슬롯머신 레벨업! 현재 레벨: {slotMachineLevel}");


            if (_upgradeVisual != null) _upgradeVisual.SetBlocked(false);

            // 새로운 레벨의 확률 데이터를 가져와서 실제 뽑기 배열 갱신
            var nextData = GetMachineData(slotMachineLevel);
            if (nextData != null && nextData.RankWeight != null)
            {
                probalityList = nextData.RankWeight.ToArray();
            }

            // 5. 확률 바 UI 갱신
            UpdateProbabilityPanel();
            UpdateLayout();
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

        #region CameraZoom

        /// <summary>
        /// 잭팟 기회(사전 연출) 및 시스템 역전 애니메이션
        /// </summary>
        private async UniTask PlayJackpotSequence(List<Item> trueResults, int legendCount, bool isFakeLegend)
        {
            // [공통] 1.6배 줌 상태를 유지하며 2번째 슬롯으로 패닝
            Vector3 slot2Pos = GetSlotWorldPos(1);
            await _renderCamera.transform.DOMove(new Vector3(slot2Pos.x, slot2Pos.y, _originCamPos.z), 0.5f).SetEase(Ease.InOutSine);

            // [공통] 잭팟 기회 사전 연출 및 2번 슬롯 초고속 회전 + 진동
            _animationController.SetJackpotAlertMode(true);
            slots[1].StartFastGlitchSpin(probalityList);
            await _renderCamera.transform.DOShakePosition(2.0f, 0.4f, 40).ToUniTask();

            _animationController.SetJackpotAlertMode(false);

            // 💥 케이스 1: 페이크 (Legend 1개인 경우 실망 연출)
            if (isFakeLegend)
            {
                slots[1].StopGlitchSpin(trueResults[1]);
                // 페이크 결과에 맞는 태그 노출 (보통 Legend 미만 등급)
                slots[1].ShowTag(trueResults[1].rareType);

                await _renderCamera.transform.DOMove(_originCamPos, 0.5f);
                await _renderCamera.DOOrthoSize(_originCamSize, 0.5f);
                return;
            }

            // 케이스 2 & 3 공통: 2번째 슬롯이 Legend(6형태)로 강제 변환
            slots[1].ForceChangeToLegend(trueResults[1]);
            // 변환 즉시 Legend 태그로 갱신
            slots[1].ShowTag(Define.RareType.Legend);

            // 💥 케이스 3: 찐 잭팟 (Legend 3개인 경우 1번 슬롯까지 변환)
            if (legendCount == 3)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

                // 1번째 슬롯으로 패닝
                Vector3 slot1Pos = GetSlotWorldPos(0);
                await _renderCamera.transform.DOMove(new Vector3(slot1Pos.x, slot1Pos.y, _originCamPos.z), 0.5f).SetEase(Ease.InOutSine);

                // 1번째 슬롯 진동 및 회전
                _animationController.SetJackpotAlertMode(true);
                slots[0].StartFastGlitchSpin(probalityList);
                await _renderCamera.transform.DOShakePosition(1.5f, 0.4f, 40).ToUniTask();
                _animationController.SetJackpotAlertMode(false);

                slots[0].ForceChangeToLegend(trueResults[0]);
                // 1번 슬롯도 변환 즉시 Legend 태그로 갱신
                slots[0].ShowTag(Define.RareType.Legend);
            }

            // 최종 카메라 복귀
            await _renderCamera.transform.DOMove(_originCamPos, 0.5f);
            await _renderCamera.DOOrthoSize(_originCamSize, 0.5f);
        }

        /// <summary>
        /// 확률 표에 따라 줌 연출 종류를 결정합니다. 
        /// </summary>
        private ZoomType DetermineZoomType(Define.RareType slot3Rarity)
        {
            int rand = UnityEngine.Random.Range(0, 100); // 0 ~ 99

            switch (slot3Rarity)
            {
                case Define.RareType.Rare:
                    return rand < 30 ? ZoomType.Epic : ZoomType.None; // 30% 확률로 Epic 페이크 줌 [cite: 69]
                case Define.RareType.Epic:
                    return rand < 30 ? ZoomType.Legend : ZoomType.Epic; // 30% 확률로 Legend 페이크 줌 [cite: 69]
                case Define.RareType.Legend:
                    return ZoomType.Legend; // Legend는 100% 확정 줌 [cite: 69]
                default:
                    return ZoomType.None;
            }
        }

        /// <summary>
        /// 기획서 명세: Legend가 여러 개일 때 처음에는 1개만 있는 것처럼 속입니다.
        /// </summary>
        private List<Item> CreateDisplayList(List<Item> trueList)
        {
            List<Item> displayList = new List<Item>(trueList);
            int legendCount = trueList.Count(x => x.rareType == Define.RareType.Legend);

            if (legendCount == 2)
            {
                // [비전설, 전설, 전설] -> [비전설, 비전설, 전설]로 조작
                displayList[1] = Managers.Item.GetRandomItemOfGrade(Define.RareType.Rare);
            }
            else if (legendCount == 3)
            {
                // [전설, 전설, 전설] -> [비전설, 비전설, 전설]로 조작
                displayList[0] = Managers.Item.GetRandomItemOfGrade(Define.RareType.Rare);
                displayList[1] = Managers.Item.GetRandomItemOfGrade(Define.RareType.Rare);
            }
            return displayList;
        }

        /// <summary>
        /// 잭팟 기회 연출: 카메라 이동 + 격렬한 진동 + 아이콘 변신
        /// </summary>
        private async UniTask PlayJackpotSequence(List<Item> trueResults, int legendCount)
        {
            // 2번째 슬롯부터 검사 (기획서 상 3번은 이미 떴고, 2번 혹은 1번이 변함)
            int[] targetSlots = legendCount == 3 ? new int[] { 1, 0 } : new int[] { 1 };

            foreach (int idx in targetSlots)
            {
                // 카메라 패닝 (해당 슬롯으로)
                Vector3 targetPos = GetSlotWorldPos(idx);
                await _renderCamera.transform.DOMove(new Vector3(targetPos.x, targetPos.y, _originCamPos.z), 0.5f).SetEase(Ease.OutCubic);

                // 격렬한 진동 (2초)
                await _renderCamera.transform.DOShakePosition(2.0f, 0.3f, 30).ToUniTask();

                // 쾅! 소리와 함께 실제 전설(6형태)로 변경
                slots[idx].ForceChangeToLegend(trueResults[idx]);
            }

            // 카메라 원복
            await _renderCamera.transform.DOMove(_originCamPos, 0.5f);
        }

        /// <summary>
        /// 기획서의 카메라 배율(Scale), 이동(TranslateX), 대기 시간 연출을 담당합니다. [cite: 69, 70, 71]
        /// </summary>
        private Tween CameraActionForSlot3(ZoomType zoomType, System.Action onMiddleAction = null)
        {
            if (_renderCamera == null || zoomType == ZoomType.None) return null;

            Sequence seq = DOTween.Sequence();

            float targetOrthoSize = _originCamSize;
            float waitTime = 0f;

            // 기획서 스펙 적용
            if (zoomType == ZoomType.Epic)
            {
                targetOrthoSize = _originCamSize / 1.2f;
                waitTime = 1.2f;
            }
            else if (zoomType == ZoomType.Legend)
            {
                targetOrthoSize = _originCamSize / 1.6f;
                waitTime = 1.5f;
            }

            // 🔥 핵심: 타겟을 3번째 슬롯의 실제 위치로 잡음
            Vector3 slot3Pos = GetSlotWorldPos(2);
            // Z축은 카메라의 원래 Z축 거리를 유지
            Vector3 finalCamPos = new Vector3(slot3Pos.x, slot3Pos.y, _originCamPos.z);

            // 1. 타겟 위치(3번째 슬롯)로 줌인 및 패닝
            seq.Append(_renderCamera.transform.DOMove(finalCamPos, zoomInTime).SetEase(Ease.OutQuad));
            seq.Join(_renderCamera.DOOrthoSize(targetOrthoSize, zoomInTime).SetEase(Ease.OutQuad));

            // 2. 텐션을 위한 대기 (이 시간 동안 화면이 줌인 된 상태로 3번 슬롯이 계속 돌아갑니다)
            seq.AppendInterval(waitTime);

            // 3. 아이템 결정 콜백이 있다면 호출
            seq.AppendCallback(() => {
                if (onMiddleAction != null) onMiddleAction.Invoke();
            });

            // ※ 줌아웃(복귀) 로직은 잭팟 발생 시 줌인 상태를 유지해야 하므로 여기서 제거했습니다.
            // SettingSlots 함수에서 상황에 맞게 수동으로 줌아웃 시킵니다.

            return seq;
        }

        #endregion

        #region Tool

        /// <summary>
        /// 특정 슬롯 UI의 월드 좌표를 반환하는 헬퍼 함수입니다.
        /// 카메라가 해당 슬롯으로 부드럽게 패닝(이동)할 때 목표 위치로 사용됩니다.
        /// </summary>
        /// <param name="index">슬롯의 인덱스 (0: 첫 번째, 1: 두 번째, 2: 세 번째)</param>
        /// <returns>해당 슬롯의 월드 좌표(Vector3)</returns>
        private Vector3 GetSlotWorldPos(int index)
        {
            // 인덱스 범위 초과를 막기 위한 안전 장치
            if (slots == null || index < 0 || index >= slots.Count)
            {
                Debug.LogError($"[GetSlotWorldPos] 유효하지 않은 슬롯 인덱스입니다: {index}");
                return Vector3.zero;
            }

            // RectTransform(또는 Transform)의 position은 해당 UI 요소의 실제 월드 좌표를 반환합니다.
            // 카메라가 이 좌표의 x, y 값을 타겟으로 삼아 이동(DOMove)하게 됩니다.
            return slots[index].GetComponent<RectTransform>().position;
        }




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