using Dungeon.Game.Common;
using Newtonsoft.Json;

namespace Dungeon.Game.Entities
{
    public class Door : Entity
    {
        public Door(Point initialPosition) : base(string.Empty, initialPosition)
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
        public override string TextureKey => IsOpen ? Game.TextureKey.DoorOpen : Game.TextureKey.DoorClosed;
    }
}
