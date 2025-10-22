using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core;
using Cardevil.Utils;

namespace Cardevil.Cards.InStage.Model
{
    /// <summary>
    /// 매 스테이지에서 사용되는 카드 시스템의 상태(Model).
    /// 덱/버린 패/손 패/선택을 관리하고, 정렬/스왑 등 로직을 제공.
    /// </summary>
    [Serializable]
    public class StageCardsModel : IReadOnlyStageCardsModel, IClearable
    {
        private List<CardData> _deck = new();
        private List<CardData> _discardPile = new();
        private List<Card> _hand = new();
        private HashSet<Card> _selection = new();

        #region IReadOnlyStageCardsModel
        
        public event Action HandChanged;
        
        public int MaxHand { get; private set; }
        public int DiscardRemain { get; private set; }
        
        public IReadOnlyList<CardData> Deck => _deck;
        public IReadOnlyList<CardData> DiscardPile => _discardPile;
        public IReadOnlyList<Card> Hand => _hand;
        public IReadOnlyCollection<Card> Selection => _selection;
        public IReadOnlyList<Card> SortedSelection => _selection.OrderBy(c => _hand.IndexOf(c)).ToList();
        
        public bool TryGetIndex(Card card, out int index)
        {
            index = -1;
            if (!card || _hand == null) return false;
            index = _hand.IndexOf(card);
            return index >= 0;
        }
        
        #endregion
        
        /// <summary>
        /// 덱 상태만 기준으로 카드를 사용할 수 있는지 여부를 반환.
        /// (플레이어 턴/입력 가능 여부 등은 고려하지 않음)
        /// </summary>
        public bool CanUseCard
        {
            get
            {
                if (_selection.Count == 0)
                    return false;

                foreach (var card in _selection)
                {
                    var data = card.Data;
                    if (data.Kind == CardKind.Attack && data.NumberSelectState.FinalValue == null)
                        continue;
                    if (data.Kind == CardKind.Move && data.DirectionSelectState.FinalValue == null)
                        continue;

                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 주어진 카드 목록으로 스테이지 덱을 설정,
        /// 최대 손패 수와 초기 버리기 횟수를 설정
        /// </summary>
        public void SetUp(List<CardData> cards, int maxHand, int initialDiscardCount)
        {
            _deck = cards;
            MaxHand = maxHand;
            DiscardRemain = initialDiscardCount;
        }
        
        /// <summary>
        /// 모든 카드를 덱으로 되돌린 뒤 섞음.
        /// 내부적으로 버린 패/손패를 비우고, 덱을 셔플.
        /// </summary>
        public void Shuffle()
        {
            if (_discardPile.Count != 0)
            {
                _deck.AddRange(_discardPile);
                _discardPile.Clear();
            }
            if (_hand.Count != 0)
            {
                _deck.AddRange(_hand.Select(c => c.Data));
                _hand.Clear();
            }
            
            if (_deck.Count != 50)
            {
                Debug.LogError("덱의 초기화에 실패했습니다.");
                Clear();
                return;
            }

            // DeckFactory.Shuffle(_deck);
        }
        
        /// <summary>
        /// 모델의 내부 상태를 초기화.
        /// 덱/버린 패/손패/선택을 모두 비우고, 버리기 잔여 횟수를 초기화.
        /// </summary>
        public void Clear()
        {
            _deck.Clear();
            _discardPile.Clear();
            _hand.Clear();
            _selection.Clear();
            
            DiscardRemain = 3;
        }

        public Card GetHandCard(int index) => _hand[index];
        
        /// <summary>
        /// 카드를 선택 집합에 추가.
        /// </summary>
        public void Select(Card card)
        {
            _selection.Add(card);
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 카드를 선택 집합에서 제거.
        /// </summary>
        public void Deselect(Card card)
        {
            _selection.Remove(card);
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 손패 내 카드의 위치를 교환.
        /// </summary>
        /// <param name="a">교환할 카드 A</param>
        /// <param name="indexB">교환할 대상 인덱스 B</param>
        public void Swap(Card a, int indexB)
        {
            if (!TryGetIndex(a, out var indexA)) return;
            (_hand[indexA], _hand[indexB]) = (_hand[indexB], _hand[indexA]);
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 손패에 카드를 추가. (드로우 처리)
        /// </summary>
        public void Draw(Card card)
        {
            _hand.Add(card);
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 카드를 손패에서 제거하고, 선택 해제 후 버린 패 더미에 추가.
        /// </summary>
        public void Discard(Card card)
        {
            _hand.Remove(card);
            _selection.Remove(card);
            _discardPile.Add(card.Data);
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 버리기(Discard) 가능 횟수를 증가.
        /// </summary>
        /// <param name="amount">증가량</param>
        public void IncreaseDiscardRemainCount(int amount)
        {
            DiscardRemain += amount;
        }
        
        /// <summary>
        /// 버리기 가능 횟수를 감소 후,
        /// 감소 후 잔여 횟수가 0보다 큰지 여부를 반환.
        /// TODO: 테스트 용으로 아직 방어 로직 없음. 추가해야함.
        /// </summary>
        /// <param name="amount">감소량(기본 1)</param>
        /// <returns>감소 후 잔여 횟수가 0보다 크면 true</returns>
        public bool TryReduceDiscardRemainCount(int amount = 1)
        {
            DiscardRemain -= amount;
            return DiscardRemain > 0;
        }
        
        /// <summary>
        /// 손패를 숫자 우선 규칙에 따라 정렬.
        /// </summary>
        public void SortByNumber()
        {
            _hand = _hand
                .OrderBy(ValueTypeOrder)

                // 이동카드 정렬
                .ThenBy(MoveSelectTypeOrder)
                .ThenBy(DirectionOrder)

                // 숫자카드 정렬
                .ThenBy(NumberSelectTypeOrder)
                // .ThenBy(c => c.Data.Number.SelectState.FinalValue)
                .ThenBy(NumberSelectedValueOrder)

                .ToList();
            HandChanged?.Invoke();
        }
        
        /// <summary>
        /// 손패를 아이콘(색상) 우선 규칙에 따라 정렬.
        /// </summary>
        public void SortByIcon()
        {
            _hand = _hand
                .OrderBy(ValueTypeOrder)

                // 이동카드 정렬
                .ThenBy(MoveSelectTypeOrder)
                .ThenBy(DirectionOrder)

                // 숫자카드 정렬
                .ThenBy(NumberColorOrder)
                .ThenBy(NumberSelectTypeOrder)
                .ThenBy(NumberSelectedValueOrder)

                .ToList();
            HandChanged?.Invoke();
        }

        /// <summary>
        /// 사용된 카드를 다시 덱에 넣음.
        /// </summary>
        public void Revive()
        {
            var randomDiscardIndex = Random.Range(0, maxExclusive: _discardPile.Count);
            var randomDeckIndex = Random.Range(0, _deck.Count);

            var card = _discardPile[randomDiscardIndex];
            _deck.Insert(randomDeckIndex, card);

            _discardPile.Remove(card);
        }

        /// <summary>
        /// 덱에서 랜덤한 카드를 반환하기만 함. 삭제하진 않음.
        /// </summary>
        public CardData GetRandomCard()
        {
            int randomIndex = Random.Range(0, _deck.Count);
            return _deck[randomIndex];
        }

        /// <summary>
        /// 덱의 첫 카드를 반환하고 덱에서 삭제함.
        /// </summary>
        public CardData PopCard()
        {
            if (_deck.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = _deck[0];
            _deck.RemoveAt(0);
            // cardData.OnDraw();

            return cardData;
        }

        #region Sorting Helper
        
        private static int MoveSelectTypeOrder(Card c)
        {
            if (c.Data.Kind != CardKind.Move)
                return int.MaxValue;

            return c.Data.DirectionSelectState.Selectables.Count;
        }
        
        private static int DirectionOrder(Card c)
        {
            if (c.Data.Kind != CardKind.Move)
                return int.MaxValue;
            if (!c.Data.DirectionSelectState.FinalValue.HasValue)
                return 5;
            
            return c.Data.DirectionSelectState.FinalValue switch
            {
                Utils.Directions.Direction.Up => 0,
                Utils.Directions.Direction.Down => 1,
                Utils.Directions.Direction.Left => 2,
                Utils.Directions.Direction.Right => 3,
                _ => 4,
            };
        }

        private static int ValueTypeOrder(Card c)
        {
            return c.Data.Kind == CardKind.Move ? 0 : 1;
        }
        
        private static int NumberSelectTypeOrder(Card c)
        {
            if (c.Data.Kind != CardKind.Attack)
                return int.MinValue;

            return c.Data.NumberSelectState.Selectables.Count;
        }

        private static int NumberColorOrder(Card c)
        {
            if (c.Data.Kind != CardKind.Attack)
                return int.MinValue;

            return (int)c.Data.Color;
        }

        private static int NumberSelectedValueOrder(Card c)
        {
            if (c.Data.Kind != CardKind.Attack)
                return int.MinValue;

            if (!c.Data.NumberSelectState.FinalValue.HasValue)
                return int.MaxValue;

            return (int)c.Data.NumberSelectState.FinalValue;
        }
        
        #endregion
    }
}
