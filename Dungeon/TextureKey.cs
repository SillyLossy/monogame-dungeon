using Dungeon.Game.Levels;

namespace Dungeon
{
    public static class TextureKey
    {
        public const string Player = "Textures/Player";
        public const string Target = nameof(Target);
        public const string Path = nameof(Path);

        public static string FromTile(DungeonTile tile)
        {
            return $"Textures/{tile}";
        }
    }
}