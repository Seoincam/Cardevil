using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Cards.Visual.Handler;
using Cardevil.Core;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage.View
{
    public class CardValueSelectionView : MonoBehaviour, IClearable
    {
        [Header("State")] 
        [field: SerializeField, VisibleOnly, Tooltip("드래그 중, 카드(포인터)가 Drop Area 내에 있는가?")] 
        public bool IsInDropArea { get; private set; }
        
        [field: SerializeField, VisibleOnly, Tooltip("Drop Area 내에 Drop을 했나?")] 
        public bool IsDropped { get; private set; }
        
        [Header("References")]
        [SerializeField] private Image bar;
        [SerializeField] private PointerAreaTrigger valueChangeArea;
        
        [Header("SO")]
        [SerializeField] private ValueSelectionViewAnimSetting setting;


        /// <summary>
        /// 값 선택 완료 이벤트.
        /// 선택된 카드와 선택 값(번호 또는 방향) 전달.
        /// </summary>
        public event Action<Card, (int, Direction)> ValueSelected;
        
        private const float CardScale = .6f;
        private const string SlotPath = "UI/CardUI/Slot";
        
        private readonly List<RectTransform> _slots = new();
        private readonly List<CardVisualValueSelectionView> _visuals = new();
        private CancellationTokenSource _animCts;
        
        private Card _draggedCard;
        private (CardColor, int) _attackValue;
        private Direction _moveValue;
        private Tween _draggedCardFadeTween;
        

        /// <summary>
        /// 선택 UI 초기화.
        /// 내부 참조 캐싱 및 초기 비활성화 처리.
        /// </summary>
        public void Init()
        {
            bar.gameObject.SetActive(false);
            valueChangeArea.gameObject.SetActive(false);
            
            valueChangeArea.PointerEntered  += OnPointerEnterInArea;
            valueChangeArea.PointerExited += OnPointerExitInArea;
        }
        
        private void Open(Card card)
        {
            Close();
            Clear();

            var cardData = card.Data;
            
            // 구성
            int count = cardData.Kind switch
            {
                CardKind.Attack => cardData.NumberSelectState.Selectables.Count,
                CardKind.Move => cardData.DirectionSelectState.Selectables.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            ConfigureFrame(count);
            ConfigureSlots(count);
            ConfigureCards(cardData, count);

            bar.rectTransform.anchoredPosition = setting.openPosition;
            bar.gameObject.SetActive(true);
            
            // 애니메이션
            _animCts = new CancellationTokenSource();
            PlayAnimateAsync(_animCts.Token).Forget();
        }

        /// <summary>
        /// 선택 UI 닫기.
        /// 애니메이션 취소, 트윈 정리, 오브젝트 비활성화 및 내부 상태 초기화.
        /// </summary>
        public void Close()
        {
            if (!bar.gameObject.activeSelf)
                return;
            
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
            
            bar.gameObject.SetActive(false);
            
            Clear();
        }
        
        public void Clear()
        {
            // TODO Pooling하기

            if (_visuals.Count == 0)
                return;

            for (int i = _visuals.Count - 1; i >= 0; i--)
            {
                _visuals[i].Selected -= OnValueSelected;
                AssetUtil.Destroy(_visuals[i].gameObject);
            }
            
            _visuals.Clear();
        }

        /// <summary>
        /// 값 선택 시 호출되는 내부 핸들러.
        /// 선택 이벤트 전달 후 UI 닫기.
        /// </summary>
        private void OnValueSelected((int, Direction) values)
        {
            ValueSelected?.Invoke(_draggedCard, values);
            // FadeDraggedCard(true);
            
            _draggedCard = null;
            IsDropped = false;
            IsInDropArea = false;
            
            Close();
            valueChangeArea.gameObject.SetActive(false);
        }

        private async UniTaskVoid PlayAnimateAsync(CancellationToken ct)
        {
            // 외관 초기화
            bar.color -= new Color(0, 0, 0, bar.color.a);
            foreach (var visual in _visuals)
                visual.CanvasGroup.alpha = 0;

            // 애니메이션 처리
            bool fadeCanceled = await bar
                .DOFade(1f, setting.barFadeInDuration)
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();
            
            if (fadeCanceled)
                return;

            foreach (var visual in _visuals)
            {
                AnimateCard(visual, setting.cardFadeInUpDuration, ct).Forget();
                
                bool delayCanceled = await UniTask
                    .Delay(TimeSpan.FromSeconds(setting.cardInterval), cancellationToken: ct)
                    .SuppressCancellationThrow();
                
                if (delayCanceled)
                    return;
            }
        }

        private async UniTaskVoid AnimateCard(CardVisualValueSelectionView card, float dur, CancellationToken ct)
        {
            var originalPos = card.Rect.anchoredPosition;
            card.Rect.anchoredPosition = originalPos + new Vector2(0, -20);

            await DOTween.Sequence()
                .Join(card.CanvasGroup.DOFade(1f, dur))
                .Join(card.Rect.DOAnchorPos(originalPos, dur))
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();
        }
        
        /// <summary>
        /// <see cref="bar"/> 크기를 조정.
        /// </summary>
        private void ConfigureFrame(int slotCount)
        {
            if (!setting.frameWidths.TryGetValue(slotCount, out var width))
            {
                LogEx.LogWarning("지정되지 않은 선택 가능 개수: " + slotCount);
                width = slotCount * 140f;
            }

            bar.rectTransform.sizeDelta = new Vector2(width, bar.rectTransform.rect.height);
        }

        /// <summary>
        /// 슬롯 개수를 조정.  
        /// 부족한 슬롯은 새로 생성, 초과한 슬롯은 제거.
        /// </summary>
        private void ConfigureSlots(int slotCount)
        {
            while (_slots.Count < slotCount)
            {
                var slot = AssetUtil.Instantiate(SlotPath, bar.rectTransform).GetComponent<RectTransform>();
                _slots.Add(slot);
            }

            while (_slots.Count > slotCount)
            {
                var last = _slots[^1];
                _slots.RemoveAt(_slots.Count - 1);
                AssetUtil.Destroy(last.gameObject);
            }
        }

        /// <summary>
        /// 카드 UI 생성 및 데이터 바인딩
        /// </summary>
        private void ConfigureCards(CardData data, int count)
        {
            const string path = "Cards/Visual/Handler/CardVisual_ValueSelectionView";

            for (int i = 0; i < count; i++)
            {
                var go = AssetUtil.Instantiate(path, _slots[i]);
                var visual = go.GetComponent<CardVisualValueSelectionView>();
                visual.Selected += OnValueSelected;

                if (data.Kind == CardKind.Attack)
                {
                    var num = data.NumberSelectState.Selectables[i].value;
                    visual.Init(CardScale, data.Color, num);
                }
                else if (data.Kind == CardKind.Move)
                {
                    var dir = data.DirectionSelectState.Selectables[i].value;
                    visual.Init(CardScale, dir);
                }
                
                _visuals.Add(visual);
            }
        }
        
        
        public void OnDragStarted(Card card)
        {
            if (!card.Data.CanOpenSelection)
                return;
            
            _draggedCard = card;
            valueChangeArea.gameObject.SetActive(true);
        }

        public void OnDragEnded()
        {
            if (IsInDropArea)
            {
                IsDropped = true;
                
                SetRaycastTarget(true);
                _draggedCard.transform.SetParent(transform);
                _draggedCard.UpdatePosition();
                _draggedCard.FadeChangeImage(false);
                return;
            }
            
            _draggedCard = null;
            valueChangeArea.gameObject.SetActive(false);
        }

        
        private void OnPointerEnterInArea()
        {
            if (IsDropped)
                return;
            if (!_draggedCard)
                return;
            
            // FadeDraggedCard(false);
            
            IsInDropArea = true;
            Open(_draggedCard);
            SetRaycastTarget(false);
        }

        private void OnPointerExitInArea()
        {
            if (!_draggedCard)
                return;
            if (IsDropped)
                return;
            
            // FadeDraggedCard(true);
            
            IsInDropArea = false;
            Close();
        }

        private void FadeDraggedCard(bool fadeIn)
        {
            var targetAlpha = fadeIn ? 1f : 0f;
            
            _draggedCardFadeTween?.Kill();
            _draggedCardFadeTween = _draggedCard.VisualCanvasGroup.DOFade(targetAlpha, setting.draggedCardFadeDuration)
                .SetEase(setting.draggedCardFadeEase);
        }
        
        private void SetRaycastTarget(bool value)
        {
            bar.raycastTarget = value;
            foreach (var visual in _visuals)
                visual.CanvasGroup.blocksRaycasts = value;
        }
    }
}