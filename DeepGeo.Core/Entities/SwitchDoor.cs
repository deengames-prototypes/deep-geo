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
    public class SwitchDoor : Entity
    {
        private bool isOpen = false;

        public SwitchDoor(ColourTuple colour) : base('+', colour, true)
        {
        }

        public bool IsOpen
        {
            get { return this.isOpen; }
            set
            {
                this.isOpen = !this.isOpen;
                this.IsSolid = !this.isOpen;
            }
        }
    }
}
