using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// <summary>
    /// A locked door. Any key goes with any door.
    /// TODO: convert to data
    /// </summary>
    public class LockedDoor : Entity
    {
        public LockedDoor() : base('+', new ColourTuple(255, 255, 0), true)
        {
        }
    }
}
