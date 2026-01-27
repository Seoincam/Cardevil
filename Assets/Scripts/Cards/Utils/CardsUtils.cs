using Cardevil.Cards.Core;
using Cardevil.Cards.InStage;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Utils
{
    public static class CardsUtils
    {
        // 필요하다면 사용 카드를 list가 아닌 별도의 자료구조로 관리해도 될 듯
        // public static class 별도자료구조Extensions
        // 필요하다면..
        
        public static IReadOnlyList<StageCard> GetAttackCards(this IReadOnlyList<StageCard> cards) =>
            cards.GetCardsByKind(CardKind.Attack);

        public static IReadOnlyList<StageCard> GetMoveCards(this IReadOnlyList<StageCard> cards) =>
            cards.GetCardsByKind(CardKind.Move);

        public static IReadOnlyList<StageCard> GetCardsByKind(this IReadOnlyList<StageCard> cards, CardKind kind) =>
            InternalGetCardByKind(cards, kind).ToArray();

        public static IReadOnlyList<CardData> GetCardsData(this IReadOnlyList<StageCard> cards) => cards
            .Select(c => c.Data)
            .ToArray();
        
        public static IReadOnlyList<CardData> GetAttackCardData(this IReadOnlyList<StageCard> cards) =>
            cards.GetCardsDataByKind(CardKind.Attack);

        public static IReadOnlyList<CardData> GetMoveCardData(this IReadOnlyList<StageCard> cards) =>
            cards.GetCardsDataByKind(CardKind.Move);

        public static IReadOnlyList<CardData> GetCardsDataByKind(this IReadOnlyList<StageCard> cards, CardKind kind) =>
            InternalGetCardByKind(cards, kind)
                .Select(c => c.Data)
                .ToArray();
        
        private static IEnumerable<StageCard> InternalGetCardByKind(IReadOnlyList<StageCard> cards, CardKind kind) => cards?
            .Where(c => c.Data.Kind == kind);
    }
}