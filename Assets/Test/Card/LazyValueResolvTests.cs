using Cardevil.Card.Common.Core;
using Cardevil.Core.Utils;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace Cardevil.Test.Card
{
    [TestFixture]
    public class LazyValueResolveTest
    {
        private const int TEST_ID = 100;
    
        #region 1. Attack Card Resolution Tests
    
        [Test]
        public void AttackCard_WithSingleValues_ShouldBeResolvedImmediately()
        {
            // Setup: Color 1개, Number 1개 (기본값만 존재)
            var elements = new List<ISpecElement> { 
                new BaseColorElement(CardColor.Red),
                new BaseNumberElement(5) 
            };
            var spec = new CardSpec(TEST_ID, CardType.Attack, elements);
    
            // Build
            var state = spec.NewState;
    
            // Verify: 둘 다 1개씩이므로 즉시 Resolved 상태여야 함
            Assert.IsTrue(state.ColorList.IsResolved, "Color가 1개면 즉시 Resolved여야 합니다.");
            Assert.IsTrue(state.NumberList.IsResolved, "Number가 1개면 즉시 Resolved여야 합니다.");
            Assert.IsTrue(state.ValueSelected, "모두 Resolved이므로 ValueSelected는 true여야 합니다.");
            Assert.AreEqual(5, state.NumberList.FixedValue);
        }
    
        [Test]
        public void AttackCard_WithMultipleValues_ShouldBeUnresolved_AndHandleDelayedResolution()
        {
            // Setup: Color는 1개(고정), Number는 3개(1개 기본 + 2개 랜덤)
            // SelectableNumberElement.Random()이 builder.AddNumberAlternative(null)을 호출한다고 가정
            var elements = new List<ISpecElement> { 
                new BaseColorElement(CardColor.Blue),
                new BaseNumberElement(3),
                SelectableNumberElement.Random(), 
                SelectableNumberElement.Random()
            };
            var spec = new CardSpec(TEST_ID, CardType.Attack, elements);
    
            var state = spec.NewState;
    
            // Verify Initial State
            Assert.AreEqual(3, state.NumberList.AllCandidateValues.Count, "후보 값은 총 3개여야 합니다.");
            Assert.IsTrue(state.ColorList.IsResolved, "Color는 1개이므로 Resolved여야 합니다.");
            Assert.IsFalse(state.NumberList.IsResolved, "Number가 여러 개(랜덤 포함)면 Not Resolved여야 합니다.");
            Assert.Contains(null, (System.Collections.ICollection)state.NumberList.AllCandidateValues, "미정인 값(null)이 포함되어야 합니다.");
    
            // Action: 전투 시작 시 해소 호출
            state.ResolveValues();
    
            // Verify Resolved State
            Assert.IsTrue(state.NumberList.IsResolved, "ResolveValues 호출 후에는 모든 값이 채워져야 합니다.");
            Assert.IsFalse(state.NumberList.AllCandidateValues.Any(v => v == null), "더 이상 null이 존재해서는 안 됩니다.");
            
            // 중복 체크: Resolver가 기존 값(3)을 피해서 숫자를 뽑았는지 확인
            var uniqueValues = new HashSet<int?>(state.NumberList.AllCandidateValues);
            Assert.AreEqual(3, uniqueValues.Count, "해소된 값들 사이에 중복이 없어야 합니다.");
        }
    
        #endregion
    
        #region 2. Move Card (Direction) Tests
    
        [Test]
        public void MoveCard_DirectionCount1_ShouldResolveToOppositePair()
        {
            // Setup: 기본 Up + 랜덤 1개 (총 2개 방향이 될 예정)
            var elements = new List<ISpecElement> { 
                new BaseDirectionElement(Direction.Up),
                SelectableDirectionElement.Random() // Count 1인 상황
            };
            var spec = new CardSpec(TEST_ID, CardType.Move, elements);
    
            // Build: Move는 빌드 시점에 ResolveDirections가 호출됨
            var state = spec.NewState;
    
            // Verify
            Assert.IsTrue(state.DirectionList.IsResolved);
            Assert.AreEqual(2, state.DirectionList.AllCandidateValues.Count);
            
            // Resolver 로직: Count 1이면 [Default, Opposite] 반환
            Assert.Contains(Direction.Up, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.Contains(Direction.Down, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.IsTrue(state.DirectionFlag.HasFlag(Direction.Up.ToDirectionFlag()));
            Assert.IsTrue(state.DirectionFlag.HasFlag(Direction.Down.ToDirectionFlag()));
        }

        [Test]
        public void MoveCard_DirectionCount4()
        {
            var elements = new List<ISpecElement>
            {
                SelectableDirectionElement.Fixed(Direction.Up),
                SelectableDirectionElement.Fixed(Direction.Down),
                SelectableDirectionElement.Fixed(Direction.Left),
                SelectableDirectionElement.Fixed(Direction.Right)
            };
            var spec = new CardSpec(TEST_ID, CardType.Move, elements);

            var state = spec.NewState;
            
            Assert.IsTrue(state.DirectionList.IsResolved);
            Assert.AreEqual(4, state.DirectionList.AllCandidateValues.Count);
            
            Assert.Contains(Direction.Up, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.Contains(Direction.Down, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.Contains(Direction.Left, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.Contains(Direction.Right, (System.Collections.ICollection)state.DirectionList.AllCandidateValues);
            Assert.IsTrue(state.DirectionFlag.HasFlag(DirectionFlag.All));
        }
    
        #endregion
    
        #region 3. Logic Consistency Tests
    
        [Test]
        public void Resolution_ShouldKeepOriginalSpecReference()
        {
            // ResolveValues가 인스턴스를 교체할 때 originalSpec 참조를 잃지 않는지 확인
            var spec = new CardSpec(TEST_ID, CardType.Attack, new List<ISpecElement> { SelectableNumberElement.Random() });
            var state = spec.NewState;
    
            state.ResolveValues();
    
            Assert.IsNotNull(state.ColorList, "Resolve 후에도 리스트 인스턴스가 존재해야 합니다.");
            Assert.AreEqual(CardType.Attack, state.Type, "Resolve 후에도 원본 Spec의 타입이 유지되어야 합니다.");
            Assert.AreEqual(TEST_ID, state.Id, "Resolve 후에도 원본 Spec의 ID가 유지되어야 합니다.");
        }
    
        [Test]
        public void ColorResolution_ShouldNotExceedAvailablePool()
        {
            // 모든 색깔(4종)을 다 써버리는 케이스 테스트
            var elements = new List<ISpecElement> { 
                new BaseColorElement(CardColor.Red),
                SelectableColorElement.Random(),
                SelectableColorElement.Random(),
                SelectableColorElement.Random()
            };
            var spec = new CardSpec(TEST_ID, CardType.Attack, elements);
            var state = spec.NewState;
    
            state.ResolveValues();
    
            // 4종의 색깔이 중복 없이 모두 채워졌는지 확인
            var colors = state.ColorList.AllCandidateValues.Select(v => v.Value).ToList();
            Assert.AreEqual(4, colors.Distinct().Count());
            Assert.IsTrue(colors.Contains(CardColor.Black));
        }
    
        #endregion
    }
}