using System;
using GameDevKit.GameLogic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GameDevKit
{
    public class TestGrid:MonoBehaviour
    {
        public SpriteRenderer sprite;
        [Button]
        public void GetDefaultTexture(Texture2D texture)
        {
            Debug.Log( texture.width/25.2578);    
            Debug.Log( texture.height/25.2578);    
            sprite.sprite = texture.Texture2dToSprite();
        }
        private Grid<GameObject> grid;
        private void Start()
        {
            grid = new Grid<GameObject>(CreateGridObject,new Vector2Int(10,10),Vector2.one*4 ,-Vector2.one*20, true);
            foreach (var o in grid)
            {
                Debug.Log(o);
            }
        }

        private GameObject CreateGridObject(Grid<GameObject> grid, Vector2Int position)
        {
           var sprite = DebugWordUtil.CreateWorldSprite("dwa", grid.GetCellWorldPosition(position.x, position.y)+grid.CellSize/2, grid.CellSize-new Vector2(1,1)*0.5f, Color.red);
           return sprite.gameObject;
        }

        private Vector2 worldPos;
        private void Update()
        {
            if (grid == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = Input.mousePosition;
                
                 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y));
                var cellPos = grid.GetValueByWorldPosition(worldPos);

                if (cellPos == null)
                {
                    return;
                }
                cellPos.GetComponent<SpriteRenderer>().color = Color.white;
            }
          
        }

        public void OnDrawGizmos()
        {
            
            Gizmos.DrawLine(worldPos, new Vector2(worldPos.x, worldPos.y) + new Vector2(1, 0));
        }
    }
}