namespace Dungeon.Game.Common
{
    public class DoubleRange : AbstractRange<double>
    {
        public DoubleRange(double min, double max) : base(min, max)
        {
        }

        public DoubleRange(double minMax) : base(minMax)
        {
        }

        public override double Value
        {
            get
            {
                double value = Random?.NextDouble() ?? DungeonGame.Random.NextDouble();
                
                return value * (Max - Min) + Min;
            }
        }
    }
}