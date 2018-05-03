using Dungeon.Game.Levels;

namespace Dungeon.Game
{
    public static class TextureKey
    {
        private const string TexturesPath = "Textures";
        public const string Player = TexturesPath + "/" + nameof(Player);
        public const string Target = TexturesPath + "/" + nameof(Target);
        public const string Path = TexturesPath + "/" + nameof(Path);
        public const string DoorOpen = TexturesPath + "/" + nameof(DoorOpen);
        public const string DoorClosed = TexturesPath + "/" + nameof(DoorClosed);
        public const string Empty = "Empty";

        public static string FromTile(DungeonTile tile)
        {
            return $"{TexturesPath}/{tile}";
        }
    }
}