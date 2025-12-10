using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Math = System.Math;

namespace Cardevil.Cards.InStage.View
{
    public class RerollView : MonoBehaviour, IClearable
    {
        [Header("Buttons")] 
        [SerializeField] private Button doButton;
        [SerializeField] private Button endButton;
        [SerializeField] private Button togglePreviewButton;

        [Header("Text")] 
        [SerializeField] private TextMeshProUGUI ticketCountText;

        [Header("Transform")] 
        [SerializeField] private Transform bar;
        [SerializeField] private Transform background;
        [SerializeField] private Transform ticketCountPanel;

        private bool _isInitialized;
        
        private readonly List<RectTransform> _slots = new();
        private CardVisualSettingSO _visualSetting;
        private Transform _cardVisualTransform;
        private CardDeckVisual _deckVisual;

        private UniTask _ticketAnim = UniTask.CompletedTask;
        private Tween _ticketTween;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!doButton) LogEx.LogError("doButton is null");
            if  (!endButton) LogEx.LogError("endButton is null");
            if (!togglePreviewButton) LogEx.LogError("togglePreviewButton is null");
            if (!ticketCountText) LogEx.LogError("ticketCountText is null");
            if (!bar) LogEx.LogError("bar is null");
            if (!background) LogEx.LogError("background is null");
            if (!ticketCountPanel) LogEx.LogError("ticketCountPanel is null");
        }
#endif

        /// <summary>
        /// 리롤 UI(View)를 초기화.
        /// 카드 비주얼 설정(<see cref="CardVisualSettingSO"/>)을 주입받고,
        /// 씬 내의 <see cref="CardDeckVisual"/>, Card Visual Transform 참조를 획득.
        /// 이미 초기화된 경우 재실행하지 않음.
        /// </summary>
        public void Init(CardVisualSettingSO visualSetting)
        {
            if (_isInitialized) return;
            
            _visualSetting = visualSetting;
            var deckVisuals = FindObjectsByType<CardDeckVisual>(FindObjectsSortMode.None);
            if (deckVisuals == null || deckVisuals.Length == 0) { LogEx.LogError("씬 내에 Deck Visual이 존재하지 않음!"); return; }
            _deckVisual = deckVisuals[0];

            string t = "Card Visual Transform";
            _cardVisualTransform = GameObject.Find(t).transform;
            if (!_cardVisualTransform) { LogEx.LogError($"씬 내에 존재하지 않음: {t}"); return; }
            _isInitialized = true;
        }
        
        /// <summary>
        /// 뷰를 초기 상태로 정리.
        /// 생성된 슬롯을 제거, 버튼의 모든 리스너를 해제.
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _slots)
            {
                if (slot) Managers.Resource.Destroy(slot.gameObject);
            }
            _slots.Clear();
            
            doButton.onClick.RemoveAllListeners();
            endButton.onClick.RemoveAllListeners();
            togglePreviewButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// 리롤 단계 진입 연출 수행.
        /// 배경 페이드인, 티켓 카운트 등장 애니메이션, 버튼 스케일 인 등을 순서대로 재생.
        /// </summary>
        /// <returns>연출이 모두 완료되면 완료되는 <see cref="UniTask"/>.</returns>
        public async UniTask EnterRerollAsync()
        {
            var image = background.GetComponent<Image>();
            var color = image.color;
            
            ticketCountPanel.transform.localScale = Vector3.one;
            image.color = new Color(color.r, color.g, color.b, 0f);
            doButton.transform.localScale = Vector3.zero;
            endButton.transform.localScale = Vector3.zero;
            togglePreviewButton.transform.localScale = Vector3.zero;
            
            await image.DOColor(color, 1f);
            await AnimateTicketChangeAsync(0, Managers.Game.PlayerStatus.RerollTicket); 
            await doButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
            await endButton.transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
            await togglePreviewButton.transform.DOScale(1f, .2f);
            
            _cardVisualTransform.SetAsLastSibling();
            _deckVisual.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 리롤 단계 종료 연출 수행.
        /// 배경 페이드아웃, 버튼/티켓 패널 스케일 아웃 등을 순서대로 재생.
        /// </summary>
        /// <returns>연출이 모두 완료되면 완료되는 <see cref="UniTask"/>.</returns>
        public async UniTask ExitRerollAsync()
        {
            var image = background.GetComponent<Image>();
            var color = image.color;
            
            await doButton.transform.DOScale(0f, .2f).SetEase(Ease.InBack);
            await endButton.transform.DOScale(0f, .2f).SetEase(Ease.InBack);
            await togglePreviewButton.transform.DOScale(0f, .2f).SetEase(Ease.InBack);
            await ticketCountPanel.transform.DOScale(0f, .2f).SetEase(Ease.InBack);
            await image.DOColor(new Color(color.r, color.g, color.b, 0), .7f);
        }

        /// <summary>
        /// 각 버튼에 전달된 UnityAction을 바인딩.  
        /// 기존에 등록된 모든 리스너를 제거한 뒤 새로 지정된 콜백을 연결.
        /// </summary>
        /// <param name="doReroll">'다시뽑기' 버튼 클릭 시 호출될 콜백</param>
        /// <param name="endReroll">'선택하기' 버튼 클릭 시 호출될 콜백</param>
        public void BindButtonEvents(UnityAction doReroll, UnityAction endReroll)
        {
            doButton.onClick.RemoveAllListeners();
            endButton.onClick.RemoveAllListeners();

            doButton.onClick.AddListener(doReroll);
            endButton.onClick.AddListener(endReroll);
            // TODO: TogglePreview도 추가
        }

        /// <summary>
        /// 뷰의 상호작용 가능 여부를 일괄 설정.
        /// 버튼들의 상태를 갱신.
        /// </summary>
        public void SetInteractable(bool value)
        {
            doButton.interactable = value;
            endButton.interactable = value;
            togglePreviewButton.interactable = value;
        }
        
        /// <summary>
        /// 슬롯 개수를 조정.  
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
        /// 지정된 인덱스 슬롯에 카드를 배치, 시각적 인덱스를 갱신.
        /// </summary>
        /// <param name="card">배치할 카드.</param>
        /// <param name="slotIndex">배치할 슬롯의 인덱스.</param>
        public void SetCardToSlot(Card card, int slotIndex)
        {
            card.transform.SetParent(_slots[slotIndex]);
            card.UpdatePosition();
        }

        #region Reroll Ticket Count

        /// <summary>
        /// 리롤 개수를 즉시 지정 값으로 표시합
        /// 애니메이션 없이 텍스트만 갱신.
        /// </summary>
        /// <param name="value">표시할 티켓 개수.</param>
        public void SetTicketImmediate(int value)
        {
            ticketCountText.text = value.ToString();
        }
        
        /// <summary>
        /// 리롤 티켓 개수 변화를 애니메이션으로 표현.
        /// 증가/감소 단계별로 텍스트를 갱신, 스케일 트윈을 반복 적용.
        /// </summary>
        /// <param name="ct">외부 취소 토큰. 파괴 토큰과 결합하여 사용 가능.</param>
        /// <returns>애니메이션이 완료되면 완료되는 <see cref="Cysharp.Threading.Tasks.UniTask"/>.</returns>
        public async UniTask AnimateTicketChangeAsync(int oldValue, int newValue, CancellationToken ct = default)
        {
            // 파괴 시 종료 위해 토큰 결합
            var linked = CancellationTokenSource.CreateLinkedTokenSource(
                ct, this.GetCancellationTokenOnDestroy()).Token;

            _ticketAnim = InnerAnimateTicketChangeAsync(oldValue, newValue, linked);
            await _ticketAnim;
        }
        
        // 내부에서 사용하는 AnimateTicketChangeAsync
        private async UniTask InnerAnimateTicketChangeAsync(int oldValue, int newValue, CancellationToken ct)
        {
            int delta = newValue - oldValue;
            if (delta == 0)
            {
                SetTicketImmediate(newValue);
                return;
            }
                
            // 캐시
            var tr = ticketCountText.transform;
            float half = _visualSetting.RerollCountScaleDuration * .5f;
            float scale = _visualSetting.RerollCountScale;
            Ease ease =  _visualSetting.RerollCountEase;
                
            // 기존 크기
            var originalScale = tr.localScale;

            int remain = oldValue;
            int dir = Math.Sign(delta);
            int steps =  Math.Abs(delta);

            try
            {
                for (int i = 0; i < steps; i++)
                {
                    remain += dir;
                    ticketCountText.text = remain.ToString();
                    _ticketTween?.Kill(true);
                    _ticketTween = tr.DOScale(scale, half)
                        .SetEase(ease)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetLink(gameObject);
                    await _ticketTween.AsyncWaitForCompletion();
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 시 넘어감
            }
            finally
            {
                _ticketTween?.Kill(true);
                tr.localScale = originalScale;
                SetTicketImmediate(remain); // 오류 방지
            }
        }

        #endregion
    }
}