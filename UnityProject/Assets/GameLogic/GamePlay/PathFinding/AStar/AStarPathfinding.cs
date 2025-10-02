using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevKit
{
    public class AStarPathfinding
    {
        //直线花费
        private const int MOVE_STRAIGHT_COST = 10;

        //斜边的花费
        private const int MOVE_DIAGONAL_COST = 14;

        public static AStarPathfinding Instance { get; private set; }

        public Grid<AstarNode> grid;
        private List<AstarNode> openList;
        private List<AstarNode> closedList;

        public AStarPathfinding(Func<Grid<AstarNode>, Vector2Int, AstarNode> CreateGridObject, int width, int height)
        {
            Instance = this;
            grid = new Grid<AstarNode>(CreateGridObject, width, height);
        }

        public List<Vector2> FindPath(Vector2 startWorldPosition, Vector2 endWorldPosition)
        {
            var (startX,startY) = grid.GetGridCoordinates(startWorldPosition);
            var (endX,endY) = grid.GetGridCoordinates(endWorldPosition);

            List<AstarNode> path = FindPath(startX, startY, endX, endY);
            if (path == null)
            {
                return null;
            }
            else
            {
                List<Vector2> vectorPath = new List<Vector2>();
                foreach (AstarNode pathNode in path)
                {
                    vectorPath.Add(grid.GetCellWorldPosition(pathNode.coordinate.x, pathNode.coordinate.y));
                }

                return vectorPath;
            }
        }

        public List<AstarNode> FindPath(int startX, int startY, int endX, int endY)
        {
            AstarNode startNode = grid.GetValue(startX, startY);
            AstarNode endNode = grid.GetValue(endX, endY);

            if (startNode == null || endNode == null)
            {
                // Invalid Path
                return null;
            }

            openList = new List<AstarNode> { startNode };
            closedList = new List<AstarNode>();

            //初始化所有节点
            foreach (var pathNode in grid)
            {
                pathNode.gcost = Int32.MaxValue;
                pathNode.CalCulateCost();
                pathNode.fromNode = null;
            }

            startNode.gcost = 0;
            startNode.hcost = CalculateDistanceCost(startNode, endNode);
            startNode.CalCulateCost();

            while (openList.Count > 0)
            {
                AstarNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (AstarNode neighbourNode in grid.GetNeighbors(currentNode.coordinate))
                {
                    if (closedList.Contains(neighbourNode)) continue;
                    if (!neighbourNode.isWalkable)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gcost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gcost)
                    {
                        neighbourNode.fromNode = currentNode;
                        neighbourNode.gcost = tentativeGCost;
                        neighbourNode.hcost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalCulateCost();
                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            // Out of nodes on the openList
            return null;
        }

        //得到路径
        private List<AstarNode> CalculatePath(AstarNode endNode)
        {
            List<AstarNode> path = new List<AstarNode>();
            path.Add(endNode);
            AstarNode currentNode = endNode;
            while (currentNode.fromNode != null)
            {
                path.Add(currentNode.fromNode);
                currentNode = currentNode.fromNode;
            }

            path.Reverse();
            return path;
        }

        //对角线距离
        private int CalculateDistanceCost(AstarNode a, AstarNode b)
        {
            return ManhattanDistance(a, b);
        }

// 曼哈顿启发式距离
        private int ManhattanDistance(AstarNode a, AstarNode b)
        {
            int xDistance = Math.Abs(a.x - b.x);
            int yDistance = Math.Abs(a.y - b.y);
            return xDistance + yDistance;
        }

// 对角线距离
        private int DiagonalDistance(AstarNode a, AstarNode b)
        {
            int xDistance = Math.Abs(a.x - b.x);
            int yDistance = Math.Abs(a.y - b.y);
            int remaining = Math.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

// 欧几里得距离
        private int EuclideanDistance(AstarNode a, AstarNode b)
        {
            int xDistance = a.x - b.x;
            int yDistance = a.y - b.y;
            return (int)Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }

        //获取最小f花费的元素
        private AstarNode GetLowestFCostNode(List<AstarNode> pathNodeList)
        {
            return pathNodeList.OrderBy(e => e.fcost).First();
        }
    }
}