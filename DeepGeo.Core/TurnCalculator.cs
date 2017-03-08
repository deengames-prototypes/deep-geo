using DeenGames.DeepGeo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core
{
    /// <summary>
    /// Stateful class that remembers who had turns, and who's turn is next
    /// </summary>
    public class TurnCalculator
    {
        private readonly int MaxAgility;

        private Dictionary<IHasAgility, int> agilityPoints = new Dictionary<IHasAgility, int>();
        private IEnumerable<IHasAgility> entities = new List<IHasAgility>();

        public TurnCalculator(IEnumerable<IHasAgility> entities)
        {
            this.entities = entities;
            this.MaxAgility = this.entities.Max(e => e.Agility);
            foreach (var e in this.entities)
            {
                agilityPoints[e] = 0;
            }
        }

        public IHasAgility NextTurn()
        {
            // Add agility points, as much as you have agility
            // Whoever overflows gets a turn (eg. if you have 
            // someone with 5 and someone with 7 agility, 
            // and you boost so the person with 7 gets 14
            // points, they get two sequential turns).

            var needToAdd = entities.All(e => agilityPoints[e] < MaxAgility);

            if (needToAdd)
            {
                foreach (var e in entities)
                {
                    agilityPoints[e] += e.Agility;
                }
            }

            // Guaranteed to have at least one: the guy with max agility got exactly enough points.
            var haveTurns = agilityPoints.Where(kvp => kvp.Value >= MaxAgility).Select(kvp => kvp.Key)
                // Order from slowest to fastest; this seems more "fair" in terms of attack turn orders.
                .OrderBy(e => e.Agility);

            var toReturn = haveTurns.First();
            agilityPoints[toReturn] -= MaxAgility;
            return toReturn;
        }

        public override string ToString()
        {
            var toReturn = "";
            foreach (var kvp in agilityPoints)
            {
                toReturn += $"{kvp.Key} => {kvp.Value}, ";
            }
            return toReturn;
        }
    }
}
