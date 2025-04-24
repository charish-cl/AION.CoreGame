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
        private void Start()
        {
            Grid<GameObject> myGrid = new Grid<GameObject>(10, 10,CreateGridObject, true);
            foreach (var o in myGrid)
            {
                Debug.Log(o);
            }
        }

        private GameObject CreateGridObject(Grid<GameObject> grid, int x, int y)
        {
           var sprite = WordUtil.CreateWorldSprite("dwa", grid.GetCellPosition(x, y), grid.cellsize-new Vector3(1,1,0)*0.5f, Color.red);
           return sprite.gameObject;
        }
    }
}