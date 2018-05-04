using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Priority_Queue;

namespace Dungeon.Game.Levels
{
    public static class PathFinder
    {
        private static double HeuristicCostEstimate(Point start, Point goal)
        {
            double num1 = start.X - goal.X;
            double num2 = start.Y - goal.Y;
            return Math.Sqrt(num1 * num1 + num2 * num2);
        }

        private static LinkedList<Point> ReconstructPath(IDictionary<Point, Point> cameFrom, Point current,
            Point start)
        {
            var result = new LinkedList<Point>();
            Point? currentNode = current;
            while (true)
            {
                result.AddFirst(currentNode.Value);
                currentNode = cameFrom[currentNode.Value];
                if (currentNode.Value == start)
                {
                    break;
                } 
            }
            
            return result;
        }

        public static LinkedList<Point> AStar(DungeonFloor floor, Point start, Point goal, bool ignoreEntities = false)
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
                ;
                foreach (var next in floor.GetNeighbors(current, ignoreEntities))
                {
                    double newCost = costSoFar[current] + HeuristicCostEstimate(current, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + HeuristicCostEstimate(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return null;
        }
    }
}

