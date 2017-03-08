using DeenGames.DeepGeo.Core.Maps;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.Entities
{
    public class Monster : Entity, IHasAgility
    {
        public string VisionType { get; private set; }
        public int VisionSize { get; private set; }
        public int Agility { get; private set; }

        private MonsterState currentState = MonsterState.Wandering;

        private static CaveFloorMap map; // there's only one map at a time
        private static Random random = new Random();

        private Point goal;
        private IGoalMap goalMap;

        public Monster(ColourTuple colour, int agility, string visionType, int visionSize, CaveFloorMap map) : base('m', colour, true)
        {
            this.Agility = agility;
            this.VisionType = visionType;
            this.VisionSize = visionSize;

            Monster.map = map;
            this.goal = map.FindEmptyPosition();
            this.goalMap = this.CreateGoalMap();
        }

        // We pick a goal, and move towards it, no matter what. Doing this every frame
        // is really expensive and massively slows down the game. We can't have that.
        // Instead, change your goal only when you're stuck -- path is blocked.
         // Returns true if we attacked the player
        public bool MoveWithAi(Player player)
        {
            switch (this.currentState)
            {
                case MonsterState.Wandering:
                    this.MoveTowardsGoal();
                    return false;
                case MonsterState.Hunting:
                    if (DistanceBetween(player.X, player.Y, this.X, this.Y) == 1)
                    {
                        player.Hurt();
                        return true;
                    }
                    else
                    {
                        // Find an empty space around the player that we can pathfind to. If one exists.
                        var target = map.GetIMap().GetCellsInRadius(player.X, player.Y, 1).FirstOrDefault(c =>
                        {
                            // Make sure it's walkable, and it's not the player's spot (kills pathfinding)
                            // Also make sure it's closer to the player than where we are now
                            if (c.IsWalkable && (c.X != player.X || c.Y != player.Y) && DistanceBetween(player.X, player.Y, c.X, c.Y) <= DistanceBetween(player.X, player.Y, this.X, this.Y))
                            {
                                var map = CreateGoalMap();
                                map.AddGoal(c.X, c.Y, 100);
                                return map.FindPaths(this.X, this.Y).Any();
                            }
                            else
                            {
                                return false;
                            }
                        });

                        if (target != null)
                        {
                            this.goal = new Point(target.X, target.Y);
                            this.goalMap = this.CreateGoalMap();
                        }
                        else
                        {
                            if (this.DistanceBetween(this.X, this.Y, player.X, player.Y) > this.VisionSize)
                            {
                                this.currentState = MonsterState.Wandering;
                                this.goal = new Point(this.X, this.Y); // reset next round
                            }
                        }
                        this.MoveTowardsGoal();
                    }
                    return false;
                default:
                    throw new InvalidOperationException($"Not sure how to deal with state {this.currentState}");
            }
        }

        public void HuntPlayer()
        {
            this.currentState = MonsterState.Hunting;
        }

        // Distance between (x1, y1) and (x2, y2)
        private int DistanceBetween(int x1, int y1, int x2, int y2)
        {
            // Approximate? Manhatten? distance.
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        private void MoveTowardsGoal()
        {
            if (this.X == this.goal.X && this.Y == this.goal.Y)
            {
                this.goal = map.FindEmptyPosition();
                this.goalMap = this.CreateGoalMap();
            }

            var path = goalMap.FindPath(this.X, this.Y);
            if (path != null)
            {
                var now = path.Steps.First();
                var nextStep = path.Steps.Skip(1).FirstOrDefault();
                if (nextStep != null && map.IsWalkable(nextStep.X, nextStep.Y))
                {
                    this.Move(nextStep.X, nextStep.Y);
                }
                else
                {
                    // Pick a new goal next time
                    this.goal = new Point(this.X, this.Y);
                }
            } else
            {
                // No path to goal... Pick a new goal next time
                this.goal = new Point(this.X, this.Y);
            }
        }

        private GoalMap CreateGoalMap()
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

            return goalMap;
        }

        
    }

    public enum MonsterState
    {
        Wandering,
        Hunting, // Actively targetting the player
        Scouting // Previously saw the player and now hanging around those areas looking for him
    }
}