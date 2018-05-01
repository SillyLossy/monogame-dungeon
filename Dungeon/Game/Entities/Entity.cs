using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public abstract class Entity
    {
        public Point Position { get; set; }

        public Entity(Point initialPosition)
        {
            Position = initialPosition;
        }
    }
}
