using UnityEngine;

namespace GameDevKit
{
    public class FlowNode
    {
        public Vector3 worldPos;
        //网格的坐标	
        public Vector2Int gridIndex;
        //上一个节点
        public FlowNode fromNode;
        public byte cost;
        public ushort bestCost;
        public GridDirection bestDirection;
        public GameObject sprite;
        
        public NodeType nodeType = NodeType.Normal;
        
        public enum NodeType
        {
            Obstacle,
            Normal,
        }
        public void SetColor(Color color)
        {
            sprite.SetColor(color);    
        }

        public void RefreshCost()
        {
            cost = 1;
            bestCost = ushort.MaxValue; 
        }
        public override string ToString()
        {
            return $"{cost}" +"\n"+
                   $"{bestCost}";
        }

        public float padding=0.5f;
        public FlowNode(Grid<FlowNode> grid,Vector3 _worldPos,Vector2Int gridIndex)
        {
            worldPos = _worldPos;
            this.gridIndex = gridIndex;
            cost = 1;
            bestCost = ushort.MaxValue;            
            bestDirection = GridDirection.None;
            
            sprite = WordUtil.CreateWorldSprite(ToString(), 
                grid.GetCellPosition(gridIndex),
                grid.cellsize-new Vector3(1,1,0)*padding,
                Color.red);
        }

      
        public void IncreaseCost(int amnt)
        {
            if (cost == byte.MaxValue) { return; }
            if (amnt + cost >= 255) { cost = byte.MaxValue; }
            else { cost += (byte)amnt; }
        }
    }
}