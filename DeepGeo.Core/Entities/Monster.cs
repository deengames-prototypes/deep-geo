using DeenGames.DeepGeo.Core.Maps;
using RogueSharp;
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

        private static CaveFloorMap map; // there's only one map at a time
        private static Random random = new Random();
        private Point goal;

        public Monster(ColourTuple colour, int speed, string visionType, int visionSize, CaveFloorMap map) : base('m', colour, true)
        {
            this.Speed = speed;
            this.VisionType = visionType;
            this.VisionSize = VisionSize;
            Monster.map = map;
        }

        public void PickRandomTarget()
        {
            this.goal = map.FindEmptyPosition();
        }

        // TODO: do this every frame. Why? Stuff moves. Player moves, barrels move,
        // doors lock and unlock, stuff happens. Can't just go with a static path.
        public void MoveTowardsGoal()
        {
            var goalMap = new GoalMap(map.GetIMap());
            // Aim for our random goal
            goalMap.AddGoal(this.goal.X, this.goal.Y, 100);
            // Avoid ANYTHING that's solid
            foreach (var e in map.Objects)
            {
                if (e.IsSolid && e != this)
                {
                    goalMap.AddObstacle(e.X, e.Y);
                }
            }

            try
            {
                var path = goalMap.FindPath(this.X, this.Y);
                var nextStep = path.Steps.Skip(1).First();
                this.Move(nextStep.X, nextStep.Y);
            }
            catch (Exception e)
            {
                // TODO: pick a different goal? meander around aimlessly?
            }
        }
    }
}
