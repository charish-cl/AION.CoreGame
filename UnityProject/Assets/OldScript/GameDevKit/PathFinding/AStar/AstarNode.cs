using System;
using UnityEngine;

namespace GameDevKit
{
    
    public class AstarNode
    {
        private Grid<AstarNode> _grid;
        //g,h,f,的花费
        public int gcost;
        public int hcost;
        public int fcost;

        //x,y坐标
        public Vector2Int coordinate;

        //上一个节点
        public AstarNode fromNode;
        public bool isWalkable;
        public int x;
        public int y;

        public AstarNode(Grid<AstarNode> grid, int x, int y) {
            this._grid = grid;
            coordinate = new Vector2Int(x, y);
            this.x = x;
            this.y = y;
            isWalkable = true;
            sprite = WordUtil.CreateWorldSprite(ToString(), 
                grid.GetCellPosition(coordinate),
                grid.cellsize-new Vector3(1,1,0)*0.5f,
                Color.grey);
        }
        public GameObject sprite;
        public void SetColor(Color color)
        {
            sprite.SetColor(color);    
            _grid.RefreshGridObject(x,y);
        }
        public void CalCulateCost()
        {
            fcost = gcost + hcost;
            _grid.RefreshGridObject(x,y);
        }
        public override string ToString()
        {
            if (gcost == Int32.MaxValue)
            {
                return "";
            }
            return "g:"+gcost+"\n"+"f:"+fcost;
        }
    }
}