using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Presenter;
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
        // TODO 애니메이션 처리

        [SerializeField] private Vector2 openPosition = new(0, -125);
        
        private const float CardScale = .6f;
        private const string SlotPath = "Cards/Slot";
        private readonly Dictionary<int, float> _frameWidths = new()
        {
            {2, 391}, {3, 542}, {4, 691}, {9, 1300}
        };
        
        public event Action<Card, (int, Direction)> ValueSelected;
        
        private Image _bar;
        private RectTransform _rect;
        
        private readonly List<RectTransform> _slots = new();
        private readonly List<CardVisualLightUI> _visuals = new();
        private readonly List<Tween> _tweens = new();
        private CancellationTokenSource _animCts;
        
        private Card _cardCache;
        private (CardColor, int) _attackValue;
        private Direction _moveValue;

        public void Init()
        {
            _bar = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

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

            _rect.anchoredPosition = openPosition;
            gameObject.SetActive(true);
            
            // 애니메이션
            _animCts = new CancellationTokenSource();
            DoAnim(_animCts.Token).Forget();
        }

        public void Close()
        {
            if (!gameObject.activeSelf)
                return;
            
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
            
            foreach(var tween in _tweens)
                tween?.Kill();
            
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
                Managers.Resource.Destroy(_visuals[i].gameObject);
            }
            
            _visuals.Clear();
            _tweens.Clear();
            _cardCache = null;
        }

        private void OnValueSelected((int, Direction) values)
        {
            ValueSelected?.Invoke(_cardCache, values);
            Close();
        }

        private async UniTaskVoid DoAnim(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                // 외관 초기화
                _bar.color -= new Color(0, 0, 0, _bar.color.a);
                foreach (var visual in _visuals)
                    visual.Rect.localScale = Vector3.zero;

                // 애니메이션 처리
                var dur = .4f;
                var dur2 = .2f;
                var interval = .06f;
                Tween tween;

                tween = _bar.DOFade(1f, dur);
                _tweens.Add(tween);
                await tween.ToUniTask(cancellationToken: ct);

                foreach (var visual in _visuals)
                {
                    ct.ThrowIfCancellationRequested();
                    tween = visual.Rect.DOScale(CardScale, dur2)
                        .SetEase(Ease.OutBack);
                    _tweens.Add(tween);
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _animCts?.Dispose();
                _animCts = null;
            }
        } 
        
        /// <summary>
        /// <see cref="_bar"/> 크기를 조정.
        /// </summary>
        private void ConfigureFrame(int slotCount)
        {
            if (!_frameWidths.TryGetValue(slotCount, out var width))
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
                var slot = Managers.Resource.Instantiate(SlotPath, _rect).GetComponent<RectTransform>();
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
        /// 카드 UI 생성 및 데이터 바인딩
        /// </summary>
        private void ConfigureCards(CardData data, int count)
        {
            const string path = "UI/CardUI/CardVisual_Light_UI";

            for (int i = 0; i < count; i++)
            {
                var go = Managers.Resource.Instantiate(path, _slots[i]);
                var visual = go.GetComponent<CardVisualLightUI>();
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