using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// <summary>
    /// A key for a locked door. Any key goes with any door.
    /// TODO: convert to data
    /// </summary>
    public class Key : Entity
    {
        public Key() : base('|', new ColourTuple(255, 255, 0), false)
        {
        }
    }
}
