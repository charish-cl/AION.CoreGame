using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    public class AstarTest:MonoBehaviour
    {
        public Vector2Int gridSize;
        public float cellRadius = 0.5f;
        AStarPathfinding pathfinding;
        [LabelText("unit预制体")]
        public GameObject unitPrefab;
        public float moveSpeed=3;
        private int numUnitsPerSpawn=5;

        public SpriteRenderer[][] SpriteRenderers;

        public Vector2 CellSize;
        private void Start()
        {
            SpriteRenderers = new SpriteRenderer[gridSize.x][];
            for (int i = 0; i < gridSize.x; i++)
            {
                SpriteRenderers[i] = new SpriteRenderer[gridSize.y];
            }
            pathfinding = new AStarPathfinding(CreateGridObject, gridSize.x, gridSize.y);
            CellSize = pathfinding.grid.CellSize;
            
            
            //随机几个障碍物
            for (int i = 0; i < 10; i++)
            {
                var x = Random.Range(0, gridSize.x);
                var y = Random.Range(0, gridSize.y);
                var node = pathfinding.grid.GetValue(x, y);
                
                // SetObstacle(node);
            }
            
            
         
        }
        private AstarNode CreateGridObject(Grid<AstarNode> arg1,Vector2Int arg2)
        {
            onCreateCell(arg1,arg2.x,arg2.y);
            return new AstarNode( arg2.x,arg2.y);
        }
        private void onCreateCell(Grid<AstarNode> grid, int arg1, int arg2)
        { 
            var coordinate = new Vector2Int(arg1, arg2);
            var sprite = DebugWordUtil.CreateWorldSprite("dwa", grid.GetCellWorldPosition(coordinate)+grid.CellSize/2, grid.CellSize-new Vector2(1,1)*0.5f, Color.red);
            SpriteRenderers[arg1][arg2] = sprite.GetComponent<SpriteRenderer>();
            
        }


        public void SetObstacle(AstarNode node)
        {
            node.isWalkable = false;
            SpriteRenderers[node.x][node.y].color = Color.white;
        }
        public void SetNormal(AstarNode node)
        {
            node.isWalkable = true;
            SpriteRenderers[node.x][node.y].color = Color.red;
        }
        public Vector2Int AreaSize = new Vector2Int(3, 3);
        private void Update()
        {
            
            foreach (var astarNode in pathfinding.grid)
            {
                SetNormal(astarNode);
            }
            // if (Input.GetMouseButtonDown(0))
            // {
            var mouseCell = pathfinding.grid.GetByMousePosition();
            // SetObstacle(cell);

            if (mouseCell == null)
            {
                return;
            }
            var boundCell = pathfinding.grid.GetBoundCell(mouseCell.coordinate, AreaSize.x, AreaSize.y);
            if (boundCell == null)
            {
                return;
            }
            foreach (var astarNode in boundCell)
            {
                SetObstacle(astarNode);
            }
            
            // }
            if (Input.GetMouseButtonDown(1))
            {
                var cell = pathfinding.grid.GetByMousePosition();
                var result = pathfinding.FindPath(Vector2.zero, pathfinding.grid.GetCellWorldPosition(cell.coordinate));
                
                for (var i = 0; i < result.Count-1; i++)
                {
                    Debug.DrawLine(result[i]+CellSize/2,result[i+1]+CellSize/2,Color.green,20);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //SpawnUnits();
            }
            
        }
        
    }
}