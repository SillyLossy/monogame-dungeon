using Dungeon.Game.Common;

namespace Dungeon.Game.Entities.Common
{
    public abstract class Entity : GameObject
    {
        public Point Position { get; set; }

        protected Entity(Point initialPosition, int spriteId) : base(spriteId)
        {
            Position = initialPosition;
        }
    }
}
