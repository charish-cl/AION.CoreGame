using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public class FlowField
    {
        public Grid<FlowNode> _grid;
        public FlowNode destinationCell;
        private float cellRadius;
        
        public FlowField( Vector2Int gridSize)
        {
            _grid = new Grid<FlowNode>(gridSize.x, gridSize.y, CreateGridObject, true);
            cellRadius=_grid.cellsize.x / 2;
        }

        private FlowNode CreateGridObject(Grid<FlowNode> grid, int x, int y)
        {
            grid.SetGridObject(x,y,new FlowNode(grid,grid.GetCellPosition(x,y),new Vector2Int(x,y)));
            return grid.GetGridObject(x, y);
        }

        /// <summary>
        ///设置目标点, 计算每个节点的花费
        /// </summary>
        /// <param name="_destinationCell"></param>
        public void CreateIntegrationField(FlowNode _destinationCell)
        {
            destinationCell = _destinationCell;

            destinationCell.cost = 0;
            destinationCell.bestCost = 0;

            Queue<FlowNode> cellsToCheck = new Queue<FlowNode>();

            cellsToCheck.Enqueue(destinationCell);

            while(cellsToCheck.Count > 0)
            {
                FlowNode curCell = cellsToCheck.Dequeue();
                List<FlowNode> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.quadRantDirections);
                foreach (FlowNode curNeighbor in curNeighbors)
                {
                    if (curNeighbor.cost == byte.MaxValue) { continue; }
                    if (curNeighbor.cost + curCell.bestCost < curNeighbor.bestCost)
                    {
                        curNeighbor.bestCost = (ushort)(curNeighbor.cost + curCell.bestCost);
                        cellsToCheck.Enqueue(curNeighbor);
                    }
                }
            }
        }
        //TODO:把Unity方法跟算法分开
        public void CreateCostField()
        {
            
            Vector3 cellHalfExtents = Vector3.one * cellRadius;
            foreach (var flowNode in _grid)
            {
                flowNode.RefreshCost();
                if (flowNode.nodeType == FlowNode.NodeType.Obstacle)
                {
                    flowNode.IncreaseCost(255);
                }
            }
            return;
            
            int terrainMask = LayerMask.GetMask("Impassible", "RoughTerrain");
            foreach (FlowNode curCell in _grid)
            {
               
                Collider[] obstacles = Physics.OverlapBox(curCell.worldPos, cellHalfExtents, Quaternion.identity, terrainMask);
                bool hasIncreasedCost = false;
                foreach (Collider col in obstacles)
                {
                    if (col.gameObject.layer == 8)
                    {
                        curCell.IncreaseCost(255);
                        continue;
                    }
                    else if (!hasIncreasedCost && col.gameObject.layer == 9)
                    {
                        curCell.IncreaseCost(3);
                        hasIncreasedCost = true;
                    }
                }
            }
        }
        /// <summary>
        /// 计算每个节点该流向的方向
        /// </summary>
        public void CreateFlowField()
        {
            foreach(FlowNode curCell in _grid)
            {
                List<FlowNode> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.AllDirections);
        
                int bestCost = curCell.bestCost;
        
                //找到周围cost最小的cell,指向花费最小的方向
                foreach(FlowNode curNeighbor in curNeighbors)
                {
                    if(curNeighbor.bestCost < bestCost)
                    {
                        bestCost = curNeighbor.bestCost;
                        //获取方向
                        curCell.bestDirection = GridDirection.GetDirectionFromV2I(curNeighbor.gridIndex - curCell.gridIndex);
                    }
                }
            }
        }

        //获取周围元素
        private List<FlowNode> GetNeighborCells(Vector2Int nodeIndex, List<GridDirection> directions)
        {
            List<FlowNode> neighborCells = new List<FlowNode>();

            foreach (Vector2Int curDirection in directions)
            {
                FlowNode newNeighbor = _grid.GetCellFromXY(nodeIndex+curDirection);
                if (newNeighbor != null)
                {
                    neighborCells.Add(newNeighbor);
                }
            }

            return neighborCells;
        }

        public FlowNode GetCellFromWorldPos(Vector3 worldMousePos)
        {
            return _grid.GetCellFromWordPosition(worldMousePos);
        }
    }
}