using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Common;

namespace SpecialAdventure.Core.World
{
    public class Door : Entity
    {
        public Door(Point initialPosition) : base(initialPosition, Sprites.Reserved)
        {
        }

        public bool IsOpen { get; set; }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }
        
        public override int SpriteId => IsOpen ? Sprites.DoorOpen : Sprites.DoorClosed;
    }
}
