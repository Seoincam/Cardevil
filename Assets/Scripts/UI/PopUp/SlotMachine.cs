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

namespace Cardevil.UI.PopUp
{
    public class SlotMachine : UI_Popup
    {
        // 확대 설정을 위해
        [SerializeField]
        private Camera _renderCamera;
        private Vector3 _originCamPos;
        private float _originCamSize;
        private Tween _cameraTween; // 중복 실행 방지 및 관리용
        private int slotMachineLevel;

        //----
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
            UpdateLayout();
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
                // 작업이 끝났거나 취소되었을 때 반드시 실행
                isSetting = false;
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
            // 돈이 된다면
            /*
            if (Managers.Game.PlayerStatus.gold >= Managers.Database.Database.MachineProbabillityList[slotMachineLevel - 1].LevelUpCost)
            {

                // 레벨업, 

            }
            */

            // 상승 및 인덱스 조절
            var newSlotMachineLevel = Math.Min(
                CardevilCore.PlayerStatus.GetFinalValue(PlayerStatType.SlotMachineLevel) + 1, 
                CardevilCore.Database.Database.MachineProbabillityList.Count);
            
            CardevilCore.PlayerStatus.SetBaseValue(PlayerStatType.SlotMachineLevel, newSlotMachineLevel);
            
            //동기화
            slotMachineLevel = newSlotMachineLevel;
       
            //리스트받기
            probalityList = CardevilCore.Database.Database.MachineProbabillityList[slotMachineLevel - 1].RankWeight.ToArray();

            // 레이아웃 업데이트
            UpdateLayout();

        }
        private void OnSelectClicked(PointerEventData eventData) { this.gameObject.SetActive(false); }
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
    }
}