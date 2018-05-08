using System;

namespace SpecialAdventure.Core.Common
{
    public class Point : IEquatable<Point>
    {
        public readonly int X;
        public readonly int Y;
        
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static Point operator +(Point value1, Point value2)
        {
            return new Point(value1.X + value2.X, value1.Y + value2.Y);
        }

        public static Point operator -(Point value1, Point value2)
        {
            return new Point(value1.X - value2.X, value1.Y - value2.Y);
        }

        public static Point operator *(Point value1, Point value2)
        {
            return new Point(value1.X * value2.X, value1.Y * value2.Y);
        }

        public static Point operator *(Point value, int multiplier)
        {
            return new Point(value.X * multiplier, value.Y * multiplier);
        }

        public static Point operator /(Point source, Point divisor)
        {
            return new Point(source.X / divisor.X, source.Y / divisor.Y);
        }

        public static Point operator /(Point source, int divisor)
        {
            return new Point(source.X / divisor, source.Y / divisor);
        }

        public static bool operator ==(Point a, Point b)
        {
            if (b is null && a is null)
            {
                return true;
            }
            return !(a is null) && a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a is null) && !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && this.Equals(point);
        }

        public bool Equals(Point other)
        {
            return other != null && (X == other.X && Y == other.Y);
        }

        public override int GetHashCode()
        {
            return (X * 397) ^ Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }
        
        public static double EuclideanDistance(Point start, Point goal)
        {
            double num1 = start.X - goal.X;
            double num2 = start.Y - goal.Y;
            return Math.Sqrt(num1 * num1 + num2 * num2);
        }
    }
}