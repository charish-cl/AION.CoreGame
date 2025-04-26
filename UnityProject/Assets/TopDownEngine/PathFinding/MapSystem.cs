using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TopDownEngine.PathFinding
{
    public class MapSystem:SerializedMonoBehaviour
    {
        
        private Dictionary<long,MapCell> mapCells = new Dictionary<long, MapCell>();
       
        private MapCell[,] mapArray;

        
        //这里传进来的都是能看到的格子，所以不需要考虑障碍物
        public void Initialize(List<MapCell> mapCells)
        {
            
            //一边构造Array
        }
        public void GetMouseClickCell()
        {
        
            
            
            // Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            // Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            // var cell = GetCellFromWordPosition(worldMousePos);
            // return cell;
        }
    }
}