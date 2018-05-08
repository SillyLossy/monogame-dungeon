using System;

namespace SpecialAdventure.Core.Common
{
    public static class NumberUtil
    {
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : (val.CompareTo(max) > 0 ? max : val);
        }

        /** 
         * 'Wrap' the value so it falls within the range [min, max], respecting the 'direction'
         * of the wrap.
         *      e.g. with a range [0, 10]: 12 'wraps' to 8 while 24 'wraps' to 4 
         */

        public static double WrapNumber(int min, int max, double value)
        {
            double remainder = (Math.Abs(value) % max);
            int numWraps = (int)Math.Floor(Math.Abs(value) / max);
            double val = 0;
            if (numWraps % 2 == 0)
                val = min + remainder;
            else
                val = max - remainder;

            return val;
        }

        public static double Rescale(double val, double min, double max)
        {
            return (val - min) / max;
        }
    }
}
