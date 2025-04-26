using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public class Grid<T> : IEnumerable<T> 
    {
        private T[,] gridData;
        private int width;
        private int height;
        private Vector2 cellSize;
        private Vector2 gridOrigin;

        // Debug settings
        private bool enableDebug;
        private TextMesh[,] debugTexts;
        private const int DefaultFontSize = 15;
        private readonly Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        public event Action<int, int, T> OnCellValueChanged;

        public Vector2 CellSize => cellSize;
        public Vector2 GridOrigin => gridOrigin;
        public int Width => width;
        public int Height => height;
        /// <summary>
        /// 基础构造函数
        /// </summary>
        public Grid(T[,] initialData, Vector2 cellSize, Vector2 origin, bool enableDebug = false)
        {
            this.gridData = initialData;
            //获取横坐标象限长度
            this.width = initialData.GetLength(0);
            this.height = initialData.GetLength(1);
            this.cellSize = cellSize;
            this.gridOrigin = origin;
            this.enableDebug = enableDebug;

            InitializeDebugSystem();
        }

        public Grid(Func<Grid<T>, Vector2Int, T> getData,Vector2Int cellNum, Vector2 cellSize, Vector2 origin, bool enableDebug = false)
        {
            this.width = cellNum.x;
            this.height = cellNum.y;
            this.cellSize = cellSize;
            this.gridOrigin = origin;
            this.enableDebug = enableDebug;

            InitializeGridData(getData);
            InitializeDebugSystem();
        }

        //调使用
        public Grid(Func<Grid<T>, Vector2Int, T> getData,int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cellSize = new Vector2(5,5);
            this.gridOrigin = Vector2.zero-new Vector2(this.width/2*cellSize.x,this.height/2*cellSize.y);
            this.enableDebug = true;
            InitializeGridData(getData);
            InitializeDebugSystem();
        }
        //自动初始化默认的Grid
        private void InitializeGridData(Func<Grid<T>, Vector2Int, T> getData)
        {
            gridData = new T[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridData[x, y] = getData(this,new Vector2Int(x, y));
                }
            }
        }
        private void InitializeDebugSystem()
        {
            if (!enableDebug) return;

            debugTexts = new TextMesh[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 创建调试文本
                    Vector2 textPosition = GetCellWorldPosition(x, y);
                    // debugTexts[x, y] = DebugWordUtil.CreateWorldText("", 
                    //     localPosition:textPosition);

                    // 绘制单元格边框
                    DrawCellBorder(x, y);
                }
            }

            OnCellValueChanged += HandleCellValueChanged;
        }

        private void HandleCellValueChanged(int x, int y, T value)
        {
            if (enableDebug && debugTexts[x, y] != null)
                debugTexts[x, y].text = value?.ToString();
        }

        private void DrawCellBorder(int x, int y)
        {
            Vector2 cellStart = GetCellWorldPosition(x, y);
            //一个单元格的单位长度
            Vector2 cellEnd = GetCellWorldPosition(x + 1, y + 1);
            FDraw.DebugDrawRectangle(cellStart, cellEnd, gridColor, 100f);
        }

        #region Get相关

        /// <summary>
        /// 获取Cell的世界坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector2 GetCellWorldPosition(int x, int y)
        {
            return new Vector2(x * cellSize.x, y * cellSize.y) + gridOrigin;
        }

        public Vector2 GetCellWorldPosition(Vector2Int coordinate)
        {
            return GetCellWorldPosition(coordinate.x, coordinate.y);
        }

        public (int x, int y) GetGridCoordinates(Vector2 worldPosition)
        {
            Vector2 localPos = worldPosition - gridOrigin;
            var (x,y) = (
                Mathf.FloorToInt(localPos.x / cellSize.x),
                Mathf.FloorToInt(localPos.y / cellSize.y)
            );
            return (x,y);
        }

        public T GetValue(int x, int y)
        {
            return IsValidCoordinate(x, y) ? gridData[x, y] : default;
        }
        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }


        /// <summary>
        /// 获取矩形可放置区域
        /// </summary>
        /// <param name="xNum"></param>
        /// <param name="yNum"></param>
        /// <returns></returns>
        public List<T> GetBoundCell(Vector2Int point,int xNum, int yNum)
        {
            if (!IsValidCoordinate(point.x, point.y))
            {
                return null;
            }
            List<T> boundCell = new List<T>();
            //选择起点
            int startX = point.x - xNum / 2;
            int startY = point.y - yNum / 2;
            for (int i = 0; i < xNum; i++)
            {
                for (int j = 0; j < yNum; j++)
                {
                    var cell = GetValue(startX + i, startY + j);
                    if (cell != null)
                    {
                        boundCell.Add(cell);
                    }
                }
            }
            return boundCell;
        }

        #endregion
       

        
        
        public void SetValue(int x, int y, T value)
        {
            if (!IsValidCoordinate(x, y)) return;
            
            gridData[x, y] = value;
            OnCellValueChanged?.Invoke(x, y, value);
        }

        public void RefreshAllValues()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    OnCellValueChanged?.Invoke(x, y, gridData[x, y]);
        }

 
        // 实现枚举器接口
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in gridData)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // 新增实用方法
        public T GetValueByWorldPosition(Vector2 worldPosition)
        {
            var (x, y) = GetGridCoordinates(worldPosition);
            Debug.Log($"worldPosition :{worldPosition}, x:{x}, y:{y}");
            return GetValue(x, y);
        }

        public T GetByMousePosition()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector2( Input.mousePosition.x, Input.mousePosition.y));
            var cell = GetValueByWorldPosition(worldPos);
            return cell;
        }

        public IEnumerable<T> GetNeighbors(Vector2Int position, bool includeDiagonal = false)
        {
            int x = position.x;
            int y = position.y;
            return GetNeighbors(x, y, includeDiagonal);
        }
        public IEnumerable<T> GetNeighbors(int x, int y, bool includeDiagonal = false)
        {
            List<T> neighbors = new List<T>();
            Vector2Int[] directions = includeDiagonal ? 
                GridDirections.AllDirections : 
                GridDirections.CardinalDirections;

            foreach (var dir in directions)
            {
                int newX = x + dir.x;
                int newY = y + dir.y;
                if (IsValidCoordinate(newX, newY))
                    neighbors.Add(gridData[newX, newY]);
            }
            return neighbors;
        }
    }

    public static class GridDirections
    {
        public static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public static readonly Vector2Int[] AllDirections =
        {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.right + Vector2Int.down,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.left + Vector2Int.up
        };
    }
}