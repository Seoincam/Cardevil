using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.InStage;
using Cardevil.Utils.Directions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Cardevil.Test.Card
{
    public class HandRankEvaluatorTests
    {
        private static CardState CreateAttackCard(uint id, CardColor color, int number)
        {
            return new CardSpec(id, CardType.Attack)
                .AddElements(
                    new BaseColorElement(color),
                    new BaseNumberElement(number)
                )
                .State;
        }

        private static CardState CreateMoveCard(uint id, Direction direction)
        {
            return new CardSpec(id, CardType.Move)
                .AddElements(
                    new BaseDirectionElement(direction)
                )
                .State;
        }

        // --- None ---

        [Test]
        public void Returns_None_WhenValueNotSelected()
        {
            // SelectableNumberElement가 있지만 Select하지 않은 상태
            var state = new CardSpec(1, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    SelectableNumberElement.Fixed(1)
                )
                .State;

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { state });

            Assert.AreEqual(HandRank.None, result.HandRank);
        }

        // --- HighCard ---

        [Test]
        public void Returns_HighCard_WhenSingleAttackCard()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 5)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
        }

        [Test]
        public void Returns_HighCard_WhenNoPairsNoStraightNoFlush()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 1),
                CreateAttackCard(2, CardColor.Blue, 3),
                CreateAttackCard(3, CardColor.Green, 6),
                CreateAttackCard(4, CardColor.Black, 9)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
        }

        // --- OnePair ---

        [Test]
        public void Returns_OnePair_WhenTwoCardsSameNumber()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 3),
                CreateAttackCard(2, CardColor.Blue, 3),
                CreateAttackCard(3, CardColor.Green, 5)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.OnePair, result.HandRank);
        }

        // --- TwoPair ---

        [Test]
        public void Returns_TwoPair_WhenTwoPairsExist()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 3),
                CreateAttackCard(2, CardColor.Blue, 3),
                CreateAttackCard(3, CardColor.Green, 7),
                CreateAttackCard(4, CardColor.Black, 7)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.TwoPair, result.HandRank);
        }

        // --- Triple ---

        [Test]
        public void Returns_Triple_WhenThreeCardsSameNumber()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 5),
                CreateAttackCard(2, CardColor.Blue, 5),
                CreateAttackCard(3, CardColor.Green, 5)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.Triple, result.HandRank);
        }

        // --- Straight ---

        [Test]
        public void Returns_Straight_WhenFourConsecutiveNumbers_DifferentColors()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 3),
                CreateAttackCard(2, CardColor.Blue, 4),
                CreateAttackCard(3, CardColor.Green, 5),
                CreateAttackCard(4, CardColor.Black, 6)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.Straight, result.HandRank);
        }

        [Test]
        public void Returns_HighCard_WhenFourNumbers_NotConsecutive()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 1),
                CreateAttackCard(2, CardColor.Blue, 2),
                CreateAttackCard(3, CardColor.Green, 3),
                CreateAttackCard(4, CardColor.Black, 5) // gap
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
        }

        // --- FourCard ---

        [Test]
        public void Returns_FourCard_WhenFourCardsSameNumber_DifferentColors()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 7),
                CreateAttackCard(2, CardColor.Blue, 7),
                CreateAttackCard(3, CardColor.Green, 7),
                CreateAttackCard(4, CardColor.Black, 7)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.FourCard, result.HandRank);
        }

        // --- TwoPairFlush ---

        [Test]
        public void Returns_TwoPairFlush_WhenTwoPairsAndFlush()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 3),
                CreateAttackCard(2, CardColor.Red, 3),
                CreateAttackCard(3, CardColor.Red, 7),
                CreateAttackCard(4, CardColor.Red, 7)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.TwoPairFlush, result.HandRank);
            Assert.AreEqual(CardColor.Red, result.CardColor);
        }

        // --- StraightFlush ---

        [Test]
        public void Returns_StraightFlush_WhenStraightAndFlush()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Blue, 4),
                CreateAttackCard(2, CardColor.Blue, 5),
                CreateAttackCard(3, CardColor.Blue, 6),
                CreateAttackCard(4, CardColor.Blue, 7)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.StraightFlush, result.HandRank);
            Assert.AreEqual(CardColor.Blue, result.CardColor);
        }

        // --- FourCardFlush ---

        [Test]
        public void Returns_FourCardFlush_WhenFourOfAKindAndFlush()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Green, 9),
                CreateAttackCard(2, CardColor.Green, 9),
                CreateAttackCard(3, CardColor.Green, 9),
                CreateAttackCard(4, CardColor.Green, 9)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.FourCardFlush, result.HandRank);
            Assert.AreEqual(CardColor.Green, result.CardColor);
        }

        // --- Move 카드 무시 ---

        [Test]
        public void IgnoresMoveCards_EvaluatesAttackOnly()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 5),
                CreateAttackCard(2, CardColor.Blue, 5),
                CreateMoveCard(3, Direction.Up)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.OnePair, result.HandRank);
        }

        // --- Straight는 4장이 아니면 불가 ---

        [Test]
        public void Returns_HighCard_WhenThreeConsecutive_NotStraight()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 3),
                CreateAttackCard(2, CardColor.Blue, 4),
                CreateAttackCard(3, CardColor.Green, 5)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
        }

        // --- Flush는 4장이 아니면 불가 ---

        [Test]
        public void Returns_HighCard_WhenThreeSameColor_NotFlush()
        {
            var cards = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 1),
                CreateAttackCard(2, CardColor.Red, 3),
                CreateAttackCard(3, CardColor.Red, 6)
            };

            var result = HandRankEvaluator.GetHandRank(cards);

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
        }

        // --- 우선순위: FourCardFlush > StraightFlush ---

        [Test]
        public void FourCardFlush_HasHigherPriority_ThanStraightFlush()
        {
            // FourCardFlush 확인 (4장 같은 숫자 + 같은 색)
            var fourCardFlush = new List<ICardState>
            {
                CreateAttackCard(1, CardColor.Red, 5),
                CreateAttackCard(2, CardColor.Red, 5),
                CreateAttackCard(3, CardColor.Red, 5),
                CreateAttackCard(4, CardColor.Red, 5)
            };

            var result = HandRankEvaluator.GetHandRank(fourCardFlush);

            Assert.AreEqual(HandRank.FourCardFlush, result.HandRank);
        }

        // --- SelectableNumber 선택 후 족보 평가 ---

        [Test]
        public void Returns_OnePair_WhenSelectableNumberSelected()
        {
            var spec = new CardSpec(1, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(3),
                    SelectableNumberElement.Fixed(5)
                );
            var state1 = spec.State;
            state1.Numbers.Select(3);

            var state2 = CreateAttackCard(2, CardColor.Blue, 3);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { state1, state2 });

            Assert.AreEqual(HandRank.OnePair, result.HandRank);
        }

        // ============================================================
        // RankedCards 검증
        // ============================================================

        [Test]
        public void RankedCards_None_IsEmpty()
        {
            var state = new CardSpec(1, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    SelectableNumberElement.Fixed(1)
                )
                .State;

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { state });

            Assert.AreEqual(HandRank.None, result.HandRank);
            Assert.IsEmpty(result.RankedCards);
        }

        [Test]
        public void RankedCards_HighCard_ContainsSingleCard()
        {
            var card = CreateAttackCard(1, CardColor.Red, 5);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { card });

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
            Assert.AreEqual(1, result.RankedCards.Count);
            CollectionAssert.Contains(result.RankedCards, card);
        }

        [Test]
        public void RankedCards_HighCard_ContainsHighestAttackCards()
        {
            var a = CreateAttackCard(1, CardColor.Red, 1);
            var b = CreateAttackCard(2, CardColor.Blue, 3);
            var c = CreateAttackCard(3, CardColor.Green, 6);
            var d = CreateAttackCard(4, CardColor.Black, 9);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.HighCard, result.HandRank);
            Assert.AreEqual(1, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_OnePair_ContainsOnlyPairedCards()
        {
            var pair1 = CreateAttackCard(1, CardColor.Red, 3);
            var pair2 = CreateAttackCard(2, CardColor.Blue, 3);
            var other = CreateAttackCard(3, CardColor.Green, 7);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { pair1, pair2, other });

            Assert.AreEqual(HandRank.OnePair, result.HandRank);
            Assert.AreEqual(2, result.RankedCards.Count);
            CollectionAssert.Contains(result.RankedCards, pair1);
            CollectionAssert.Contains(result.RankedCards, pair2);
            CollectionAssert.DoesNotContain(result.RankedCards, other);
        }

        [Test]
        public void RankedCards_TwoPair_ContainsAllPairedCards()
        {
            var a = CreateAttackCard(1, CardColor.Red, 3);
            var b = CreateAttackCard(2, CardColor.Blue, 3);
            var c = CreateAttackCard(3, CardColor.Green, 7);
            var d = CreateAttackCard(4, CardColor.Black, 7);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.TwoPair, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_Triple_ContainsOnlyTripleCards()
        {
            var t1 = CreateAttackCard(1, CardColor.Red, 5);
            var t2 = CreateAttackCard(2, CardColor.Blue, 5);
            var t3 = CreateAttackCard(3, CardColor.Green, 5);
            var other = CreateAttackCard(4, CardColor.Black, 9);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { t1, t2, t3, other });

            Assert.AreEqual(HandRank.Triple, result.HandRank);
            Assert.AreEqual(3, result.RankedCards.Count);
            CollectionAssert.Contains(result.RankedCards, t1);
            CollectionAssert.Contains(result.RankedCards, t2);
            CollectionAssert.Contains(result.RankedCards, t3);
            CollectionAssert.DoesNotContain(result.RankedCards, other);
        }

        [Test]
        public void RankedCards_Straight_ContainsAllCards()
        {
            var a = CreateAttackCard(1, CardColor.Red, 3);
            var b = CreateAttackCard(2, CardColor.Blue, 4);
            var c = CreateAttackCard(3, CardColor.Green, 5);
            var d = CreateAttackCard(4, CardColor.Black, 6);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.Straight, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_FourCard_ContainsAllCards()
        {
            var a = CreateAttackCard(1, CardColor.Red, 7);
            var b = CreateAttackCard(2, CardColor.Blue, 7);
            var c = CreateAttackCard(3, CardColor.Green, 7);
            var d = CreateAttackCard(4, CardColor.Black, 7);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.FourCard, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_TwoPairFlush_ContainsAllCards()
        {
            var a = CreateAttackCard(1, CardColor.Red, 3);
            var b = CreateAttackCard(2, CardColor.Red, 3);
            var c = CreateAttackCard(3, CardColor.Red, 7);
            var d = CreateAttackCard(4, CardColor.Red, 7);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.TwoPairFlush, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_StraightFlush_ContainsAllCards()
        {
            var a = CreateAttackCard(1, CardColor.Blue, 4);
            var b = CreateAttackCard(2, CardColor.Blue, 5);
            var c = CreateAttackCard(3, CardColor.Blue, 6);
            var d = CreateAttackCard(4, CardColor.Blue, 7);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.StraightFlush, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_FourCardFlush_ContainsAllCards()
        {
            var a = CreateAttackCard(1, CardColor.Green, 9);
            var b = CreateAttackCard(2, CardColor.Green, 9);
            var c = CreateAttackCard(3, CardColor.Green, 9);
            var d = CreateAttackCard(4, CardColor.Green, 9);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { a, b, c, d });

            Assert.AreEqual(HandRank.FourCardFlush, result.HandRank);
            Assert.AreEqual(4, result.RankedCards.Count);
            CollectionAssert.AreEquivalent(new ICardState[] { a, b, c, d }, result.RankedCards);
        }

        [Test]
        public void RankedCards_MoveCardsNotIncluded()
        {
            var attack1 = CreateAttackCard(1, CardColor.Red, 5);
            var attack2 = CreateAttackCard(2, CardColor.Blue, 5);
            var move = CreateMoveCard(3, Direction.Up);

            var result = HandRankEvaluator.GetHandRank(new List<ICardState> { attack1, attack2, move });

            Assert.AreEqual(HandRank.OnePair, result.HandRank);
            Assert.AreEqual(2, result.RankedCards.Count);
            CollectionAssert.DoesNotContain(result.RankedCards, move);
        }
    }
}