using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
//参考CodeMonkey
namespace GameDevKit
{
    public class Grid<T> : IEnumerable<T>
    {
        private T[,] cells;

        private int xNum;
        private int yNum;

        public Vector3 cellsize;
        public int fontSize=15;
        private Color defaultColdor= Color.grey;
        private Vector3 gridPosition =Vector2.zero;
        private IEnumerable<T> _enumerableImplementation;

        public delegate void OnItemChange(int x, int y, T cell);

        public event OnItemChange EventItemChange;
        
        
        public T GetGridObject(int x, int y) {
            if (x >= 0 && y >= 0 && x < xNum && y < yNum) {
                return cells[x, y];
            } else
            {
                return default(T);
            }
        }
        
        
      
        /// <summary>
        /// x,y这里是坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void SetGridObject(int x, int y, T value) {
            if (x >= 0 && y >= 0 && x < xNum && y < yNum) {
                cells[x, y] = value;
                EventItemChange?.Invoke(x,y,cells[x,y]);
            }
        }
        public void RefreshGridObject(int x,int y) {
            EventItemChange?.Invoke(x,y,cells[x,y]);
        }
        /// <summary>
        /// 默认网格元素大小为5,中心为世界远点
        /// </summary>
        /// <param name="xNum"></param>
        /// <param name="yNum"></param>
        /// <param name="createGridObject"></param>
        /// <param name="showdebug"></param>
        public Grid(int xNum, int yNum, Func<Grid<T>, int, int, T> createGridObject, bool showdebug )
        {
            this.xNum = xNum;
            this.yNum = yNum;
            this.cellsize = Vector2.one*5;
            this.gridPosition = -GetCellWorldLeftDownPosition(xNum / 2, yNum / 2);
            CreateGrid(createGridObject,showdebug);
        }
        
        public Grid(int xNum, int yNum, Vector3 cellsize,Vector3 gridPosition,
            Func<Grid<T>,int,int,T> createGridObject,bool showdebug)
        {
            this.xNum = xNum;
            this.yNum = yNum;
            this.cellsize = cellsize;
            this.gridPosition = gridPosition;
            CreateGrid(createGridObject,showdebug);
        }

        private TextMesh[,] debugTextArray;
        void CreateGrid(Func<Grid<T>, int, int, T> createGridObject,bool showdebug)
        {
            cells = new T[xNum,yNum];
            if (createGridObject!=null)
            {
                for (int x = 0; x < cells.GetLength(0); x++) {
                    for (int y = 0; y < cells.GetLength(1); y++) {
                        cells[x, y] = createGridObject(this, x, y);
                    }
                }
            }
            if (showdebug) {
                debugTextArray= new TextMesh[xNum, this.yNum];

                for (int x = 0; x < cells.GetLength(0); x++) {
                    for (int y = 0; y < cells.GetLength(1); y++) {
                        debugTextArray[x, y] = WordUtil.CreateWorldText(cells[x, y]?.ToString(), null, GetCellWorldLeftDownPosition(x, y) + cellsize * .5f, fontSize, Color.white, TextAnchor.MiddleCenter);
                        FDraw.DebugDrawRectangle(GetCellWorldLeftDownPosition(x, y), GetCellWorldLeftDownPosition(x+1,y + 1),defaultColdor, 100f);
                    }
                }
               
                void OnValueCahge(int x, int y, T cell)
                {
                    debugTextArray[x, y].text = cells[x,y]?.ToString();
                }
                
                //刷新UI
                EventItemChange += OnValueCahge;
            }
        }

        public void RefreshRender()
        {
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    debugTextArray[i, j].text  = cells[i,j]?.ToString();
                }
            }
        }
        /// <summary>
        /// 获取元素的左下角世界坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetCellWorldLeftDownPosition(int x, int y) {
            return new Vector3(x*cellsize.x, y*cellsize.y,0) +gridPosition;
        }
        /// <summary>
        /// 获取元素中心坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetCellPosition(int x,int y)
        {
            return GetCellWorldLeftDownPosition(x, y) + cellsize / 2;
        }
        public Vector3 GetCellPosition(Vector2Int vector2Int)
        {
            return GetCellWorldLeftDownPosition(vector2Int.x, vector2Int.y) + cellsize / 2;
        }
        //坐标是否合法
        bool isLegal(int x, int y)
        {
            return x >= 0 && y >= 0 && x < xNum && y < yNum;
        }
        /// <summary>
        /// 从世界坐标获取元素
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public T GetCellFromWordPosition(Vector3 worldPosition) {
            var x = Mathf.FloorToInt((worldPosition - gridPosition).x / cellsize.x);
            var y = Mathf.FloorToInt((worldPosition - gridPosition).y / cellsize.y);
            if (isLegal(x,y))
            {
                return cells[x, y];
            }
            return default(T);
        }
        /// <summary>
        /// 返回的x,y是坐标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public (int x, int y) GetXY(Vector3 worldPosition) {
            var x = Mathf.FloorToInt((worldPosition - gridPosition).x / cellsize.x);
            var y = Mathf.FloorToInt((worldPosition - gridPosition).y / cellsize.y);
            
            return (x,y);
        }

        public T GetCellFromXY(Vector2Int nodeIndex)
        {
            if (isLegal(nodeIndex.x,nodeIndex.y))
            {
                return cells[nodeIndex.x,nodeIndex.y];
            }
            return default(T);
        }


        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < cells.GetLength(0); i++)
            {
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    yield return cells[i, j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void GeCellXY(Vector3 startWorldPosition, out int x, out int y)
        {
            var position = GetXY(startWorldPosition);
            x = position.x;
            y = position.y;
        }





        public T GetMouseTargetCell()
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            var cell = GetCellFromWordPosition(worldMousePos);
            return cell;
        }

        public List<T> GetFourSidesNeighbors(Vector2Int nodeIndex)
        {
           return GetNeighborCells(nodeIndex, GridDirection.quadRantDirections);
        }
        public List<T> GetAllSidesNeighbors(Vector2Int nodeIndex)
        {
            return GetNeighborCells(nodeIndex, GridDirection.AllDirections);
        }
        private List<T> GetNeighborCells(Vector2Int nodeIndex, List<GridDirection> directions)
        {
            List<T> neighborCells = new List<T>();
            
            foreach (Vector2Int curDirection in directions)
            {
                T newNeighbor = GetCellFromXY(nodeIndex+curDirection);
                if (newNeighbor != null)
                {
                    neighborCells.Add(newNeighbor);
                }
            }
            return neighborCells;
        }

    }
}