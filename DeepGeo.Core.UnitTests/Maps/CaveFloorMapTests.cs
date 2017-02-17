using DeenGames.DeepGeo.Core.Maps;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Parameters;

namespace DeenGames.DeepGeo.Core.UnitTests.Maps
{
    [TestFixture]
    class CaveFloorMapTests
    {
        [Test]
        public void IsWalkableReturnsTrueForWalkableTiles()
        {
            var floorWidth = 100;
            var floorHeight = 50;

            var floor = new CaveFloorMap(floorWidth, floorHeight);
            var foundFloor = false;
            var foundWall = false;
            
            // There must be at least one walkable tile and one non-walkable tile, based on the current generation strategy
            for (var x = 0; x < floorWidth; x++)
            {
                for (var y = 0; y < floorHeight; y++)
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

        }
    }
}
