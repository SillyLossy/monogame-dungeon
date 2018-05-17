using System;

namespace SpecialAdventure.Core.World
{
    public class Location
    {
        public LocationType Type { get; }
        public Guid Id { get;  }
        public int Depth { get; }

        public Location(LocationType type, Guid id, int depth)
        {
            Type = type;
            Id = id;
            Depth = depth;
        }

        public static bool operator ==(Location first, Location second)
        {
            return first?.Id == second?.Id;
        }

        public static bool operator !=(Location first, Location second)
        {
            return first?.Id != second?.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is Location location && this == location;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}