using DeenGames.DeepGeo.Core.Entities;
using SadConsole;
using SadConsole.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.ConsoleUi.SadConsoleHelpers
{
    // ModelView (view with reference to model object)
    class GameObjectWithData : GameObject
    {
        public Entity Data { get; private set; }
        public GameObjectWithData(Font font, Entity data) : base(font)
        {
            this.Data = data;
        }
    }
}
