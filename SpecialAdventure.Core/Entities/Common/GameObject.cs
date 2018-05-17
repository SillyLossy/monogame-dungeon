using System;
using System.Collections.Generic;

namespace SpecialAdventure.Core.Entities.Common
{
    public abstract class GameObject
    {
        // Holds the effects of a particular game object
        // e.g. burning door, cursed item, bleeding character
        public ICollection<Effect> Effects { get; set; } = new List<Effect>();
        
        // Points to a texture associated with this object
        public virtual int SpriteId { get; set; }

        public Guid Id { get; }

        protected GameObject(int spriteId)
        {
            Id = Guid.NewGuid();
            SpriteId = spriteId;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
