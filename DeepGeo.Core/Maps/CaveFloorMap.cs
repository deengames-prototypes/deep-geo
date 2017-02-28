using DeenGames.DeepGeo.Core.Entities;
using DeenGames.DeepGeo.Core.IO;
using Newtonsoft.Json.Linq;
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
        public Point playerStartPosition;

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

            this.playerStartPosition = this.FindEmptyPosition();
            var distanceSquared = Math.Pow(Math.Min(this.width, this.height), 2);
            int tries = 0;

            // Approximate distance must be the width of the map or more. Try it 100 times, then quit.
            while (tries <= 100 && Math.Pow(this.playerStartPosition.X - this.stairsDown.X, 2) + Math.Pow(this.playerStartPosition.Y - this.stairsDown.Y, 2) <= distanceSquared)
            {
                this.playerStartPosition = this.FindEmptyPosition();
                distanceSquared = Math.Pow(Math.Min(this.width, this.height), 2);
                tries += 1;
            }

            if (random.Next(100) <= Config.Instance.Get<int>("PushPuzzleProbability"))
            {
                this.GeneratePushPuzzle();
            }

            this.GenerateLockedDoorsAndKeys();

            if (random.Next(100) <= Config.Instance.Get<int>("SwitchPuzzleProbability"))
            {
                this.GenerateSwitchPuzzle();
            }

            this.GenerateMonsters();            
        }

        // Adds the player to objects, returns their coordinates
        public Point AddPlayer(Player player)
        {
            this.entities.Add(player);
            player.Move(this.playerStartPosition);
            return this.playerStartPosition;
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

        public void Remove(Entity e)
        {
            this.entities.Remove(e);
        }

        public void FlipSwitches()
        {
            var doors = this.entities.Where(s => s is SwitchDoor).Select(s => s as SwitchDoor);
            foreach (var d in doors)
            {
                d.IsOpen = !d.IsOpen;
            }
        }

        internal Point FindEmptyPosition()
        {
            int x = this.random.Next(1, this.width - 1);
            int y = this.random.Next(1, this.height - 1);

            while (!(tileData.IsWalkable(x, y)) || this.entities.Any(e => e.X == x && e.Y == y))
            {
                x = this.random.Next(this.width);
                y = this.random.Next(this.height);
            }

            return new Point(x, y);
        }

        private bool IsWall(int x, int y)
        {
            return !IsWalkable(x, y) && !this.entities.Any(e => e.X == x && e.Y == y && e.IsSolid == true);
        }

        private void GeneratePushPuzzle()
        {
            // Generate a bunch of stuff you have to push into place in a pattern
            // This is reminiscent of the old Lufia 2 block-pushing puzzles. 

            // Simple: generate blocks of alternating colours in a line.
            // Player has to reassemble them in a grid.

            var red = ColourTuple.Red;
            var blue = ColourTuple.Blue;

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

        private void GenerateSwitchPuzzle()
        {
            // Generate a puzzle with N switches, each of which toggles one set of alternating doors.
            // Eg. half the doors are blue, and half are purple; switches toggle between the two,
            // eg. blue open purple closed, purple open blue closed.

            int doorsToGenerate = Config.Instance.Get<int>("SwitchDoors");
            int generated = 0;

            var doorColours = new ColourTuple[] { ColourTuple.Cyan, ColourTuple.Purple };

            while (generated < doorsToGenerate)
            {
                var spot = this.FindEmptyPosition();
                if (this.IsInHallway(spot.X, spot.Y))
                {
                    var door = new SwitchDoor(generated % 2 == 0 ? doorColours[0] : doorColours[1]);
                    if (door.Colour == doorColours[1])
                    {
                        door.IsOpen = true;
                    }
                    door.Move(spot.X, spot.Y);
                    this.entities.Add(door);
                    generated++;
                }
            }

            for (var i = 0; i < Config.Instance.Get<int>("Switches"); i++)
            {
                var spot = this.FindEmptyPosition();
                var s = new Switch(doorColours);
                s.Move(spot.X, spot.Y);
                this.entities.Add(s);
            }
        }

        private void GenerateMonsters()
        {
            var num = Config.Instance.Get<int>("NumMonsters");

            JArray templates = Config.Instance.Get<dynamic>("Monsters");
            var monsterColours = new ColourTuple[] { ColourTuple.Orange, ColourTuple.Green };

            for (var i = 0; i < num; i++)
            {
                var t = templates[i % templates.Count];
                var colour = monsterColours[i % templates.Count];
                var speed = (int)t["Speed"];
                var vision = t["Vision"].ToString();
                var visionSize = (int)t["VisionSize"];
                var spot = this.FindEmptyPosition();

                var m = new Monster(colour, speed, vision, visionSize, this);
                m.Move(spot);
                this.entities.Add(m);
            }
        }

        private void GenerateLockedDoorsAndKeys()
        {
            // Generate a bunch of locked doors, and about half as many keys.
            // Locked doors tend to appear near the stairs down.

            int numToGenerate = Config.Instance.Get<int>("NumberLockedDoors");
            int generated = 0;
            int radiusUsed = 4;
            var nearStairs = new List<ICell>();

            while (generated < numToGenerate)
            {
                if (!nearStairs.Any())
                {
                    nearStairs = this.tileData.GetCellsInArea(this.stairsDown.X, this.stairsDown.Y, radiusUsed).OrderBy(r => random.Next(100)).ToList();
                    radiusUsed *= 2;
                }

                var spot = nearStairs.First();
                nearStairs.Remove(spot);

                if (this.IsInHallway(spot.X, spot.Y))
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

        private bool IsInHallway(int x, int y)
        {
            // Try to find spots near the stairs
            var left = this.IsWall(x - 1, y);
            var right = this.IsWall(x + 1, y);
            var up = this.IsWall(x, y - 1);
            var down = this.IsWall(x, y + 1);

            return ((!left && !right && up && down) || (left && right && !up && !down));
        }
    }
}
