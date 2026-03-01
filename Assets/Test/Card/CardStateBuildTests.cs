using Cardevil.Card.Common.Core;
using Cardevil.Utils.Directions;
using NUnit.Framework;

namespace Cardevil.Test.Card
{
    public class CardStateBuildTests
    {
        // 기본 Color, Number 빌드 테스트
        [Test]
        public void AttackCard_Builds_DefaultColor_And_Number()
        {
            var spec = new CardSpec(1, CardType.Attack);
            spec.AddElements(
                new BaseColorElement(CardColor.Red),
                new BaseNumberElement(3)
            );

            var state = spec.State;

            Assert.NotNull(state.Colors);
            Assert.NotNull(state.Numbers);

            Assert.AreEqual(CardColor.Red, state.Colors.DefaultValue);
            Assert.AreEqual(3, state.Numbers.DefaultValue);

            Assert.IsFalse(state.Colors.HasAlternatives);
            Assert.IsFalse(state.Numbers.HasAlternatives);
        }

        // Attack Card - Alternative Color 추가 테스트
        [Test]
        public void AttackCard_WithAlternativeColor_AddsSelectableOption()
        {
            var spec = new CardSpec(2, CardType.Attack);
            spec.AddElements(
                new BaseColorElement(CardColor.Red),
                new BaseNumberElement(3),
                SelectableColorElement.Fixed(CardColor.Blue)
            );

            var state = spec.State;

            Assert.IsTrue(state.Colors.HasAlternatives);
            Assert.Contains(CardColor.Blue, (System.Collections.ICollection)state.Colors.Alternatives);
        }

        // SelectableValues 선택 로직 테스트
        [Test]
        public void SelectableValues_Select_ChangesCurrentValue()
        {
            var selectable = new CardState.SelectableValues<CardColor>(CardColor.Red);
            selectable.AddAlternative(CardColor.Blue);

            selectable.Select(CardColor.Blue);

            Assert.AreEqual(CardColor.Blue, selectable.Current);
        }

        // Move 카드 생성 테스트
        [Test]
        public void MoveCard_Builds_Direction_Only()
        {
            var spec = new CardSpec(3, CardType.Move);
            spec.AddElements(
                new BaseDirectionElement(Direction.Up)
            );

            var state = spec.State;

            Assert.NotNull(state.Directions);
            Assert.AreEqual(Direction.Up, state.Directions.DefaultValue);

            Assert.IsNull(state.Colors);
            Assert.IsNull(state.Numbers);
        }

        // Spec 더티 플러그 체크
        [Test]
        public void CardSpec_State_IsCached_UntilDirty()
        {
            var spec = new CardSpec(4, CardType.Attack);
            spec.AddElements(
                new BaseColorElement(CardColor.Red),
                new BaseNumberElement(3)
            );

            var first = spec.State;
            var second = spec.State;

            Assert.AreSame(first, second);
        }
    }
}