using DeenGames.DeepGeo.Core.Maps;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Parameters;
using System.IO;
using DeenGames.DeepGeo.Core.IO;

namespace DeenGames.DeepGeo.Core.UnitTests.Maps
{
    [TestFixture]
    class CaveFloorMapTests
    {
        // Tests with these don't get a lot of collisions/churn (eg. place stairs far away from the player)
        private const int MapWidth = 100;
        private const int MapHeight = 50;

        [Test]
        public void IsWalkableReturnsTrueForWalkableTiles()
        {
            File.WriteAllText("data/config.json", "{ 'PuzzlePushProbability': 0 }");
            new Config("data/config.json");

            var floor = new CaveFloorMap(MapWidth, MapHeight);
            var foundFloor = false;
            var foundWall = false;
            
            // There must be at least one walkable tile and one non-walkable tile, based on the current generation strategy
            for (var x = 0; x < MapWidth; x++)
            {
                for (var y = 0; y < MapHeight; y++)
                {
                    var walkable = floor.IsWalkable(x, y);
                    if (walkable)
                    {
                        foundFloor = true;
                    }
                    else
                    {
                        foundWall = true;
                    }
                }
            }

            Assert.That(foundFloor, Is.EqualTo(true));
            Assert.That(foundWall, Is.EqualTo(true));
        }

        [Test]
        public void ConstructorCreatesStairsDownAwayFromPlayer()
        {
            // Stairs are randomly positioned. To avoid a flaky test, just check if the distance is non-zero.
            var floor = new CaveFloorMap(MapWidth, MapHeight);
            var playerPosition = floor.playerStartPosition;
            var stairsDown = floor.StairsDownPosition;

            var distance = Math.Abs(playerPosition.X - stairsDown.X) + Math.Abs(playerPosition.Y - stairsDown.Y);
            Assert.That(distance, Is.GreaterThan(0));
        }
    }
}
