using Cardevil.NewCard.Config;
using Cardevil.NewCard.Core;
using Cardevil.Utils.Directions;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace Cardevil.Test.Card
{
    public class CardDeckTests
    {
        [Test]
        public void CreateDefaultSpecs_CreatesCorrectTotalCount()
        {
            var config = Resources.Load<DeckConfig>("ScriptableObjects/NewCard/DeckConfig");
            var specs = config.CreateDefaultSpecs();

            Assert.AreEqual(
                config.Debug_DeckCount,
                specs.Count
            );
        }
        
        [Test]
        public void AttackCards_HaveColorAndNumber()
        {
            var config = Resources.Load<DeckConfig>("ScriptableObjects/NewCard/DeckConfig");
            var specs = config.CreateDefaultSpecs();

            var attackSpecs = specs.Where(s => s.Type == CardType.Attack);

            foreach (var spec in attackSpecs)
            {
                var state = spec.State;

                Assert.NotNull(state.Colors);
                Assert.NotNull(state.Numbers);
                Assert.IsNull(state.Directions);
            }
        }
        
        [Test]
        public void MoveCards_DoNotContainNoneDirection()
        {
            var config = Resources.Load<DeckConfig>("ScriptableObjects/NewCard/DeckConfig");
            var specs = config.CreateDefaultSpecs();

            var moveSpecs = specs.Where(s => s.Type == CardType.Move);

            foreach (var spec in moveSpecs)
            {
                var directions = spec.State.Directions.AllOptions;
                CollectionAssert.DoesNotContain(directions, Direction.None);
            }
        }
        
        [Test]
        public void FourDirectionMoveCards_HaveFourAlternatives()
        {
            var config = Resources.Load<DeckConfig>("ScriptableObjects/NewCard/DeckConfig");
            var specs = config.CreateDefaultSpecs();

            var fourDirCards = specs
                .Where(s => s.Type == CardType.Move)
                .Where(s => s.State.Directions.HasAlternatives);

            foreach (var spec in fourDirCards)
            {
                var options = spec.State.Directions.AllOptions.ToList();

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        Direction.Up,
                        Direction.Down,
                        Direction.Left,
                        Direction.Right
                    },
                    options
                );
            }
        }
    }
}