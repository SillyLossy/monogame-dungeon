namespace Dungeon.Game.World.Tiles
{
    public abstract class Tile
    {
        public abstract bool IsPassable { get; }

        public abstract bool IsTransparent { get; }

        public int SpriteId { get; private set; }

        protected Tile(int spriteId)
        {
            SpriteId = spriteId;
        }
        
        public static readonly Tile Wall = new WallTile(Sprites.Wall);
        public static readonly Tile Floor = new FloorTile(Sprites.Floor);
        public static readonly Tile Stone = new WallTile(Sprites.Stone);
        public static readonly Tile LadderUp = new FloorTile(Sprites.LadderUp);
        public static readonly Tile LadderDown = new FloorTile(Sprites.LadderDown);
        public static readonly Tile ValleyGrass = new FloorTile(Sprites.ValleyGrass);
        public static readonly Tile Water = new WallTile(Sprites.Water);
        public static readonly Tile ShallowWater =  new WaterTile(Sprites.ShallowWater);
        public static readonly Tile Sand = new FloorTile(Sprites.Sand);
        public static readonly Tile Mountain = new FloorTile(Sprites.Mountain);
        public static readonly Tile Snow = new FloorTile(Sprites.Snow);
        public static readonly Tile ForestGrass = new FloorTile(Sprites.ForestGrass);
        public static readonly Tile DeepWater = new WaterTile(Sprites.Water);
        public static readonly Tile Reserved = new FloorTile(Sprites.Reserved);
    }
}