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
using DeenGames.DeepGeo.ConsoleUi.SadConsoleHelpers;
using DeenGames.DeepGeo.Core;

namespace DeenGames.DeepGeo.ConsoleUi.Windows
{
    class AreaViewWindow : SadConsole.Consoles.Console
    {
        private IList<GameObjectWithData> objects = new List<GameObjectWithData>();
        private CaveFloorMap currentMap;
        private bool gameOver = false;
        private TurnCalculator turnCalculator;

        private ICellEffect DiscoveredEffect = new Recolor() { Foreground = Color.LightGray * 0.5f, Background = Color.Black, DoForeground = true, DoBackground = true, CloneOnApply = false };
        private ICellEffect HiddenEffect = new Recolor() { Foreground = Color.Black, Background = Color.Black, DoForeground = true, DoBackground = true, CloneOnApply = false };
        private ICellEffect MonsterVisionEffect = new Recolor() { DoForeground = false, DoBackground = true, Background = Color.White * 0.5f, CloneOnApply = false };

        // This is hideous, but necessary. We mark tiles as discovered when discovered; when a monster moves over them,
        // and then out of sight, how do we know if the tile was discovered or never visible at all? We have to remember.
        // So, remember. Map of $"{x}, {y}" => cell
        public Dictionary<string, Cell> discoveredTiles = new Dictionary<string, Cell>();

        private Action<string> showMessageCallback;

        public AreaViewWindow(int width, int height, Action<string> showMessageCallback) : base(Config.Instance.Get<int>("MapWidth"), Config.Instance.Get<int>("MapHeight"))
        {
            this.showMessageCallback = showMessageCallback;
            this.TextSurface.RenderArea = new Rectangle(0, 0, width, height);

            var player = new Player();
            var playerEntity = this.CreateGameObject("Player", player);
            this.CreateGameObject("StairsDown", '>', Color.White);

            SadConsole.Engine.ActiveConsole = this;
            // Keyboard setup
            SadConsole.Engine.Keyboard.RepeatDelay = 0.07f;
            SadConsole.Engine.Keyboard.InitialRepeatDelay = 0.1f;

            this.GenerateAndDisplayMap();

            var currentFieldOfView = new RogueSharp.FieldOfView(this.currentMap.GetIMap());
            var fovTiles = currentFieldOfView.ComputeFov(playerEntity.Position.X, playerEntity.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);
            this.MarkCurrentFovAsVisible(fovTiles);

            var creatures = this.objects.Where(s => s.Data is Monster || s.Data is Player).Select(e => e.Data as IHasAgility);
            this.turnCalculator = new TurnCalculator(creatures);
        }

        public override void Render()
        {
            base.Render();
            foreach (var obj in this.objects)
            {
                if (obj.IsVisible)
                {
                    // Special case for switch doors
                    if (obj.Data is SwitchDoor)
                    {
                        var cell = obj.RenderCells[0].ActualForeground;

                        if ((obj.Data as SwitchDoor).IsOpen)
                        {
                            obj.RenderCells[0].ActualForeground = new Color(cell.R, cell.G, cell.B, 0.5f); // transparent
                        }
                        else
                        {
                            obj.RenderCells[0].ActualForeground = new Color(cell.R, cell.G, cell.B, 1f);   // 100% opaque
                        }
                    }

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
                    // Check the direction in X axis of left analog stick
                    if (state.ThumbSticks.Left.X < -0.5f)
                    {
                        this.MovePlayerBy(new Point(-1, 0));
                    }
                    else if (state.ThumbSticks.Left.X > 0.5f)
                    {
                        this.MovePlayerBy(new Point(1, 0));
                    }
                    else if (state.ThumbSticks.Left.Y < -0.5f)
                    {
                        this.MovePlayerBy(new Point(0, 1));
                    }
                    else if (state.ThumbSticks.Left.Y > 0.5f)
                    {
                        this.MovePlayerBy(new Point(0, -1));
                    }
                }
            }
        }

        public override bool ProcessKeyboard(KeyboardInfo info)
        {
            if (!this.gameOver)
            {
                if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Down)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.S)))
                {
                    this.MovePlayerBy(new Point(0, 1), info);
                }
                else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Up)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.W)))
                {
                    this.MovePlayerBy(new Point(0, -1), info);
                }

                if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Right)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.D)))
                {
                    this.MovePlayerBy(new Point(1, 0), info);
                }
                else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Left)) || info.KeysPressed.Contains(AsciiKey.Get(Keys.A)))
                {
                    this.MovePlayerBy(new Point(-1, 0), info);
                }
                else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Space)))
                {
                    this.MovePlayerBy(new Point(0, 0), info);
                }

                if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Space)))
                {
                    var switches = this.objects.Where(o => o.Data is Switch);
                    var player = this.objects.Single(o => o.Data is Player);
                    foreach (var s in switches)
                    {
                        if ((Math.Abs(s.Position.X - player.Position.X) + Math.Abs(s.Position.Y - player.Position.Y)) <= 1)
                        {
                            (s.Data as Switch).Flip();
                            this.currentMap.FlipSwitches();

                            foreach (var w in switches)
                            {
                                w.RenderCells[0].ActualForeground = new Color(s.Data.Colour.R, s.Data.Colour.G, s.Data.Colour.B);
                            }

                            showMessageCallback("You flip the switch.");
                        }
                    }
                }
            } else
            {
                if (info.KeysPressed.Any())
                {
                    Game.Stop();
                }
            }
            return false;
        }

        private void MovePlayerBy(Point amount, KeyboardInfo info = null)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var currentFieldOfView = new RogueSharp.FieldOfView(this.currentMap.GetIMap());
            var playerView = this.objects.Single(g => g.Name == "Player");

            var fovTiles = currentFieldOfView.ComputeFov(playerView.Position.X, playerView.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);
            this.MarkCurrentFovAsDiscovered(fovTiles);
            foreach (var obj in this.objects)
            {
                obj.IsVisible = false;
            }

            // Undo monster vision tile effect
            foreach (var monsterView in this.objects.Where(o => o.Data is Monster))
            {
                var data = monsterView.Data as Monster;
                var monsterFov = currentFieldOfView.ComputeFov(data.X, data.Y, data.VisionSize, true);
                // If we can see them, they're discovered
                foreach (var cell in monsterFov)
                {
                    if (this.discoveredTiles.ContainsKey($"{cell.X}, {cell.Y}"))
                    {
                        this[cell.X, cell.Y].ApplyEffect(DiscoveredEffect);
                    } else
                    {
                        this[cell.X, cell.Y].ApplyEffect(HiddenEffect);
                    }
                }
            }

            // Get the position the player will be at
            Point newPosition = playerView.Position + amount;
            
            // Is there a block there?
            if (currentMap.GetObjectsAt(newPosition.X, newPosition.Y).Any(e => e is Pushable))
            {
                // Is there an empty space behind it?
                var behindBlock = newPosition + amount;
                if (currentMap.IsWalkable(behindBlock.X, behindBlock.Y))
                {
                    // Push it (update data)
                    var obj = currentMap.GetObjectsAt(newPosition.X, newPosition.Y).Single(e => e is Pushable);
                    obj.Move(behindBlock.X, behindBlock.Y);
                    // Push it (move view object)
                    this.objects.Single(g => g.Data == obj).Move(behindBlock.X, behindBlock.Y);
                    this.CheckIfBlockPuzzleIsComplete();
                }
            }

            // Check to see if the position is within the map and walkable
            if (new Rectangle(0, 0, Width, Height).Contains(newPosition) && currentMap.IsWalkable(newPosition.X, newPosition.Y))
            {
                // Pull a block along if specified
                if (info.KeysDown.Contains(AsciiKey.Get(Keys.Space)))
                {
                    // Are you holding space? Did you move in a direction (eg. left) and there's a push block on the other side (eg. right)?
                    // If so, and if the target block position is walkable, pull it along. But this only works if you're pulling
                    // in the direction you're moving (eg. standing on top and pulling a block upward), NOT standing beside a block
                    // and pulling it upward or standing above a block and pulling to the left/right
                    var currentBlockPos = playerView.Position + new Point(amount.X * -1, amount.Y * -1);
                    if (currentMap.GetObjectsAt(currentBlockPos.X, currentBlockPos.Y).Any(e => e is Pullable))
                    {
                        // Given constraints above, target position is current player position
                        var obj = currentMap.GetObjectsAt(currentBlockPos.X, currentBlockPos.Y).Single(e => e is Pullable);
                        obj.Move(playerView.Position.X, playerView.Position.Y);
                        this.objects.Single(g => g.Data == obj).Move(playerView.Position.X, playerView.Position.Y);
                        this.CheckIfBlockPuzzleIsComplete();
                    }
                }

                // Move the player
                playerView.Position += amount;
                playerView.Data.Move(playerView.Position.X, playerView.Position.Y);
                CenterViewToPlayer();
            }

            var key = this.currentMap.GetObjectsAt(playerView.Position.X, playerView.Position.Y).Where(s => s is Key).SingleOrDefault();
            if (key != null)
            {
                // Not sure why two keys are spawned here
                var keys = this.objects.Where(s => s.Data == key).ToList();
                foreach (var k in keys)
                {
                    this.objects.Remove(k);
                    this.currentMap.Remove(key);
                }
                
                (this.objects.Single(o => o.Data is Player).Data as Player).Keys += 1;
                this.showMessageCallback("Got a key.");
            }

            // Door there, and we have a key? Unlock it!
            if ((playerView.Data as Player).Keys > 0 && this.currentMap.GetObjectsAt(newPosition.X, newPosition.Y).Any(o => o is LockedDoor))
            {
                var doorData = this.currentMap.GetObjectsAt(newPosition.X, newPosition.Y).Where(o => o is LockedDoor).ToList();
                var doors = this.objects.Where(o => doorData.Contains(o.Data)).ToList();

                // Why, why WHY are there multiple copies?
                foreach (var door in doors)
                {
                    this.objects.Remove(door);
                    this.currentMap.Remove(door.Data);
                }
                showMessageCallback("You unlock the door.");
                (playerView.Data as Player).Keys -= 1;
            }

            fovTiles = currentFieldOfView.ComputeFov(playerView.Position.X, playerView.Position.Y, Config.Instance.Get<int>("PlayerLightRadius"), true);
            this.MarkCurrentFovAsVisible(fovTiles);

            // Monsters turn. Also, draw their field-of-view. Keep queuing turns until it's the player's turn again.
            var monstersToMove = new List<IHasAgility>();
            IHasAgility next = null;
            while (next != playerView.Data)
            {
                if (next != null)
                {
                    monstersToMove.Add(next);
                }
                next = turnCalculator.NextTurn();
            }
            
            while (monstersToMove.Any())
            {
                var m = monstersToMove[0];
                monstersToMove.RemoveAt(0); 
                var monsterView = this.objects.Single(o => o.Data is Monster && o.Data == m);
                var data = monsterView.Data as Monster;
                var hurtPlayer = data.MoveWithAi(playerView.Data as Player);
                if (hurtPlayer)
                {
                    if ((playerView.Data as Player).IsDead)
                    {
                        showMessageCallback("The monster hits you! You die ...");
                        this.gameOver = true;
                    }
                    else
                    {
                        showMessageCallback("Monster hits you!");
                    }
                }
                else
                {
                    monsterView.Position = new Point(data.X, data.Y);
                    bool sawPlayer = false;

                    var monsterFov = currentFieldOfView.ComputeFov(data.X, data.Y, data.VisionSize, true);
                    foreach (var cell in monsterFov)
                    {
                        if (fovTiles.Any(f => f.X == data.X && f.Y == data.Y))
                        {
                            this[cell.X, cell.Y].ApplyEffect(MonsterVisionEffect);
                            if (playerView.Position.X == cell.X && playerView.Position.Y == cell.Y)
                            {
                                data.HuntPlayer();
                                monsterView.RenderCells.First().ActualForeground = new Color(255, 0, 0);
                                sawPlayer = true;
                            }
                        }
                        else
                        {
                            if (this.discoveredTiles.ContainsKey($"{data.X}, {data.Y}"))
                            {
                                this[cell.X, cell.Y].ApplyEffect(DiscoveredEffect);
                            }
                            else
                            {
                                this[cell.X, cell.Y].ApplyEffect(HiddenEffect);
                            }
                        }
                    }

                    if (!sawPlayer && monsterView.RenderCells.First().ActualForeground.R == 255)
                    {
                        monsterView.RenderCells.First().ActualForeground = new Color(data.Colour.R, data.Colour.G, data.Colour.B);
                    }
                }
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Moving took {elapsed}s");
        }

        private void CheckIfBlockPuzzleIsComplete()
        {
            if (currentMap.IsBlockPuzzleComplete())
            {
                var keys = currentMap.DeleteBlocksAndSpawnKeys();

                var toDelete = this.objects.Where(o => o.Data is PushReceptacle || o.Data is PushBlock).ToList();
                foreach (var e in toDelete)
                {
                    this.objects.Remove(e);
                }

                foreach (var key in keys)
                {
                    this.objects.Add(this.CreateGameObject("Key", key));
                }
            }
        }

        private void MarkCurrentFovAsVisible(IReadOnlyCollection<RogueSharp.ICell> fovTiles)
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

                this.discoveredTiles[$"{cell.X}, {cell.Y}"] = tile;
            }
        }

        private void MarkCurrentFovAsDiscovered(IReadOnlyCollection<RogueSharp.ICell> fovTiles)
        {
            foreach (var cell in fovTiles)
            {
                // Tell the map data (for FOV calculations) that we've discovered this tile
                this.currentMap.MarkAsDiscovered(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable);
                var tile = this[cell.X, cell.Y];
                tile.ApplyEffect(DiscoveredEffect);
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
            var playerView = this.objects.Single(g => g.Name == "Player");
            var mapWidth = Config.Instance.Get<int>("MapWidth");
            var mapHeight = Config.Instance.Get<int>("MapHeight");

            // Create the map
            this.currentMap = new CaveFloorMap(mapWidth, mapHeight);

            // Create the view
            var start = currentMap.AddPlayer(playerView.Data as Player);
            playerView.Position = new Point(start.X, start.Y);
            this.CenterViewToPlayer();

            var stairsDown = this.objects.Single(g => g.Name == "StairsDown");
            stairsDown.Move(currentMap.StairsDownPosition.X, currentMap.StairsDownPosition.Y);

            foreach (var obj in currentMap.Objects)
            {
                if (!this.objects.Any(o => o.Data == obj))
                {
                    this.CreateGameObject(string.Empty, obj);
                }
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

            // If there's something on it, don't draw anything
            if (this.currentMap.Objects.Any(e => e.X == x && e.Y == y))
            {
                return new CellAppearance(Color.DarkGray, Color.Transparent, ' ');
            }
            else if (this.currentMap.IsWalkable(x, y))
            {
                return new CellAppearance(Color.DarkGray, Color.Transparent, '.');
            }
            else
            {
                return new CellAppearance(Color.LightGray, Color.Transparent, '#');
            }
        }

        private GameObjectWithData CreateGameObject(string name, char display, Color colour, Point? position = null)
        {
            var toReturn = new GameObjectWithData(Engine.DefaultFont, null);
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

        private GameObjectWithData CreateGameObject(string name, Entity data)
        {
            var toReturn = new GameObjectWithData(Engine.DefaultFont, data);
            toReturn.Name = name;
            toReturn.DrawAs(data.DisplayCharacter, new Color(data.Colour.R, data.Colour.G, data.Colour.B));
            toReturn.IsVisible = false;
            toReturn.Position = new Point(data.X, data.Y);
            this.objects.Add(toReturn);
            return toReturn;
        }
    }
}
