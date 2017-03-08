using DeenGames.DeepGeo.Core.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.UnitTests
{
    [TestFixture]
    class TurnCalculatorTests
    {
        // Player will get two turns in a row, rarely
        [Test]
        public void TurnCalculatorReturnsTurnsFairly()
        {
            var player = new Creature(7, "Player");
            var monster = new Creature(5, "Monster");

            // Manually worked this out
            var expected = new Creature[] { player, monster, player, monster, player, player, monster };

            var actual = new List<Creature>();
            var turnCalculator = new TurnCalculator(new Creature[] { player, monster });

            while (actual.Count < expected.Length)
            {
                actual.Add(turnCalculator.NextTurn() as Creature);
            }

            for (int i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];
                Assert.That(a, Is.EqualTo(e), $"On turn {i + 1}, expected {e.Name} but got {a.Name}");
            }
        }

        // Monster always gets two turns
        [Test]
        public void TurnCalculatorGivesSequentialTurns()
        {
            var player = new Creature(30, "Player");
            var monster = new Creature(60, "Monster");

            // Manually worked this out
            var expected = new Creature[] { monster, player, monster, monster, player, monster };

            var actual = new List<Creature>();
            var turnCalculator = new TurnCalculator(new Creature[] { player, monster });

            while (actual.Count < expected.Length)
            {
                actual.Add(turnCalculator.NextTurn() as Creature);
            }

            for (int i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];

                Assert.That(a, Is.EqualTo(e), $"On turn {i + 1}, expected {e.Name} but got {a.Name}");
            }
        }

        private class Creature : IHasAgility
        {
            private int agility = 0;
            public string Name { get; private set; }

            public Creature(int agility, string name)
            {
                this.agility = agility;
                this.Name = name;
            }

            public int Agility { get { return this.agility; } }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }    
}
