using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage.View
{
    public class StageCardsView: MonoBehaviour, IClearable
    {
        [SerializeField] private RectTransform bar;
        [SerializeField] private PointerAreaTrigger handArea;
        
        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;

        [Header("Text")] 
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardCountText;
        
        public PointerAreaTrigger HandArea => handArea;
        
        private IReadOnlyStageCardsModel _model;

        private readonly List<RectTransform> _slots = new();
        private StageCardsViewState? _lastState; // 같은 값 재적용 방지

        private float _widthFactor = 130;
        private const string SlotPath = "UI/CardUI/Slot";
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (useButton == null) LogEx.LogError("useButton == null");
            if (discardButton == null) LogEx.LogError("discardButton == null");
            if (sortByNumberButton == null) LogEx.LogError("sortByNumberButton == null");
            if (sortByIconButton == null) LogEx.LogError("sortByIconButton == null");
            if (deckCountText == null) LogEx.LogError("deckCountText == null");
            if (discardCountText == null) LogEx.LogError("discardCountText == null");
        }
#endif

        public void Init(IReadOnlyStageCardsModel model)
        {
            _model = model;
            ConfigureSlots(model.MaxHand);
        }
        
        /// <summary>
        /// 뷰를 초기 상태로 정리.
        /// 생성된 슬롯을 제거, 버튼의 모든 리스너를 해제.
        /// </summary>
        public void Clear()
        {
            _lastState = null;
            foreach (var slot in _slots)
            {
                if (slot) Managers.Resource.Destroy(slot.gameObject);
            }
            _slots.Clear();
            
            useButton.onClick.RemoveAllListeners();
            discardButton.onClick.RemoveAllListeners();
            sortByNumberButton.onClick.RemoveAllListeners();
            sortByIconButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// HandBar UI를 화면에 등장시키는 애니메이션을 실행합니다.  
        /// </summary>
        /// <returns>애니메이션 완료 후 완료되는 <see cref="UniTask"/></returns>
        public async UniTask EnterHandBarAsync()
        {
            useButton.transform.localScale = Vector3.zero;
            discardButton.transform.localScale = Vector3.zero;
            sortByNumberButton.transform.localScale = Vector3.zero;
            sortByIconButton.transform.localScale = Vector3.zero;

            await useButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
            await discardButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
            await sortByIconButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
            await sortByNumberButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
        }

        public async UniTask ExitHandBarAsync()
        {
            
        }

        /// <summary>
        /// 각 버튼에 전달된 UnityAction을 바인딩.  
        /// 기존에 등록된 모든 리스너를 제거한 뒤 새로 지정된 콜백을 연결.
        /// </summary>
        /// <param name="use">‘사용하기’ 버튼 클릭 시 호출될 콜백</param>
        /// <param name="discard">‘버리기’ 버튼 클릭 시 호출될 콜백</param>
        /// <param name="sortByNumber">‘숫자 정렬’ 버튼 클릭 시 호출될 콜백</param>
        /// <param name="sortByIcon">‘아이콘 정렬’ 버튼 클릭 시 호출될 콜백</param>
        public void BindButtonEvents(UnityAction use, UnityAction discard, UnityAction sortByNumber, UnityAction sortByIcon)
        {
            useButton.onClick.RemoveAllListeners();
            discardButton.onClick.RemoveAllListeners();
            sortByNumberButton.onClick.RemoveAllListeners();
            sortByIconButton.onClick.RemoveAllListeners();
            
            useButton.onClick.AddListener(use);
            discardButton.onClick.AddListener(discard);
            sortByNumberButton.onClick.AddListener(sortByNumber);
            sortByIconButton.onClick.AddListener(sortByIcon);
        }

        
        /// <summary>
        /// UI 상태를 전달받은 <see cref="StageCardsViewState"/> 값으로 갱신.  
        /// 직전 상태(<c>_lastState</c>)와 비교하여 변경된 항목만 업데이트.  
        /// </summary>
        public void UpdateUI(StageCardsViewState state)
        {
            if (_lastState is { } prev)
            {
                if (prev.CanUse != state.CanUse) useButton.interactable = state.CanUse;
                if (prev.CanDiscard != state.CanDiscard) discardButton.interactable = state.CanDiscard;
                if (prev.CanSort != state.CanSort)
                {
                    sortByNumberButton.interactable = state.CanSort;
                    sortByIconButton.interactable = state.CanSort;
                }

                if (prev.RemainingCards != state.RemainingCards) deckCountText.text = $"{state.RemainingCards}/50";
                if (prev.RemainingDiscards != state.RemainingDiscards) discardCountText.text = $"{state.RemainingDiscards}";
            }
            else 
            {
                useButton.interactable = state.CanUse;
                discardButton.interactable = state.CanDiscard;
                sortByNumberButton.interactable = state.CanSort;
                sortByIconButton.interactable = state.CanSort;
                
                deckCountText.text = $"{state.RemainingCards}/50";
                discardCountText.text = $"{state.RemainingDiscards}";
            }
            _lastState = state;
        }
        
        /// <summary>
        /// 현재 핸드바의 슬롯 개수를 조정.  
        /// 부족한 슬롯은 새로 생성, 초과한 슬롯은 제거.
        /// </summary>
        private void ConfigureSlots(int slotCount)
        {
            while (_slots.Count < slotCount)
            {
                // Managers.Resource.Instantiate(SlotPath, visualRoot);
                var slot = Managers.Resource.Instantiate(SlotPath, bar).GetComponent<RectTransform>();
                _slots.Add(slot);
            }

            while (_slots.Count > slotCount)
            {
                var last = _slots[^1];
                _slots.RemoveAt(_slots.Count - 1);
                Managers.Resource.Destroy(last.gameObject);
                // TODO: 풀링한다면 VisualRoot Slot도 저장해놔야함.
            }
        }

        public void HoldCardOutsideBar(Card card)
        {
            card.transform.SetParent(transform);
        }


        public void OnHandChanged()
        {
            var count = _model.Hand.Count;
            for (int i = 0; i < count; i++)
            {
                var slot = _slots[i];
                
                slot.gameObject.SetActive(true);
                _model.Hand[i].transform.SetParent(slot);
                _model.Hand[i].UpdatePosition();
            }

            if (count < _model.MaxHand)
            {
                for (int i = count; i < _model.MaxHand; i++)
                {
                    _slots[i].gameObject.SetActive(false);
                }
            }
            
            var width = _widthFactor * _model.Hand.Count;
            var height = 200;
            bar.sizeDelta = new Vector2(width, height);
        }
    }
}

/*
 * DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1)
 *     [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;
 */