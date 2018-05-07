using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Common
{
    // Currently unused though could be implemented.
    // http://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
    // Adapated from java source by Herman Tulleken
    // http://www.luma.co.za/labs/2008/02/27/poisson-disk-sampling/
    // The algorithm is from the "Fast Poisson Disk Sampling in Arbitrary Dimensions" paper by Robert Bridson
    // http://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
    public class PoissonDisk
    {
        private const int DefaultPointsPerIteration = 30;
        private static readonly float SquareRootTwo = (float)Math.Sqrt(2);
        private const float TwoPi = (float)(Math.PI * 2);
        private readonly Random random;

        private struct Settings
        {
            public Vector2 BottomLeft, TopRight, Center;
            public Vector2 Dimensions;
            public float? RejectionSqDistance;
            public float MinimumDistance;
            public float CellSize;
            public int GridWidth, GridHeight;
        }

        private struct State
        {
            public Vector2?[,] Grid;
            public List<Vector2> ActivePoints, Points;
        }

        public PoissonDisk(int seed)
        {
            random = new Random(seed);
        }

        public List<Vector2> SampleCircle(Vector2 center, float radius, float minimumDistance, int pointsPerIteration = DefaultPointsPerIteration)
        {
            return Sample(center - new Vector2(radius), center + new Vector2(radius), radius, minimumDistance, pointsPerIteration);
        }

        public HashSet<Point> SampleRectangle(Vector2 bottomLeft, Vector2 topRight, float minimumDistance, int pointsPerIteration = DefaultPointsPerIteration)
        {
            return Transform(Sample(bottomLeft, topRight, null, minimumDistance, pointsPerIteration));
        }

        private HashSet<Point> Transform(IEnumerable<Vector2> vectors)
        {
            var set = new HashSet<Point>();

            foreach (var vector in vectors)
            {
                set.Add(new Point((int) Math.Floor(vector.X), (int) Math.Floor(vector.Y)));
            }

            return set;
        }

        private List<Vector2> Sample(Vector2 bottomLeft, Vector2 topRight, float? rejectionDistance, float minimumDistance, int pointsPerIteration)
        {
            var settings = new Settings
            {
                BottomLeft = bottomLeft,
                TopRight = topRight,
                Dimensions = topRight - bottomLeft,
                Center = (bottomLeft + topRight) / 2,
                CellSize = minimumDistance / SquareRootTwo,
                MinimumDistance = minimumDistance,
                RejectionSqDistance = rejectionDistance * rejectionDistance
            };
            settings.GridWidth = (int)(settings.Dimensions.X / settings.CellSize) + 1;
            settings.GridHeight = (int)(settings.Dimensions.Y / settings.CellSize) + 1;

            var state = new State
            {
                Grid = new Vector2?[settings.GridWidth, settings.GridHeight],
                ActivePoints = new List<Vector2>(),
                Points = new List<Vector2>()
            };

            AddFirstPoint(ref settings, ref state);

            while (state.ActivePoints.Count != 0)
            {
                var listIndex = random.Next(state.ActivePoints.Count);

                var point = state.ActivePoints[listIndex];
                var found = false;

                for (var k = 0; k < pointsPerIteration; k++)
                    found |= AddNextPoint(point, ref settings, ref state);

                if (!found)
                    state.ActivePoints.RemoveAt(listIndex);
            }

            return state.Points;
        }

        private void AddFirstPoint(ref Settings settings, ref State state)
        {
            var added = false;
            while (!added)
            {
                var d = random.NextDouble();
                var xr = settings.BottomLeft.X + settings.Dimensions.X * d;

                d = random.NextDouble();
                var yr = settings.BottomLeft.Y + settings.Dimensions.Y * d;

                var p = new Vector2((float)xr, (float)yr);
                if (settings.RejectionSqDistance != null && Vector2.DistanceSquared(settings.Center, p) > settings.RejectionSqDistance)
                    continue;
                added = true;

                var index = Denormalize(p, settings.BottomLeft, settings.CellSize);

                state.Grid[(int)index.X, (int)index.Y] = p;

                state.ActivePoints.Add(p);
                state.Points.Add(p);
            }
        }

        private bool AddNextPoint(Vector2 point, ref Settings settings, ref State state)
        {
            var found = false;
            var q = GenerateRandomAround(point, settings.MinimumDistance);

            if (q.X >= settings.BottomLeft.X && q.X < settings.TopRight.X &&
                q.Y > settings.BottomLeft.Y && q.Y < settings.TopRight.Y &&
                (settings.RejectionSqDistance == null || Vector2.DistanceSquared(settings.Center, q) <= settings.RejectionSqDistance))
            {
                var qIndex = Denormalize(q, settings.BottomLeft, settings.CellSize);
                var tooClose = false;

                for (var i = (int)Math.Max(0, qIndex.X - 2); i < Math.Min(settings.GridWidth, qIndex.X + 3) && !tooClose; i++)
                    for (var j = (int)Math.Max(0, qIndex.Y - 2); j < Math.Min(settings.GridHeight, qIndex.Y + 3) && !tooClose; j++)
                        if (state.Grid[i, j].HasValue && Vector2.Distance(state.Grid[i, j].Value, q) < settings.MinimumDistance)
                            tooClose = true;

                if (!tooClose)
                {
                    found = true;
                    state.ActivePoints.Add(q);
                    state.Points.Add(q);
                    state.Grid[(int)qIndex.X, (int)qIndex.Y] = q;
                }
            }
            return found;
        }

        private Vector2 GenerateRandomAround(Vector2 center, float minimumDistance)
        {
            var d = random.NextDouble();
            var radius = minimumDistance + minimumDistance * d;

            d = random.NextDouble();
            var angle = TwoPi * d;

            var newX = radius * Math.Sin(angle);
            var newY = radius * Math.Cos(angle);

            return new Vector2((float)(center.X + newX), (float)(center.Y + newY));
        }

        private static Vector2 Denormalize(Vector2 point, Vector2 origin, double cellSize)
        {
            return new Vector2((int)((point.X - origin.X) / cellSize), (int)((point.Y - origin.Y) / cellSize));
        }
    }
}