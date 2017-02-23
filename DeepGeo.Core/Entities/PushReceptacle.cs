using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    /// <summary>
    /// Something you push PushBlocks onto for push puzzles.
    /// TODO: convert to data
    /// </summary>
    public class PushReceptacle : Entity
    {
        public PushReceptacle(ColourTuple colour) : base('*', colour, false)
        {
        }
    }
}
