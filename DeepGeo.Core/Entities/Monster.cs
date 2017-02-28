﻿using DeenGames.DeepGeo.Core.Maps;
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
        private IGoalMap goalMap;

        public Monster(ColourTuple colour, int speed, string visionType, int visionSize, CaveFloorMap map) : base('m', colour, true)
        {
            this.Speed = speed;
            this.VisionType = visionType;
            this.VisionSize = VisionSize;
            Monster.map = map;
            this.goal = map.FindEmptyPosition();
            this.goalMap = this.CreateGoalMap();
        }

        // We pick a goal, and move towards it, no matter what. Doing this every frame
        // is really expensive and massively slows down the game. We can't have that.
        // Instead, change your goal only when you're stuck -- path is blocked.
        public void MoveTowardsGoal()
        {
            if (this.X == this.goal.X && this.Y == this.goal.Y)
            {
                this.goal = map.FindEmptyPosition();
                this.goalMap = this.CreateGoalMap();
            }

            try
            {
                var path = goalMap.FindPath(this.X, this.Y);
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
            }
            catch (PathNotFoundException p)
            {
                // Can't do anything useful with this exception.
                // Pick a new goal next time
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
}