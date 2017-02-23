using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// <summary>
    /// A block that you push for pushing puzzles. Goes into a PushReceptacle.
    /// TODO: convert to data
    /// </summary>
    public class PushBlock : Entity, Pushable, Pullable
    {
        public readonly string Match;

        public PushBlock(ColourTuple colour, string matchOn) : base('0', colour, true)
        {
            this.Match = matchOn;
        }
    }
}
