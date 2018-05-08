using SpecialAdventure.Core.Common;

namespace SpecialAdventure.Core.Entities.Common
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
