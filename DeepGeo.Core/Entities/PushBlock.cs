﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// <summary>
    /// A block that you push for pushing puzzles. Goes into a PushReceptacle.
    /// </summary>
    public class PushBlock : Entity
    {
        public ColourTuple Colour { get; private set; }

        public PushBlock(ColourTuple colour) : base('0', colour)
        {
            this.Colour = colour;
        }
    }
}
