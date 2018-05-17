namespace SpecialAdventure.Core.Entities.Common
{
    public class Door : Entity
    {
        public Door() : base(Sprites.Reserved)
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
        public override bool IsPassable => IsOpen;
        public override bool IsTransparent => IsOpen;
    }
}
