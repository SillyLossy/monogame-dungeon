using Dungeon.Game.Levels;

namespace Dungeon
{
    public static class TextureKey
    {
        private const string TexturesPath = "Textures";
        public const string Player = TexturesPath + "/" + nameof(Player);
        public const string Target = TexturesPath + "/" + nameof(Target);
        public const string Path = TexturesPath + "/" + nameof(Path);
        public const string OpenDoor = TexturesPath + "/" + nameof(OpenDoor);
        public const string ClosedDoor = TexturesPath + "/" + nameof(ClosedDoor);

        public static string FromTile(DungeonTile tile)
        {
            return $"Textures/{tile}";
        }
    }
}