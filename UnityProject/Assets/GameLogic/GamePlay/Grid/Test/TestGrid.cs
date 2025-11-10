using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GameDevKit
{
    public class TestGrid : MonoBehaviour
    {
        public SpriteRenderer sprite;

        // [Button]
        // public void GetDefaultTexture(Texture2D texture)
        // {
        //     Debug.Log( texture.width/25.2578);    
        //     Debug.Log( texture.height/25.2578);    
        //     sprite.sprite = texture.Texture2dToSprite();
        // }
        private Grid<GameObject> grid;


        [LabelText("网格大小（以1为单位）")] public Vector2Int gridSize = new Vector2Int(10, 10);

        [LabelText("网格单元大小（以1为单位）")] public Vector2 cellSize = new Vector2(1, 1);

        [LabelText("网格原点")] public Vector2 origin;

        [LabelText("网格单元格父物体")] public GameObject cellParent;

        private void OnEnable()
        {
            cellParent = gameObject;
            origin = cellParent.transform.position;
        }

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            InitGrid();
        }

        [Button("初始化网格", ButtonSizes.Large)]
        public void InitGrid()
        {
            origin = cellParent.transform.position;

            for (int i = cellParent.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(cellParent.transform.GetChild(i).gameObject);
            }

            grid = new Grid<GameObject>(CreateGridObject, gridSize, cellSize, origin, true);
            foreach (var o in grid)
            {
                o.transform.SetParent(cellParent.transform);
            }
        }

        private GameObject CreateGridObject(Grid<GameObject> grid, Vector2Int position)
        {
            var sprite = DebugWordUtil.CreateWorldSprite($"cell_{position.x}_{position.y}",
                grid.GetCellWorldPosition(position.x, position.y) + grid.CellSize / 2,
                grid.CellSize - new Vector2(0.3f, 0.3f), this.sprite.color);
            return sprite.gameObject;
        }


        // private void Update()
        // {
        //     if (grid == null)
        //     {
        //         return;
        //     }
        //
        //     //实时显示鼠标所在的单元格区域 SelectArea
        //     var selectCell = grid.GetByMousePosition();
        //     if (selectCell == null)
        //     {
        //         return;
        //     }
        //
        //     Vector2 worldPos = selectCell.transform.position;
        //     Debug.DrawLine(worldPos, new Vector2(worldPos.x, worldPos.y) + new Vector2(1, 0), Color.white);
        //
        //     foreach (var o in grid)
        //     {
        //         o.GetComponent<SpriteRenderer>().color = Color.white;
        //     }
        //
        //     if (grid.TryGetValueByWorldPosition(worldPos, out var cell))
        //     {
        //         //染成绿色
        //         DrawArea(worldPos, Color.green);
        //
        //
        //         if (Input.GetMouseButtonDown(0))
        //         {
        //             DrawArea(worldPos, Color.yellow);
        //         }
        //     }
        // }
        //
        // void DrawArea(Vector2 worldPos, Color color)
        // {
        //     //根据SelectArea的大小，染色
        //     var selectArea = new Rect(worldPos.x - SelectArea.x / 2, worldPos.y - SelectArea.y / 2, SelectArea.x,
        //         SelectArea.y);
        //     foreach (var c in grid.GetCellsInRect(selectArea))
        //     {
        //         c.GetComponent<SpriteRenderer>().color = color;
        //     }
        // }
        //
        // public Vector2 SelectArea = new Vector2(1, 1);

        [Button]
        public void SetVectorRight(Vector2 vector)
        {
            transform.right = (vector - (Vector2)transform.position).normalized;
        }

        
        
        
     
    }
}