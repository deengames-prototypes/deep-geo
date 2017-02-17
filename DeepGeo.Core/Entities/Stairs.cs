using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    public class Stairs
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        // TODO: make this a common method
        public void Move(Point position)
        {
            this.X = position.X;
            this.Y = position.Y;
        }
    }
}
