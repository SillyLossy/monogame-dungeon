using System.Collections.Generic;

namespace Dungeon.Game.Entities.Common
{
    public abstract class GameObject
    {
        // Holds the effects of a particular game object
        // e.g. burning door, cursed item, bleeding character
        public ICollection<Effect> Effects { get; set; } = new List<Effect>();
        
        // Points to a texture associated with this object
        public virtual int SpriteId { get; set; }

        protected GameObject(int spriteId)
        {
            SpriteId = spriteId;
        }
    }
}
