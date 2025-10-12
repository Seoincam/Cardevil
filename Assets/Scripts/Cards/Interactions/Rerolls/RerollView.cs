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

namespace Cardevil.Cards.Interactions
{
    public class RerollView : MonoBehaviour
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

        private readonly List<RectTransform> _slots = new();
        private CardVisualSettingSO _visualSetting;
        
        private UniTask _ticketAnim = UniTask.CompletedTask;
        private Tween _ticketTween;

        public void Init(CardVisualSettingSO visualSetting)
        {
            _visualSetting = visualSetting;
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

        public void SetCardToSlot(Card card, int slotIndex)
        {
            card.transform.SetParent(_slots[slotIndex]);
            card.UpdateIndex(slotIndex);
        }

        #region Reroll Ticket Count

        public void SetTicketImmediate(int value)
        {
            ticketCountText.text = value.ToString();
        }

        public async UniTask AnimateTicketChangeAsync(int oldValue, int newValue, CancellationToken ct = default)
        {
            // 이전 실행 중 애니메이션 있다면 완료 대기
            await _ticketAnim;
            
            // 파괴 시 종료 위해 토큰 결합
            var linked = CancellationTokenSource.CreateLinkedTokenSource(
                ct, this.GetCancellationTokenOnDestroy()).Token;

            _ticketAnim = InnerAnimateTicketChangeAsync(oldValue, newValue, linked);
            await _ticketAnim;
        }
        
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