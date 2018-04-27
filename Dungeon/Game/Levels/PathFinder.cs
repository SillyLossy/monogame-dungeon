using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Levels
{
    public static class DictionaryExtensions
    {

        public static KeyValuePair<TKey, TValue> MinValuePair<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, ISet<TKey> set) where TValue : IComparable<TValue>
        {
            KeyValuePair<TKey, TValue>? minPair = null;

            foreach (var pair in dictionary)
            {
                if (minPair != null && minPair.Value.Value.CompareTo(pair.Value) < 0)
                {
                    continue;
                }
                if (set.Contains(pair.Key))
                {
                    minPair = pair;
                }
            }

            return minPair ?? throw new InvalidOperationException("Dictionary is empty");
        }
    }
    public static class PathFinder
    {

        private class DefaultableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
            private readonly TValue _default;

            public DefaultableDictionary(TValue defaultValue)
            {
                _default = defaultValue;
            }

            public new TValue this[TKey key]
            {
                get => TryGetValue(key, out var t) ? t : _default;
                set => base[key] = value;
            }
        }


        private static float HeuristicCostEstimate(Point start, Point goal)
        {
            return Vector2.Distance(start.ToVector2(), goal.ToVector2());
        }

        private static Stack<Point> ReconstructPath(DefaultableDictionary<Point, Point?> cameFrom, Point current)
        {
            var result = new Stack<Point>();
            Point? currentNode = current;
            while (currentNode != null)
            {
                result.Push(currentNode.Value);
                currentNode = cameFrom[currentNode.Value];
            }
            
            return result;
        }

        public static Stack<Point> AStar(DungeonFloor floor, Point start, Point goal)
        {
            // The set of nodes already evaluated
            var closedSet = new HashSet<Point>();

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            var openSet = new HashSet<Point> { start };

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step
            var cameFrom = new DefaultableDictionary<Point, Point?>(null);

            // For each node, the cost of getting from the start node to that node.
            // The cost of going from start to start is zero.
            var gScore = new DefaultableDictionary<Point, float>(float.PositiveInfinity) { { start, 0 } };

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            // For the first node, that value is completely heuristic.
            var fScore = new DefaultableDictionary<Point, float>(float.PositiveInfinity) { { start, HeuristicCostEstimate(start, goal) } };

            while (openSet.Count != 0)
            {
                var current = fScore.MinValuePair(openSet).Key;

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);
                foreach (var neighbor in floor.GetNeighbors(current).Where(neighbor => !closedSet.Contains(neighbor)))
                {
                    if (!openSet.Contains(neighbor)) // Discover a new node
                    {
                        openSet.Add(neighbor);
                    }

                    // The distance from start to a neighbor
                    float tentativeGScore = gScore[current] + Vector2.Distance(current.ToVector2(), neighbor.ToVector2());
                    if (tentativeGScore >= gScore[neighbor])
                    {
                        continue; // This is not a better path.
                    }

                    // This path is the best until now. Record it!
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
                }
            }

            return null;
        }
    }
}

