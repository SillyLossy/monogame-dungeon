using System;
using Dungeon.Game;
using Dungeon.Game.Common;
using Microsoft.Xna.Framework;

namespace Dungeon
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (var game = new DungeonGame())
                game.Run();
        }
    }
}
