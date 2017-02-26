using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    public abstract class Entity
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsSolid { get; protected set; }
        public char DisplayCharacter { get; private set; }
        public ColourTuple Colour { get; internal set; }

        public Entity(char displayCharacter, ColourTuple colour, bool isSolid)
        {
            this.DisplayCharacter = displayCharacter;
            this.Colour = colour;
            this.IsSolid = isSolid;
        }

        public Entity Move(int x, int y)
        {
            this.X = x;
            this.Y = y;
            return this;
        }

        public Entity Move(Point position)
        {
            return this.Move(position.X, position.Y);
        }
    }

    public class ColourTuple
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public ColourTuple(int r, int g, int b)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
        }
    }
}
