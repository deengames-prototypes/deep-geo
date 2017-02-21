﻿using System;
using Microsoft.Xna.Framework;
using SadConsole.Input;
using SadConsole.Effects;
using SadConsole.Game;
using SadConsole;
using DeenGames.DeepGeo.ConsoleUi.SadConsoleHelpers.Extensions;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Ninject.Parameters;
using Ninject;
using DeenGames.DeepGeo.ConsoleUi.ViewExtensions;
using DeenGames.DeepGeo.Core.Maps;
using System.Linq;
using DeenGames.DeepGeo.Core.Entities;
using DeenGames.DeepGeo.Core.IO;

namespace DeenGames.DeepGeo.ConsoleUi.Windows
{
    class AreaViewWindow : SadConsole.Consoles.Console
    {
        private IList<GameObject> objects = new List<GameObject>();
        private CaveFloorMap currentMap;

        private ICellEffect DiscoveredEffect = new Recolor() { Foreground = Color.LightGray * 0.5f, Background = Color.Black, DoForeground = true, DoBackground = true, CloneOnApply = false };
        private ICellEffect HiddenEffect = new Recolor() { Foreground = Color.Black, Background = Color.Black, DoForeground = true, DoBackground = true, CloneOnApply = false };
        
        public AreaViewWindow(int width, int height) : base(Config.Instance.Get<int>("MapWidth"), Config.Instance.Get<int>("MapHeight"))
        {
            this.TextSurface.RenderArea = new Rectangle(0, 0, width, height);
            var playerEntity = this.CreateGameObject("Player", '@', Color.Orange, new Point(1, 1));
            this.CreateGameObject("StairsDown", '>', Color.White);

            SadConsole.Engine.ActiveConsole = this;
            // Keyboard setup
            SadConsole.Engine.Keyboard.RepeatDelay = 0.07f;
            SadConsole.Engine.Keyboard.InitialRepeatDelay = 0.1f;

            this.GenerateAndDisplayMap();

            var currentFieldOfView = new RogueSharp.FieldOfView(this.currentMap.GetIMap());
            var fovTiles = currentFieldOfView.ComputeFov(playerEntity.Position.X, playerEntity.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);
            this.MarkCurrentFovAsVisible(fovTiles);
        }

        public override void Render()
        {
            base.Render();
            foreach (var obj in this.objects)
            {
                if (obj.IsVisible)
                {
                    obj.Render();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (var obj in this.objects)
            {
                obj.Update();
            }
            this.ProcessGamepad();
        }

        private void ProcessGamepad()
        {
            // Check the device for Player One
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            
            // If there a controller attached, handle it
            if (capabilities.IsConnected)
            {
                // Get the current state of Controller1
                GamePadState state = GamePad.GetState(PlayerIndex.One);

                // You can check explicitly if a gamepad has support 
                // for a certain feature
                if (capabilities.HasLeftXThumbStick)
                {
                    // Check teh direction in X axis of left analog stick
                    if (state.ThumbSticks.Left.X < -0.5f) {
                        this.MovePlayerBy(new Point(-1, 0));
                    }
                    else if (state.ThumbSticks.Left.X > 0.5f) {
                        this.MovePlayerBy(new Point(1, 0));
                    } 
                    else if (state.ThumbSticks.Left.Y < -0.5f) {
                        this.MovePlayerBy(new Point(0, 1));
                    }
                    else if (state.ThumbSticks.Left.Y > 0.5f) {
                        this.MovePlayerBy(new Point(0, -1));
                    }
                }
            }
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Down)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.S)))
            {
                this.MovePlayerBy(new Point(0, 1));
            }
            else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Up)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.W)))
            {
                this.MovePlayerBy(new Point(0, -1));
            }

            if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Right)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.D)))
            {
                this.MovePlayerBy(new Point(1, 0));
            }
            else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Left)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.A)))
            {
                this.MovePlayerBy(new Point(-1, 0));
            }

            return false;
        }

        private void MovePlayerBy(Point amount)
        {
            var currentFieldOfView = new RogueSharp.FieldOfView(this.currentMap.GetIMap());
            var playerEntity = this.objects.Single(g => g.Name == "Player");
            var fovTiles = currentFieldOfView.ComputeFov(playerEntity.Position.X, playerEntity.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);

            this.MarkCurrentFovAsDiscovered(fovTiles);

            // Get the position the player will be at
            Microsoft.Xna.Framework.Point newPosition = playerEntity.Position + amount;

            // Check to see if the position is within the map
            if (new Rectangle(0, 0, Width, Height).Contains(newPosition) && currentMap.IsWalkable(newPosition.X, newPosition.Y))
            {
                // Move the player
                playerEntity.Position += amount;
                CenterViewToPlayer();
            }

            fovTiles = currentFieldOfView.ComputeFov(playerEntity.Position.X, playerEntity.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);
            this.MarkCurrentFovAsVisible(fovTiles);
        }

        private void MarkCurrentFovAsVisible(IReadOnlyCollection<RogueSharp.Cell> fovTiles)
        {
            foreach (var cell in fovTiles)
            {
                var tile = this[cell.X, cell.Y];
                tile.ClearEffect();

                foreach (var obj in this.objects)
                {
                    if (obj.Position.X == cell.X && obj.Position.Y == cell.Y)
                    {
                        obj.IsVisible = true;
                    }
                }
            }
        }

        // Marks tiles as "discovered" (50% visible). This is the same as "unmarking" visible tiles as visible.
        private void MarkCurrentFovAsDiscovered(IReadOnlyCollection<RogueSharp.Cell> fovTiles)
        {
            foreach (var cell in fovTiles)
            {
                // Tell the map data (for FOV calculations) that we've discovered this tile
                this.currentMap.MarkAsDiscovered(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable);

                // Update view rendering to the appropriate effect
                var tile = this[cell.X, cell.Y];
                tile.ApplyEffect(DiscoveredEffect);

                foreach (var obj in this.objects)
                {
                    if (obj.Position.X == cell.X && obj.Position.Y == cell.Y)
                    {
                        obj.IsVisible = false;
                    }
                }
            }
        }

        private void CenterViewToPlayer()
        {
            var playerEntity = this.objects.Single(g => g.Name == "Player");

            // Scroll the view area to center the player on the screen
            TextSurface.RenderArea = new Rectangle(playerEntity.Position.X - (TextSurface.RenderArea.Width / 2),
                                                    playerEntity.Position.Y - (TextSurface.RenderArea.Height / 2),
                                                    TextSurface.RenderArea.Width, TextSurface.RenderArea.Height);

            // If he view area moved, we'll keep our entities in sync with it.
            foreach (var obj in this.objects)
            {
                obj.RenderOffset = this.Position - TextSurface.RenderArea.Location;
            }
        }

        private void GenerateAndDisplayMap()
        {
            var playerEntity = this.objects.Single(g => g.Name == "Player");
            var mapWidth = Config.Instance.Get<int>("MapWidth");
            var mapHeight = Config.Instance.Get<int>("MapHeight");

            // Create the map
            this.currentMap = new CaveFloorMap(mapWidth, mapHeight);

            // Create the view
            var start = currentMap.PlayerStartPosition;
            playerEntity.Position = new Point(start.X, start.Y);
            this.CenterViewToPlayer();

            var stairsDown = this.objects.Single(g => g.Name == "StairsDown");
            stairsDown.Move(currentMap.StairsDownPosition.X, currentMap.StairsDownPosition.Y);

            foreach (var obj in currentMap.Objects)
            {
                this.CreateGameObject(string.Empty, obj.DisplayCharacter, new Color(obj.Colour.Red, obj.Colour.Green, obj.Colour.Blue), new Point(obj.X, obj.Y));
            }

            // Loop through the map information generated by RogueSharp and update our view
            for (var j = 0; j < mapHeight; j++)
            {
                for (var i = 0; i < mapWidth; i++)
                {
                    var tile = this[i, j];
                    this.CreateCellFor(i, j).CopyAppearanceTo(tile);
                    tile.ApplyEffect(HiddenEffect);
                }
            }
        }
        
        private ICellAppearance CreateCellFor(int x, int y)
        {
            // Objects that should appear, in darkness, should be here
            var stairs = this.objects.Where(o => o.Name.Contains("Stairs"));
            foreach (var s in stairs)
            {
                if (s.Position.X == x && s.Position.Y == y)
                {
                    // TODO: stairs up or down
                    return new CellAppearance(Color.DarkGray, Color.Transparent, '>');
                }
            }
            if (this.currentMap.IsWalkable(x, y))
            {
                return new CellAppearance(Color.DarkGray, Color.Transparent, '.');
            }
            else
            {
                return new CellAppearance(Color.LightGray, Color.Transparent, '#');

            }
        }

        private GameObject CreateGameObject(string name, char display, Color colour, Point? position = null)
        {
            var toReturn = new GameObject(Engine.DefaultFont);
            toReturn.Name = name;
            toReturn.DrawAs(display, colour);
            toReturn.IsVisible = false;
            if (position != null)
            {
                toReturn.Position = position.Value;
            }

            this.objects.Add(toReturn);
            return toReturn;
        }
    }
}
