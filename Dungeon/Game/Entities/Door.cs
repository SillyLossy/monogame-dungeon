using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public class Door : Entity
    {
        public Door(Point initialPosition) : base(initialPosition)
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
    }
}
