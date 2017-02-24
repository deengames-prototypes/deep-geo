using DeenGames.DeepGeo.Core.Entities;
using DeenGames.DeepGeo.Core.IO;
using RogueSharp;
using RogueSharp.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Maps
{
    public class CaveFloorMap
    {
        public Point PlayerStartPosition { get; private set; }

        private RogueSharp.Map tileData;

        private int width = 0;
        private int height = 0;
        private Stairs stairsDown;
        private IRandom random;

        private List<Entity> entities = new List<Entity>();

        public CaveFloorMap(int width, int height)
        {
            this.random = new RogueSharp.Random.DotNetRandom();
            // If you specify width=20, RogueSharp's map x-index goes from 0..20 inclusive. That's not what we want. Hence, -1
            this.width = width - 1;
            this.height = height - 1;

            var mapCreationStrategy = new RogueSharp.MapCreation.RandomRoomsMapCreationStrategy<RogueSharp.Map>(width, height, 100, 15, 4);
            //var mapCreationStrategy = new RogueSharp.MapCreation.CaveMapCreationStrategy<RogueSharp.Map>(width, height, 40, 2, 1);
            this.tileData = RogueSharp.Map.Create(mapCreationStrategy);

            this.stairsDown = new Entities.Stairs();
            this.stairsDown.Move(this.FindEmptyPosition());

            this.PlayerStartPosition = this.FindEmptyPosition();
            var distanceSquared = Math.Pow(Math.Min(this.width, this.height), 2);
            int tries = 0;

            // Approximate distance must be the width of the map or more. Try it 100 times, then quit.
            while (tries <= 100 && Math.Pow(this.PlayerStartPosition.X - this.stairsDown.X, 2) + Math.Pow(this.PlayerStartPosition.Y - this.stairsDown.Y, 2) <= distanceSquared)
            {
                this.PlayerStartPosition = this.FindEmptyPosition();
                distanceSquared = Math.Pow(Math.Min(this.width, this.height), 2);
                tries += 1;
            }

            if (random.Next(100) <= Config.Instance.Get<int>("PuzzlePushProbability"))
            {
                this.GeneratePushPuzzle();
            }

            this.GenerateLockedDoorsAndKeys();
        }

        public bool IsWalkable(int x, int y)
        {
            return x > 0 && y > 0 && x < this.width && y < this.height &&
                this.tileData.IsWalkable(x, y) && !this.entities.Any(e => e.X == x && e.Y == y && e.IsSolid == true);
        }

        public IMap GetIMap()
        {
            return this.tileData;
        }

        public void MarkAsDiscovered(int x, int y, bool isTransparent, bool isWalkable)
        {
            this.tileData.SetCellProperties(x, y, isTransparent, isWalkable, true);
        }

        public Point StairsDownPosition
        {
            get
            {
                return new Point(this.stairsDown.X, this.stairsDown.Y);
            }
        }

        public IReadOnlyCollection<Entity> Objects
        {
            get
            {
                return this.entities.AsReadOnly();
            }
        }

        public IEnumerable<Entity> GetObjectsAt(int x, int y)
        {
            return this.entities.Where(e => e.X == x && e.Y == y);
        }

        public bool IsBlockPuzzleComplete()
        {
            var receptacles = this.entities.Where(e => e is PushReceptacle).Select(e => e as PushReceptacle);
            var blocks = this.entities.Where(e => e is PushBlock).Select(e => e as PushBlock);
            int matched = 0;

            foreach (var r in receptacles)
            {
                if (blocks.SingleOrDefault(b => b.X == r.X && b.Y == r.Y && b.Match == r.Match) != null)
                {
                    matched += 1;
                }
            }

            return matched > 0 && matched == receptacles.Count();            
        }

        public IEnumerable<Key> DeleteBlocksAndSpawnKeys()
        {
            var toDelete = this.entities.Where(e => e is PushReceptacle || e is PushBlock).ToList();
            var spots = toDelete.OrderBy(t => random.Next(100)).Take(Config.Instance.Get<int>("KeysForBlockPuzzle"));

            foreach (var e in toDelete)
            {
                this.entities.Remove(e);
            }

            var toReturn = new List<Key>();

            foreach (var spot in spots)
            {
                var key = new Key();
                key.Move(spot.X, spot.Y);
                this.entities.Add(key);
                toReturn.Add(key);
            }

            return toReturn;
        }

        private void GeneratePushPuzzle()
        {
            // Generate a bunch of stuff you have to push into place in a pattern
            // This is reminiscent of the old Lufia 2 block-pushing puzzles. 

            // Simple: generate blocks of alternating colours in a line.
            // Player has to reassemble them in a grid.

            var red = new ColourTuple(255, 0, 0);
            var blue = new ColourTuple(0, 0, 255);

            for (var i = 0; i < Config.Instance.Get<int>("PushPuzzleBlocks"); i++)
            {
                // Block
                var colour = i % 2 == 0 ? red : blue;
                var block = new PushBlock(colour, colour == red ? "Red" : "Blue");
                block.Move(this.FindEmptyPosition());
                this.entities.Add(block);
            }

            bool foundEmptySpace = false;
            Point startingPoint = this.FindEmptyPosition();

            while (!foundEmptySpace)
            {
                // Empty 3x2 space
                if (this.IsWalkable(startingPoint.X, startingPoint.Y) && this.IsWalkable(startingPoint.X + 1, startingPoint.Y) && this.IsWalkable(startingPoint.X + 2, startingPoint.Y) &&
                    this.IsWalkable(startingPoint.X, startingPoint.Y + 1) && this.IsWalkable(startingPoint.X + 1, startingPoint.Y + 1) && this.IsWalkable(startingPoint.X + 2, startingPoint.Y + 1))
                {
                    foundEmptySpace = true;
                }
                else
                {
                    startingPoint = this.FindEmptyPosition();
                }
            }

            red = new ColourTuple(192, 64, 0);
            blue = new ColourTuple(64, 64, 192);

            for (var i = 0; i < Config.Instance.Get<int>("PushPuzzleBlocks"); i++)
            {
                var colour = i % 2 == 0 ? red : blue;
                var receptacle = new PushReceptacle(colour, colour == red ? "Red" : "Blue");
                var position = new Point(startingPoint.X + (i % 3), startingPoint.Y + (i / 3));
                receptacle.Move(position);
                this.entities.Add(receptacle);
            }
        }

        private Point FindEmptyPosition()
        {
            // Position the player somewhere on a walkable square
            int x = this.random.Next(1, this.width - 1);
            int y = this.random.Next(1, this.height - 1);

            while (!(tileData.IsWalkable(x, y)) || (stairsDown.X == x && stairsDown.Y == y))
            {
                x = this.random.Next(this.width);
                y = this.random.Next(this.height);
            }

            return new Point(x, y);
        }

        public void Remove(Entity e)
        {
            this.entities.Remove(e);
        }

        private void GenerateLockedDoorsAndKeys()
        {
            int iterations = 0;
            int numToGenerate = Config.Instance.Get<int>("NumberLockedDoors");
            int generated = 0;
            int radiusUsed = 4;
            var nearStairs = new List<Cell>();

            while (generated < numToGenerate)
            {
                iterations++;

                if (!nearStairs.Any())
                {
                    nearStairs = this.tileData.GetCellsInArea(this.stairsDown.X, this.stairsDown.Y, radiusUsed).OrderBy(r => random.Next(1000)).ToList();
                    radiusUsed *= 2;
                }

                var spot = nearStairs.First();
                nearStairs.Remove(spot);

                // Try to find spots near the stairs
                var left = this.IsWalkable(spot.X - 1, spot.Y);
                var right = this.IsWalkable(spot.X + 1, spot.Y);
                var up = this.IsWalkable(spot.X, spot.Y - 1);
                var down = this.IsWalkable(spot.X, spot.Y + 1);

                if ((!left && !right && up && down) || (left && right && !up && !down))
                {
                    var door = new LockedDoor();
                    door.Move(spot.X, spot.Y);
                    this.entities.Add(door);
                    generated++;
                }
            }

            // Generate less keys than doors. If there's a block puzzle,
            // and if you can complete it, that'll give you an extra few keys.
            generated = 0;
            var numKeys = Math.Ceiling(2 * numToGenerate / 3f);

            while (generated < numKeys)
            {
                var spot = this.FindEmptyPosition();
                var key = new Key();
                key.Move(spot);
                this.entities.Add(key);
                generated++;
            }
        }
    }
}
