namespace SpecialAdventure.Core.Entities.Common
{
    public abstract class Entity : GameObject
    {
        public abstract bool IsPassable { get; }

        public abstract bool IsTransparent { get; }

        protected Entity(int spriteId) : base(spriteId)
        {
        }
    }
}
