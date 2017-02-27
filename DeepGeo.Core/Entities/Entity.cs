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
        public static readonly ColourTuple Red = new ColourTuple(255, 0, 0);
        public static readonly ColourTuple Blue = new ColourTuple(0, 0, 255);
        public static readonly ColourTuple Orange = new ColourTuple(255, 128, 0);
        public static readonly ColourTuple Green = new ColourTuple(0, 255, 0);
        public static readonly ColourTuple Purple = new ColourTuple(128, 0, 255);
        public static readonly ColourTuple Cyan = new ColourTuple(0, 128, 255);

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public ColourTuple(int r, int g, int b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }
    }
}
