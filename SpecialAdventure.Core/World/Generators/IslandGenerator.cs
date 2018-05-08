using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LibNoise;
using LibNoise.Primitive;
using Newtonsoft.Json;
using SpecialAdventure.Core.Common;
using SpecialAdventure.Core.Entities.Common;
using SpecialAdventure.Core.World.Levels;
using SpecialAdventure.Core.World.Tiles;
using Point = SpecialAdventure.Core.Common.Point;

namespace SpecialAdventure.Core.World.Generators
{
    public class IslandGenerator : LevelGenerator
    {
        private static readonly double[,] CircleGradient = JsonConvert.DeserializeObject<double[,]>(File.ReadAllText("gradient.json"));
        private readonly SimplexPerlin moistureNoise;

        private float GetNoise2D(int octaves, float persistence, float scale, int x, int y)
        {
            float total = 0;
            float frequency = scale;
            float amplitude = 1;

            // We have to keep track of the largest possible amplitude,
            // because each octave adds more, and we need a value in [-1, 1].
            float maxAmplitude = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += moistureNoise.GetValue(x * frequency, y * frequency) * amplitude;

                frequency *= 2;
                maxAmplitude += amplitude;
                amplitude *= persistence;
            }

            return total / maxAmplitude;
        }


        public IslandGenerator(int seed, int minDepth = 0, int maxDepth = 0) : base(seed, minDepth, maxDepth)
        {
            moistureNoise = new SimplexPerlin(seed, NoiseQuality.Best);
        }

        public override AbstractLevel Generate()
        {
            var settings = IslandFloorSettings.GetSettings(Random);
            var tiles = new Dictionary<Point, Tile>();

            var heightMap = GenerateHeightMap(settings.Width, settings.Height, settings.TerrainVariability);

            for (int x = 0; x < settings.Width; x++)
            {
                for (int y = 0; y < settings.Height; y++)
                {
                    tiles.Add(new Point(x, y), TransformHeightToTile(x, y, heightMap[x, y]));
                }
            }

            PlantTrees(tiles);
            settings.InitialPoint = FindInitialPoint(tiles, settings);

            var floor = new Floor(tiles, settings);

            return new Island(new[] { floor });
        }

        private void PlantTrees(IDictionary<Point, Tile> tiles)
        {
            var disk = new PoissonDisk(Random.Next());
            var points = disk.SampleRectangle(new Vector2(0, 0), new Vector2(2048, 2048), 10);
            foreach (var point in points)
            {
                if (tiles[point] is WaterTile)
                {
                    continue;
                }

                var tile = tiles[point];
                double roll = Random.NextDouble();

                if (tile.SpriteId == Sprites.Sand)
                {
                    if (roll < 0.1)
                    {
                        tiles[point] = Tile.Reserved;
                    }
                }

                if (tile.SpriteId == Sprites.ValleyGrass)
                {
                    if (roll < 0.5)
                    {
                        tiles[point] = Tile.Reserved;
                    }
                }

                if (tile.SpriteId == Sprites.ForestGrass)
                {
                    if (roll < 0.9)
                    {
                        tiles[point] = Tile.Reserved;
                    }
                }
            }
        }

        private Point FindInitialPoint(Dictionary<Point, Tile> tiles, IslandFloorSettings settings)
        {
            for (int i = 0; i < settings.Width * settings.Height; i++)
            {
                int x = Random.Next(settings.Width);
                int y = Random.Next(settings.Height);
                var point = new Point(x, y);
                if (!(tiles[point] is WaterTile))
                {
                    return point;
                }
            }
            throw new IndexOutOfRangeException("Cannot find an acceptable initial point");
        }

        private Tile TransformHeightToTile(int x, int y, double height)
        {
            int adjustedHeight = (int)Math.Ceiling(CircleGradient[x / 2, y / 2] * 1.2 * height);
            adjustedHeight = NumberUtil.Clamp(adjustedHeight, MinHeight, MaxHeight);

            double moisture = (GetNoise2D(3, 0.5f, 1, x, y) + 1d) / 2d;

            if (adjustedHeight < 5)
            {
                return Tile.DeepWater;
            }

            if (adjustedHeight < 7)
            {
                return Tile.Water;
            }

            if (adjustedHeight < 13)
            {
                return Tile.ShallowWater;
            }

            if (adjustedHeight < 16)
            {
                return Tile.Sand;
            }

            if (adjustedHeight < 40)
            {
                return Tile.ValleyGrass;
            }

            if (adjustedHeight < 65)
            {
                return Tile.ForestGrass;
            }

            if (adjustedHeight < 80)
            {
                return Tile.Stone;
            }

            if (adjustedHeight < 95)
            {
                return Tile.Mountain;
            }

            return Tile.Snow;
        }

        private const int MaxHeight = 100;
        private const int MinHeight = 0;


        /**
         * The 'square' step of diamond square.
         * 
         * Takes the 4 corners of a square of size 'size', which has the center (col, row).
         * Averages those 4 corners, applies some random perturbation, and then sets that as
         * the new height for the center point.
         */
        private void SquareStep(double[,] map, int col, int row, int size, double scale)
        {
            double NW = map[row - size / 2, col - size / 2];
            double NE = map[row - size / 2, col + size / 2];
            double SW = map[row + size / 2, col - size / 2];
            double SE = map[row + size / 2, col + size / 2];

            double avg = (NW + NE + SW + SE) / 4;
            map[row, col] = NumberUtil.WrapNumber(MinHeight, MaxHeight, avg + (Random.NextDouble() * scale * 2 - scale));
        }

        /**
         * The 'diamond' step of diamond square.
         * 
         * Takes the 4 points of a diamond of size 'size', which has the center (col, row).
         * Averages those 4 points, applies some random perturbation, and then sets that as
         * the new height for the center point.
         */
        private void DiamondStep(double[,] map, int col, int row, int size, int scale, int maxSize)
        {
            double sum = 0;
            var numSamples = 0;
            if (row - size / 2 >= 0)
            {
                sum += map[row - size / 2, col];
                numSamples++;
            }
            if (col + size / 2 < maxSize)
            {
                sum += map[row, col + size / 2];
                numSamples++;
            }
            if (row + size / 2 < maxSize)
            {
                sum += map[row + size / 2, col];
                numSamples++;
            }
            if (col - size / 2 >= 0)
            {
                sum += map[row, col - size / 2];
                numSamples++;
            }
            var avg = sum / numSamples;
            map[row, col] = NumberUtil.WrapNumber(MinHeight, MaxHeight, avg + (Random.NextDouble() * scale * 2 - scale));

        }

        private static int RoundToPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        /**
         * Generate a height map using the diamond-square method.
         */
        private double[,] GenerateHeightMap(int width, int height, int variability)
        {

            // Diamond-square needs a square grid of size 2^n + 1, so we take the longest side and 
            // scale our map to the next largest power of 2
            var largestCanvasSide = width > height ? width : height;
            int size = RoundToPowerOfTwo(largestCanvasSide) + 1;

            // Initialize Map
            var map = new double[size, size];

            // Seed the map corners
            map[0, 0] = MinHeight;
            map[0, size - 1] = MinHeight;
            map[size - 1, 0] = MinHeight;
            map[size - 1, size - 1] = MinHeight;

            var stepSize = size - 1;
            var scale = variability;

            // Do diamond-square
            while (stepSize > 1)
            {
                var halfStep = stepSize / 2;

                for (var r = halfStep; r < size; r += stepSize)
                {
                    for (var c = halfStep; c < size; c += stepSize)
                    {
                        SquareStep(map, c, r, stepSize, scale);
                    }
                }

                for (var r = 0; r < size; r += halfStep)
                {
                    for (var c = (r + halfStep) % stepSize; c < size; c += stepSize)
                    {
                        DiamondStep(map, c, r, stepSize, scale, size);
                    }
                }

                stepSize /= 2;
                scale /= 2;
            }

            return map;
        }
    }
}
