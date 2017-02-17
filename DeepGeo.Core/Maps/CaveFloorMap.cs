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
        public Point PlayerStartPosition { get; private set; }

        private RogueSharp.Map tileData;

        private int width = 0;
        private int height = 0;
        private Stairs stairsDown;
        private IRandom random;

        public CaveFloorMap(int width, int height)
        {
            this.random = new RogueSharp.Random.DotNetRandom();
            // If you specify width=20, RogueSharp's map x-index goes from 0..20 inclusive. That's not what we want. Hence, -1
            this.width = width - 1;
            this.height = height - 1;

            var mapCreationStrategy = new RogueSharp.MapCreation.RandomRoomsMapCreationStrategy<RogueSharp.Map>(width, height, 100, 15, 4);
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

    }
}
