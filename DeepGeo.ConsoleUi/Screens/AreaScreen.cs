using Microsoft.Xna.Framework;
using SadConsole.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = SadConsole.Consoles.Console;
using SadConsole.Input;
using DeenGames.DeepGeo.ConsoleUi.Windows;

namespace DeenGames.DeepGeo.ConsoleUi.Screens
{
    /// <summary>
    /// An area, like a dungeon floor, town, or an outdoor place with enemies/NPCs
    /// </summary>
    class AreaScreen : ConsoleList
    {
        private readonly MessageAndStatusWindow messageAndStatusConsole;
        private readonly AreaViewWindow mainView;

        public AreaScreen()
        {
            // Clear the default console
            SadConsole.Engine.ConsoleRenderStack.Clear();
            
            messageAndStatusConsole = new MessageAndStatusWindow();
            mainView = new AreaViewWindow(UiConfig.GameWidth, UiConfig.GameHeight - messageAndStatusConsole.Height,
                (message) => messageAndStatusConsole.ShowMessage(message));

            mainView.Position = new Point(0, 0);
            messageAndStatusConsole.Position = new Point(0, mainView.Height);

            this.Add(mainView);
            this.Add(messageAndStatusConsole);

            messageAndStatusConsole.ShowMessage("Welcome to Deep Geo!");
        }
    }
}
