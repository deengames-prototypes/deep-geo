using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    public class Monster : Entity
    {
        public int Speed { get; private set; }
        public string VisionType { get; private set; }
        public int VisionSize { get; private set; }

        public Monster(ColourTuple colour, int speed, string visionType, int visionSize) : base('m', colour, true)
        {
            this.Speed = speed;
            this.VisionType = visionType;
            this.VisionSize = VisionSize;
        }
    }
}
