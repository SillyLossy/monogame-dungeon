using Microsoft.Xna.Framework;

namespace Dungeon.Game.Entities
{
    public class Player : Character
    {
        public Player(string name, string textureKey, PrimaryAttributes primaryAttributes, Point initialPosition) : base(name, textureKey, primaryAttributes, initialPosition)
        {
        }

        public override AttackResult Attack(Character target)
        {
            var result = base.Attack(target);
            DungeonGame.Log.LogAttack(result);
            return result;
        }
    }
}
