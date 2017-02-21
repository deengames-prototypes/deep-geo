using DeenGames.DeepGeo.Core.Entities;
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
        // TODO: externalize in a global config JSON file, as per prototyping process
        private const float PushPuzzleProbability = 100; // 30
        private const int PushPuzzleBlocks = 6;

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

            //var mapCreationStrategy = new RogueSharp.MapCreation.RandomRoomsMapCreationStrategy<RogueSharp.Map>(width, height, 100, 15, 4);
            var mapCreationStrategy = new RogueSharp.MapCreation.BorderOnlyMapCreationStrategy<RogueSharp.Map>(width, height);
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

            if (random.Next(100) <= PushPuzzleProbability)
            {
                this.GeneratePushPuzzle();
            }
        }

        public bool IsWalkable(int x, int y)
        {
            // TODO: make sure there are no entities there, eg. player, monster
            return this.tileData.IsWalkable(x, y);
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

        private void GeneratePushPuzzle()
        {
            // Generate a bunch of stuff you have to push into place in a pattern
            // This is reminiscent of the old Lufia 2 puzzles. 

            // TODO: challenging to find a space to fit this into. Make one?
            // TODO: does this unlock a key? Open a door to the stairs down?
            // TODO: if you mess up, can you restart? Or can you pull blocks?

            // Simple: generate five blocks of alternating colours in a line.
            // Player has to reassemble them in a grid.

            var start = this.FindEmptyPosition();
            var red = new ColourTuple(192, 64, 64);
            var blue = new ColourTuple(64, 64, 192);

            // TODO: don't assume we're in a six-by-three open area
            for (var i = 0; i < 6; i++)
            {
                // Block
                var colour = i % 2 == 0 ? red : blue;
                var opposite = colour == red ? blue : red;
                var block = new PushBlock(colour);
                block.Move(new Point(start.X + i, start.Y));
                this.entities.Add(block);

                // Matching receptacle
                var receptacle = new PushReceptacle(opposite);
                receptacle.Move(new Point(start.X + (i % 3), start.Y + 1 + (i / 3))); // rows of three below blocks
                Console.WriteLine(receptacle.X + ", " + receptacle.Y);
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
    }
}
