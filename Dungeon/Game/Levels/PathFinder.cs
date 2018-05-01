using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Dungeon.Game.Levels
{
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


        private static double HeuristicCostEstimate(Point start, Point goal)
        {
            double num1 = start.X - goal.X;
            double num2 = start.Y - goal.Y;
            return Math.Sqrt(num1 * num1 + num2 * num2);
        }

        private static LinkedList<Point> ReconstructPath(DefaultableDictionary<Point, Point?> cameFrom, Point current)
        {
            var result = new LinkedList<Point>();
            Point? currentNode = current;
            while (currentNode != null)
            {
                result.AddFirst(currentNode.Value);
                currentNode = cameFrom[currentNode.Value];
            }
            
            return result;
        }

        public static LinkedList<Point> AStar(DungeonFloor floor, Point start, Point goal, bool ignoreEntities = false)
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
            var gScore = new DefaultableDictionary<Point, double>(double.PositiveInfinity) { { start, 0 } };

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            // For the first node, that value is completely heuristic.
            var fScore = new DefaultableDictionary<Point, double>(double.PositiveInfinity) { { start, HeuristicCostEstimate(start, goal) } };
            
            while (openSet.Count != 0)
            {
                var current = fScore.KeyWithMinValueFromKeySet(openSet, double.PositiveInfinity);

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);
                foreach (var neighbor in floor.GetNeighbors(current, ignoreEntities).Where(neighbor => !closedSet.Contains(neighbor)))
                {
                    if (!openSet.Contains(neighbor)) // Discover a new node
                    {
                        openSet.Add(neighbor);
                    }

                    // The distance from start to a neighbor
                    double tentativeGScore = gScore[current] + HeuristicCostEstimate(start, goal);
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

