using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using Cysharp.Threading.Tasks;
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

        public int DeckCount => Deck.Count();
        public int HandCount => Hands.Count();
        public int SelectCount => Selects.Count();

        public List<Card> SortedSelect => Selects.OrderBy(c => Hands.IndexOf(c)).ToList();

        public void Swap(int indexA, int indexB)
        {
            (Hands[indexA], Hands[indexB]) = (Hands[indexB], Hands[indexA]);
        }

        public void Draw(Card card)
        {
            if (HandCount >= 6)
                Hands.Insert(HandCount / 2, card);
            else
                Hands.Add(card);
        }

        public void Select(Card card) => Selects.Add(card);
        public void Deselect(Card card) => Selects.Remove(card);
        public void Discard(Card card, float interval)
        {
            Hands.Remove(card);
            Selects.Remove(card);
            Discards.Add(card.data);
            card.Discard(interval);
        }
        public Card GetCard(int index) => Hands[index];

        // 오로지 덱의 상태만 전달 -> 플레이어 턴 여부 등은 고려x
        public bool CanUseCard => SelectCount > 0 && AllValueSelected;

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

        /// <summary>
        /// 선택한 카드들로 사용 가능 여부를 반환.
        /// </summary>
        public bool AllValueSelected => Selects.All(c => c.data.CanUse);


        public async UniTask Discard(float interval, Transform[] slots)
        {
            foreach (var card in SortedSelect)
            {
                Discard(card, interval);
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
                card.Destroy(); // TODO: 이벤트 구독 해지 로직/오브젝트 풀 관련 로직 추가
            }

            Selects.Clear();
        }

        public void UpdateVisualIndex()
        {
            foreach (var card in Hands)
                card.cardVisual.UpdateIndex();
        }

    }
}
