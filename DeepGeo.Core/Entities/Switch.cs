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
    public class Switch : Entity
    {
        private readonly ColourTuple[] colours;

        public Switch(ColourTuple[] colours) : base('&', colours[0], true)
        {
            this.colours = colours;
        }

        public void Flip()
        {
            this.Colour = this.Colour == this.colours[0] ? this.colours[1] : this.colours[0];
        }
    }
}
