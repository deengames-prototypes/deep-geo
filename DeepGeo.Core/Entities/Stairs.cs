﻿using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// Convert to data -- we don't need a class for this.
    public class Stairs : Entity
    {
        public Stairs() : base('>', new ColourTuple(255, 255, 255), false)
        {
        }
    }
}
