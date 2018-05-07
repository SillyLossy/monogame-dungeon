using System;
using System.Collections.Generic;
using Dungeon.Game.Common;
using Priority_Queue;

namespace Dungeon.Game.World
{
    public static class PathFinder
    {
        private static LinkedList<Point> ReconstructPath(IDictionary<Point, Point> cameFrom, Point current,
            Point start)
        {
            var result = new LinkedList<Point>();
            Point currentNode = current;
            while (true)
            {
                result.AddFirst(currentNode);
                currentNode = cameFrom[currentNode];
                if (currentNode == start)
                {
                    break;
                }
            }

            return result;
        }

        public static LinkedList<Point> AStar(Floor floor, Point start, Point goal, bool ignoreEntities = false)
        {
            var frontier = new SimplePriorityQueue<Point, double>();
            var cameFrom = new Dictionary<Point, Point>();
            var costSoFar = new Dictionary<Point, double>();

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count != 0)
            {
                Point current = frontier.Dequeue();

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current, start);
                }

                foreach (var next in floor.GetNeighbors(current, ignoreEntities))
                {
                    double newCost = costSoFar[current] + Point.EuclideanDistance(current, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Point.EuclideanDistance(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return null;
        }
    }
}

