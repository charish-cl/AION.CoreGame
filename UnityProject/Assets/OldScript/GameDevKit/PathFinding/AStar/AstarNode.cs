using System;
using UnityEngine;

namespace GameDevKit
{
    
    public class AstarNode
    {
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
        public AstarNode(int x, int y) {
            coordinate = new Vector2Int(x, y);
            this.x = x;
            this.y = y;
            isWalkable = true;
           
        }
        public void CalCulateCost()
        {
            fcost = gcost + hcost;
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