using System;

namespace Dungeon.Game.Common
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

        public static Point operator /(Point source, Point divisor)
        {
            return new Point(source.X / divisor.X, source.Y / divisor.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
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
            return (17 * 23 + X) * 23 + Y;
        }

        public override string ToString()
        {
            return "{X:" + X + " Y:" + Y + "}";
        }
    }
}