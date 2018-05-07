using System;

namespace Dungeon.Game.World
{
    public class Location
    {
        public LocationType Type { get; set; }
        public Guid Id { get; set; }
        public int Depth { get; set; }

        public Location(LocationType type, Guid id, int depth = 0)
        {
            Type = type;
            Id = id;
            Depth = depth;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}