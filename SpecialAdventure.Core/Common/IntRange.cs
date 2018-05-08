using System;

namespace SpecialAdventure.Core.Common
{
    public class IntRange : AbstractRange<int>
    {
        public IntRange(int min, int max) : base(min, max)
        {
        }

        public IntRange(int minMax) : base (minMax)
        {
        }

        public override int Value
        {
            get
            {
                int value = Random?.Next(Min, Max + 1) ?? RandomHelper.Random.Next(Min, Max + 1);
                return Nearest == default(int) ? value : (int) Math.Round(value / (double) Nearest) * Nearest;
            }
        }
    }
}