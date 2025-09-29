using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cardevil.Cards.Interactions;
using System;
using Random = UnityEngine.Random;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;

namespace Cardevil.Cards
{
    /// <summary>
    /// 매 스테이지에서 사용되는 덱 정보를 담음.
    /// 손패, 버려진패 등의 정보도 포함. 
    /// </summary>
    [Serializable]
    public class StageCardsContext : IClearable
    {
        public List<CardData> Deck = new();
        public List<CardData> Discards = new();
        public List<Card> Hand = new();
        public List<Card> Selects = new();

        private int _discardRemainCount = 3;

        /// <summary>
        /// 모든 카드를 덱으로 돌려넣고 섞음.
        /// </summary>
        public void Shuffle()
        {
            Deck.AddRange(Discards);
            Deck.AddRange(Hand.Select(c => c.data));

            Discards.Clear();
            Hand.Clear();

            if (Deck.Count != 50)
            {
                Debug.LogError("덱의 초기화에 실패했습니다.");
                Clear();
                return;
            }

            DeckFactory.Shuffle(Deck);
        }

        public void Clear()
        {
            Deck = DeckFactory.CreateStageDeck(Managers.Card.RuntimeBaseDeck);
            _discardRemainCount = 3;
        }



        /// <summary>
        /// 오로지 덱의 상태만 전달함. 플레이어 턴 여부 등은 고려 안 함.
        /// </summary>
        public bool CanUseCard => SelectCount > 0 && AllValueSelected;

        /// <summary>
        /// 족보와 보너스 점수만을 반환.
        /// </summary>
        public string Description
        {
            get
            {
                var ranking = CardResultEvaluator.GetRanking(Selects);
                if (ranking == HandRanking.None || ranking == HandRanking.High)
                    return "";
                else
                {
                    var datas = Managers.Database.Database;
                    // var rankingData = datas.HandRankingDataList
                    //     .FirstOrDefault(d => d.Ranking == ranking);

                    // if (rankingData == null)
                    //     Debug.LogError($"RankingData가 존재하지 않습니다 : {ranking}");

                    // return $"{rankingData.DisplayName}\n{rankingData.Value}";
                    return "";
                }

            }
        }

        /// <summary> 버리기 남은 횟수. </summary>
        public int DiscardRemainCount => _discardRemainCount;

        public int DeckCount => Deck.Count;

        public int HandCount => Hand.Count;

        public int SelectCount => Selects.Count;

        public int DiscardCount => Discards.Count;

        public List<Card> SortedSelect => Selects.OrderBy(c => Hand.IndexOf(c)).ToList();

        private bool AllValueSelected => Selects.All(c => c.data.CanUse);




        public Card GetHandCard(int index) => Hand[index];

        public void Select(Card card)
        {
            Selects.Add(card);
        }

        public void Deselect(Card card)
        {
            Selects.Remove(card);
        }

        public void Swap(int indexA, int indexB)
        {
            (Hand[indexA], Hand[indexB]) = (Hand[indexB], Hand[indexA]);
        }

        public void Draw(Card card)
        {
            Hand.Add(card);
        }

        public void Discard(Card card)
        {
            Hand.Remove(card);
            Selects.Remove(card);
            Discards.Add(card.data);
            card.Discard();
        }

        public void IncreaseDiscardCount(int amount)
        {
            _discardRemainCount += amount;
        }

        public bool ReduceDiscardCount(int amount = 1)
        {
            _discardRemainCount -= amount;
            return DiscardRemainCount > 0;
        }

        public void SortByNumber()
        {
            Hand = Hand
                .OrderBy(c => ValueTypeRank(c))

                // 이동카드 정렬
                .ThenBy(c => MoveSelectTypeRank(c))
                .ThenBy(c => DirectionRank(c))

                // 숫자카드 정렬
                .ThenBy(c => NumberSelectTypeRank(c))
                .ThenBy(c => c.data.Number.NumberValue)
                .ThenBy(c => c.data.Number.ColorValue)

                .ToList();
        }

        public void SortByIcon()
        {
            Hand = Hand
                .OrderBy(c => ValueTypeRank(c))

                // 이동카드 정렬
                .ThenBy(c => MoveSelectTypeRank(c))
                .ThenBy(c => DirectionRank(c))

                // 숫자카드 정렬
                .ThenBy(c => c.data.Number.ColorValue)
                .ThenBy(c => NumberSelectTypeRank(c))
                .ThenBy(c => c.data.Number.NumberValue)

                .ToList();
        }

        /// <summary>
        /// 사용된 카드를 다시 덱에 넣음.
        /// </summary>
        public void Revive()
        {
            var randomDiscardIndex = Random.Range(0, maxExclusive: DiscardCount);
            var randomDeckIndex = Random.Range(0, DeckCount);

            var card = Discards[randomDiscardIndex];
            Deck.Insert(randomDeckIndex, card);

            Discards.Remove(card);
        }

        /// <summary>
        /// 덱에서 랜덤한 카드를 반환하기만 함. 삭제하진 않음.
        /// </summary>
        public CardData GetRandomCard()
        {
            int randomIndex = Random.Range(0, DeckCount);
            return Deck[randomIndex];
        }

        /// <summary>
        /// 덱의 첫 카드를 반환하고 덱에서 삭제함.
        /// </summary>
        public CardData PopCard()
        {
            if (Deck.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = Deck[0];
            Deck.RemoveAt(0);
            cardData.OnDraw();

            return cardData;
        }

        public void UpdateVisualIndex()
        {
            foreach (var card in Hand)
                card.CardVisual.UpdateIndex();
        }

        #region Sorting Helper

        private static int ValueTypeRank(Card c)
        {
            return c.data.valueType == CardData.ValueType.Move ? 0 : 1;
        }

        private static int MoveSelectTypeRank(Card c)
        {
            if (c.data.selectType == CardData.SelectType.None || c.data.IsValueSelected)
                return 0;
            else
                return (int)c.data.selectType;
        }

        private static int NumberSelectTypeRank(Card c)
        {
            if (c.data.selectType == CardData.SelectType.None
                || (c.data.IsValueSelected && c.data.selectType == CardData.SelectType.Multiple))
                return -1;
            else if (c.data.selectType == CardData.SelectType.All && c.data.IsValueSelected)
                return 0;
            else if (c.data.selectType == CardData.SelectType.Multiple)
                return c.data.NumberOptionCount;
            else if (c.data.selectType == CardData.SelectType.All)
                return (int)CardData.SelectType.All;
            return 100;
        }

        private static int DirectionRank(Card c)
        {
            return c.data.Move.DirectionValue switch
            {
                Utils.Directions.Direction.Up => 0,
                Utils.Directions.Direction.Down => 1,
                Utils.Directions.Direction.Left => 2,
                Utils.Directions.Direction.Right => 3,
                _ => 4,
            };
        }
        
        #endregion
    }
}
