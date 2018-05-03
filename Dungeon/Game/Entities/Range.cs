namespace Dungeon.Game.Entities
{
    public class Range
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public Range(int minMax)
        {
            Min = minMax;
            Max = minMax;
        }

        public int Value => DungeonGame.Random.Next(Min, Max + 1);
    }
}