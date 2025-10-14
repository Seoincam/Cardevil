using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Cards.Interactions
{
    public class StageCardsView: MonoBehaviour, IClearable
    {
        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;

        [Header("Text")] 
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardCountText;
        
        [Header("Transform")]
        [SerializeField] private Transform bar;
        
        private readonly List<RectTransform> _slots = new();
        private StageCardsViewState? _lastState; // 같은 값 재적용 방지
        
        private void OnValidate()
        {
            if (useButton == null) LogEx.LogError("useButton == null");
            if (discardButton == null) LogEx.LogError("discardButton == null");
            if (sortByNumberButton == null) LogEx.LogError("sortByNumberButton == null");
            if (sortByIconButton == null) LogEx.LogError("sortByIconButton == null");
            if (deckCountText == null) LogEx.LogError("deckCountText == null");
            if (discardCountText == null) LogEx.LogError("discardCountText == null");
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
        public void ConfigureSlots(int slotCount)
        {
            while (_slots.Count < slotCount)
            {
                var slot = Managers.Resource.Instantiate("Cards/Slot", bar).GetComponent<RectTransform>();
                _slots.Add(slot);
            }

            while (_slots.Count > slotCount)
            {
                var last = _slots[^1];
                _slots.RemoveAt(_slots.Count - 1);
                Managers.Resource.Destroy(last.gameObject);
            }
        }
        
        /// <summary>
        /// 지정된 index의 슬롯에 카드를 배치.  
        /// </summary>
        public void SetCardToSlot(Card card, int slotIndex)
        {
            card.transform.SetParent(_slots[slotIndex]);
            card.UpdatePosition();
        }

        public void SetSlotActive(bool value, int index)
        {
            _slots[index].gameObject.SetActive(value);
        }

        public void AlignSlot()
        {
            _slots.Sort((a, b) =>
            {
                bool aActive = a.gameObject.activeSelf;
                bool bActive = b.gameObject.activeSelf;
                return bActive.CompareTo(aActive);
            });

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].transform.SetSiblingIndex(i);
            }
        }
    }
}