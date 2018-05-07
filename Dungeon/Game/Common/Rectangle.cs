using System;

namespace Dungeon.Game.Common
{
    public class Rectangle : IEquatable<Rectangle>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

        public Rectangle(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public Rectangle()
        {

        }
        
        public static bool operator ==(Rectangle one, Rectangle other)
        {
            return one?.Equals(other) ?? false;
        }

        public static bool operator !=(Rectangle one, Rectangle other)
        {
            return !(one == other);
        }

        public bool Equals(Rectangle other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y && W == other.W && H == other.H;
        }

        public override bool Equals(object obj)
        {
            return !(obj is null) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((Rectangle) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ W;
                hashCode = (hashCode * 397) ^ H;
                return hashCode;
            }
        }
    }
}
