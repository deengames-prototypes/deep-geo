using DeenGames.DeepGeo.Core.IO;
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
        public bool IsDead { get { return this.health <= 0; } }

        private int health = 0;

        public Player() : base('@', ColourTuple.Orange, true)
        {
            this.health = Config.Instance.Get<int>("StartingHealth");
        }

        public void Hurt()
        {
            this.health -= 1;
        }
    }
}
