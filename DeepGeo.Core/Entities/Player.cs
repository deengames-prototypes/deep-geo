using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    // The only entity with real logic, perhaps.
    public class Player : Entity
    {
        public int Keys { get; set; } = 0;

        public Player() : base('@', new ColourTuple(255, 128, 0), true)
        {
        }
    }
}
