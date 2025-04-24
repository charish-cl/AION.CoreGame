
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit.Test
{
    public class FlowFiled_Test:MonoBehaviour
    {
        public Vector2Int gridSize;
        public float cellRadius = 0.5f;
        FlowField curFlowField;
        [LabelText("unit预制体")]
        public GameObject unitPrefab;
        private List<GameObject> unitsInGame=new List<GameObject>();
        public float moveSpeed=3;
        private int numUnitsPerSpawn=5;

        private void Start()
        {
            InitializeFlowField();
        }

        private void InitializeFlowField()
        {
            curFlowField = new FlowField(gridSize);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                curFlowField.CreateCostField();
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
                FlowNode destinationCell = curFlowField.GetCellFromWorldPos(worldMousePos);
                curFlowField.CreateIntegrationField(destinationCell);
                curFlowField.CreateFlowField();
                curFlowField._grid.RefreshRender();
                destinationCell.SetColor(Color.green);
            }
            if (Input.GetMouseButtonDown(1))
            {
              
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
                Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
                FlowNode destinationCell = curFlowField.GetCellFromWorldPos(worldMousePos);
                destinationCell.nodeType = FlowNode.NodeType.Obstacle;
                curFlowField.CreateCostField();
                curFlowField.CreateFlowField();
                curFlowField._grid.RefreshRender();
                destinationCell.SetColor(Color.black);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnUnits();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DestroyUnits();
            }
        }
        private void SpawnUnits()
        {
            List<FlowNode> nodes = new List<FlowNode>();
            foreach (var flowNode in curFlowField._grid)
            {
                if(flowNode.nodeType==FlowNode.NodeType.Normal)
                    nodes.Add(flowNode);
            }
            var cnt=nodes.Count;
         
            for (int i = 0; i < numUnitsPerSpawn; i++)
            {
                var randomIndex=Random.Range(0, cnt);
                var node = nodes[randomIndex];
                var go = Instantiate(unitPrefab, curFlowField._grid.GetCellPosition(node.gridIndex),Quaternion.identity);
                unitsInGame.Add(go);
            }
        }

        /// <summary>
        /// 让unit按照箭头方向移动
        /// </summary>
        private void FixedUpdate()
        {
            if (curFlowField == null) { return; }
            foreach (GameObject unit in unitsInGame)
            {
                FlowNode cellBelow = curFlowField.GetCellFromWorldPos(unit.transform.position);
                Vector3 moveDirection = new Vector3(cellBelow.bestDirection.Vector.x,cellBelow.bestDirection.Vector.y,0 );
                Rigidbody2D unitRB = unit.GetComponent<Rigidbody2D>();
                unitRB.velocity = moveDirection * moveSpeed;
            }
        }
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