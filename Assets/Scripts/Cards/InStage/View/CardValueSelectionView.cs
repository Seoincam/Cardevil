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

        [SerializeField] private ValueSelectionViewAnimSetting setting;
        
        private const float CardScale = .6f;
        private const string SlotPath = "Cards/Slot";
        
        private Image _bar;
        private RectTransform _rect;
        
        private readonly List<RectTransform> _slots = new();
        private readonly List<CardVisualValueSelectionView> _visuals = new();
        private CancellationTokenSource _animCts;
        
        private Card _cardCache;
        private (CardColor, int) _attackValue;
        private Direction _moveValue;
        
        /// <summary>
        /// 값 선택 완료 이벤트.
        /// 선택된 카드와 선택 값(번호 또는 방향) 전달.
        /// </summary>
        public event Action<Card, (int, Direction)> ValueSelected;

        /// <summary>
        /// 선택 UI 초기화.
        /// 내부 참조 캐싱 및 초기 비활성화 처리.
        /// </summary>
        public void Init()
        {
            _bar = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 선택 UI 토글.
        /// 동일 카드 입력 시 닫기 처리, 다른 카드 입력 시 선택 UI 열기.
        /// </summary>
        /// <param name="card">선택 UI 표시 대상 카드</param>
        public void Toggle(Card card)
        {
            if (card == _cardCache)
                Close();
            else 
                Open(card);
        }
        
        private void Open(Card card)
        {
            Clear();

            _cardCache = card;
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

            _rect.anchoredPosition = setting.openPosition;
            gameObject.SetActive(true);
            
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
            if (!gameObject.activeSelf)
                return;
            
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
            
            gameObject.SetActive(false);
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
            _cardCache = null;
        }

        /// <summary>
        /// 값 선택 시 호출되는 내부 핸들러.
        /// 선택 이벤트 전달 후 UI 닫기.
        /// </summary>
        private void OnValueSelected((int, Direction) values)
        {
            ValueSelected?.Invoke(_cardCache, values);
            Close();
        }

        private async UniTaskVoid PlayAnimateAsync(CancellationToken ct)
        {
            // 외관 초기화
            _bar.color -= new Color(0, 0, 0, _bar.color.a);
            foreach (var visual in _visuals)
                visual.CanvasGroup.alpha = 0;

            // 애니메이션 처리
            bool fadeCanceled = await _bar
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
        /// <see cref="_bar"/> 크기를 조정.
        /// </summary>
        private void ConfigureFrame(int slotCount)
        {
            if (!setting.frameWidths.TryGetValue(slotCount, out var width))
            {
                LogEx.LogWarning("지정되지 않은 선택 가능 개수: " + slotCount);
                width = slotCount * 140f;
            }

            _rect.sizeDelta = new Vector2(width, _rect.rect.height);
        }

        /// <summary>
        /// 슬롯 개수를 조정.  
        /// 부족한 슬롯은 새로 생성, 초과한 슬롯은 제거.
        /// </summary>
        private void ConfigureSlots(int slotCount)
        {
            while (_slots.Count < slotCount)
            {
                var slot = AssetUtil.Instantiate(SlotPath, _rect).GetComponent<RectTransform>();
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
    }
}