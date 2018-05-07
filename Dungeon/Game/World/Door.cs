using Dungeon.Game.Common;
using Dungeon.Game.Entities.Common;
using Newtonsoft.Json;

namespace Dungeon.Game.World
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

        [JsonIgnore]
        public override int SpriteId => IsOpen ? Sprites.DoorOpen : Sprites.DoorClosed;
    }
}
