using System;

namespace Dungeon.Game.Common
{
    public abstract class AbstractRange<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public Random Random { get; set; }
        public T Nearest { get; set; }
        
        public AbstractRange(T min, T max)
        {
            Min = min;
            Max = max;
            Nearest = default(T);
        }

        public AbstractRange(T minMax)
        {
            Min = minMax;
            Max = minMax;
            Nearest = default(T);
        }

        public abstract T Value { get; }
    }
}