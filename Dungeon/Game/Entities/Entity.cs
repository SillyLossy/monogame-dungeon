using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public abstract class Entity : GameObject
    {
        public Point Position { get; set; }

        protected Entity(string textureKey, Point initialPosition) : base(textureKey)
        {
            Position = initialPosition;
        }
    }
}
