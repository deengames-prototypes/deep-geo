using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.ConsoleUi
{
    /// <summary>
    /// Gets global configuration for the view only. Yes, it's evil, because it's used everywhere.
    /// </summary>
    public static class UiConfig
    {
        public static int GameWidth { get; } = 80;
        public static int GameHeight { get; } = 25;
    }
}
