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
        public char DisplayCharacter { get; private set; }
        public ColourTuple Colour { get; private set; }

        public Entity(char displayCharacter, ColourTuple colour)
        {
            this.DisplayCharacter = displayCharacter;
            this.Colour = colour;
        }

        public Entity Move(Point position)
        {
            this.X = position.X;
            this.Y = position.Y;
            return this;
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
