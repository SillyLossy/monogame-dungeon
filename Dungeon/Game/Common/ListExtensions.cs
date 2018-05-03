using System.Collections.Generic;

namespace Dungeon.Game.Common
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = DungeonGame.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        } 
    }
}
