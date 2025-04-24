using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameDevKit
{
    public class AstarTest:MonoBehaviour
    {
        public Vector2Int gridSize;
        public float cellRadius = 0.5f;
        AStarPathfinding pathfinding;
        [LabelText("unit预制体")]
        public GameObject unitPrefab;
        private List<GameObject> unitsInGame=new List<GameObject>();
        public float moveSpeed=3;
        private int numUnitsPerSpawn=5;

        private void Start()
        {
            pathfinding = new AStarPathfinding(gridSize.x, gridSize.y);
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var cell = pathfinding.grid.GetMouseTargetCell();
                cell.isWalkable = false;
                cell.SetColor(Color.black);
            }
            if (Input.GetMouseButtonDown(1))
            {
                var cell = pathfinding.grid.GetMouseTargetCell();
                var result = pathfinding.FindPath(Vector2.zero, pathfinding.grid.GetCellPosition(cell.coordinate));
                
                for (var i = 0; i < result.Count-1; i++)
                {
                    Debug.DrawLine(result[i],result[i+1],Color.green,2);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //SpawnUnits();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DestroyUnits();
            }
        }
        // private void SpawnUnits()
        // {
        //      var nodes = new  List<AstarNode>();
        //     foreach (var flowNode in pathfinding._grid)
        //     {
        //         if(flowNode)
        //             nodes.Add(flowNode);
        //     }
        //     var cnt=nodes.Count;
        //  
        //     for (int i = 0; i < numUnitsPerSpawn; i++)
        //     {
        //         var randomIndex=Random.Range(0, cnt);
        //         var node = nodes[randomIndex];
        //         var go = Instantiate(unitPrefab, curFlowField._grid.GetCellPosition(node.gridIndex),Quaternion.identity);
        //         unitsInGame.Add(go);
        //     }
        // }
        
        private void DestroyUnits()
        {
            foreach (GameObject go in unitsInGame)
            {
                Destroy(go);
            }
            unitsInGame.Clear();
        }
    }
}