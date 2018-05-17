namespace SpecialAdventure.Core.Entities.Common
{
    public class Tree : Entity
    {
        public Tree(int spriteId) : base(spriteId)
        {
        }

        public override bool IsPassable => false;
        public override bool IsTransparent => false;
    }
}
