using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using System;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    /// <summary>
    /// 매 스테이지마다 초기화되는 덱 
    /// </summary>
    [Serializable]
    public class InStageCards
    {
        public List<CardData> Deck;
        public List<CardData> Discards;
        public List<Card> Hands;
        public List<Card> Selects;

        public enum SortType { None, Number, Icon }
        public SortType sortType = SortType.None;

        private static readonly IComparer<Card> NumberComparer = Comparer<Card>.Create((a, b) =>
        {
            return a.data.Number.number.CompareTo(b.data.Number.number);
        });

        private static readonly IComparer<Card> IconComparer = Comparer<Card>.Create((a, b) =>
        {
            return a.data.id.CompareTo(b.data.id);
        });

        /// <summary>
        /// 오로지 덱의 상태만 전달함. 플레이어 턴 여부 등은 고려 안 함.
        /// </summary>
        public bool CanUseCard => SelectCount > 0 && AllValueSelected;

        public int DeckCount => Deck.Count();
        public int HandCount => Hands.Count();
        public int SelectCount => Selects.Count();
        private bool AllValueSelected => Selects.All(c => c.data.CanUse);        

        public List<Card> SortedSelect => Selects.OrderBy(c => Hands.IndexOf(c)).ToList();

        public Card GetHandCard(int index) => Hands[index];


        public void Sort()
        {
            if (sortType == SortType.None)
                return;
            var comparer = sortType == SortType.Number ? NumberComparer : IconComparer;
            Hands.Sort(comparer);
        }

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
            (Hands[indexA], Hands[indexB]) = (Hands[indexB], Hands[indexA]);
        }

        public void Draw(Card card)
        {
            Hands.Add(card);
            Sort();
        }

        public void Discard(Card card, float interval)
        {
            Hands.Remove(card);
            Selects.Remove(card);
            Discards.Add(card.data);
            card.Discard(interval);
        }



        public InStageCards(List<CardData> deck)
        {
            Deck = deck;
            Discards = new();
            Hands = new();
            Selects = new();
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
        public CardData DrawCard()
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
            foreach (var card in Hands)
                card.cardVisual.UpdateIndex();
        }

        public void SetSortNone()
        {
            sortType = SortType.None;
        }

        public void SetSortByNumber()
        {
            sortType = SortType.Number;
            Sort();
        }

        public void SetSortByIcon()
        {
            sortType = SortType.Icon;
            Sort();
        }
    }
}
