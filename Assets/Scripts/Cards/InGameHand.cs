using Cardevil.Cards.CardInteractinos;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Cardevil.Cards
{
    public class InGameHand
    {
        private readonly List<Card> hands = new();
        private readonly List<Card> selects = new();

        public List<Card> Selects => selects;

        public int HandCount => hands.Count();
        public int SelectCount => selects.Count();

        public List<Card> SortedSelect => selects.OrderBy(c => c.GetSlotIndex()).ToList();
        public Card GetCard(int index) => hands[index];

        public void Draw(Card card) => hands.Add(card);        
        public void Select(Card card) => selects.Add(card);
        public void Deselect(Card card) => selects.Remove(card);
        public void Discard(Card card, float interval)
        {
            hands.Remove(card);
            selects.Remove(card);
            card.Discard(interval);
        }

        /// <summary>
        /// 선택한 카드들로 사용 가능 여부를 반환.
        /// </summary>
        public bool AllValueSelected => selects.All(c => c.data.CanUse);
        

        public async UniTask Discard(float interval, Transform[] slots)
        {
            foreach (var card in SortedSelect)
            {
                var slotIndex = card.GetSlotIndex();
                slots[slotIndex].gameObject.SetActive(false);

                Discard(card, interval);
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
                card.Destroy(); // TODO: 이벤트 구독 해지 로직/오브젝트 풀 관련 로직 추가
            }

            selects.Clear();
        }

        public void UpdateVisualIndex()
        {
            foreach (var card in hands)
                card.cardVisual.UpdateIndex(card.GetSlotIndex());
        }
    }
}
