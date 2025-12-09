using System;
using System.Collections.Generic;
using UnityEngine;
using GameDevKit;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 网格辅助类 - 单例Helper类，用于管理网格的放置、碰撞检测
    /// </summary>
    public class GridHelper
    {
        // 网格数据
        private Grid<GridCell> m_grid;
        private GridSetting m_setting;
        
        // 当前选中的塔（用于显示攻击范围）
        private GameActor m_selectedTower;

        public event Action OnGridChanged; // 网格状态改变事件（塔放置/移除时触发）
        
        // 单例
        private static GridHelper s_instance;
        public static GridHelper Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new GridHelper();
                }
                return s_instance;
            }
        }
        
        /// <summary>
        /// 网格单元数据
        /// </summary>
        public class GridCell
        {
            public Vector2Int coordinate;
            public bool isOccupied; // 是否被占用（有塔）
            public bool isPlaceable { get; set; } // 是否可以放置
            public GameActor tower; // 放置的塔（如果有）
            
            public GridCell(Vector2Int coord)
            {
                coordinate = coord;
                isOccupied = false;
                isPlaceable = true;
                tower = null;
            }
        }
        
        /// <summary>
        /// 初始化网格系统
        /// </summary>
        public void Initialize(GridSetting setting = null)
        {
            m_setting = setting ?? LS.Get<GridSetting>();
            
            if (m_setting == null)
            {
                Log.Warning("GridHelper: GridSetting 未找到，使用默认配置");
                m_setting = ScriptableObject.CreateInstance<GridSetting>();
            }
            
            InitializeGrid();
        }
        
        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public bool IsInitialized => m_grid != null;
        
        /// <summary>
        /// 初始化网格
        /// </summary>
        private void InitializeGrid()
        {
            m_grid = new Grid<GridCell>(
                (grid, coord) => new GridCell(coord),
                m_setting.gridSize,
                m_setting.cellSize,
                m_setting.gridOrigin,
                false
            );
        }
        
        #region 坐标转换
        
        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            if (m_grid == null) return Vector2Int.zero;
            
            var (x, y) = m_grid.GetGridCoordinates(worldPos);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// 网格坐标转世界坐标（返回单元中心点）
        /// </summary>
        public Vector2 GridToWorld(Vector2Int gridPos)
        {
            if (m_grid == null) return Vector2.zero;
            
            return m_grid.GetCellWorldPosition(gridPos.x, gridPos.y) + m_setting.cellSize * 0.5f;
        }
        
        /// <summary>
        /// 获取鼠标位置的网格坐标
        /// </summary>
        public Vector2Int GetMouseGridPosition(Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return Vector2Int.zero;
            
            Vector3 mouseWorldPos = camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 2D游戏，z设为0
            
            return WorldToGrid(mouseWorldPos);
        }
        
        /// <summary>
        /// 获取鼠标位置的世界坐标（对齐到网格）
        /// </summary>
        public Vector2 GetMouseWorldPositionAligned(Camera camera = null)
        {
            Vector2Int gridPos = GetMouseGridPosition(camera);
            return GridToWorld(gridPos);
        }
        
        #endregion
        
        #region 放置检测
        
        /// <summary>
        /// 检查位置是否可以放置塔（使用不规则形状）
        /// </summary>
        public bool CanPlaceTower(Vector2Int gridPos, List<Vector2Int> towerFootprint)
        {
            if (m_grid == null || towerFootprint == null || towerFootprint.Count == 0) return false;
            
            // 检查所有需要的单元是否可用
            foreach (var offset in towerFootprint)
            {
                Vector2Int cellPos = gridPos + offset;
                var cell = m_grid.GetValue(cellPos.x, cellPos.y);
                if (cell == null || !cell.isPlaceable || cell.isOccupied)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查位置是否可以放置塔（使用矩形大小，兼容旧代码）
        /// </summary>
        public bool CanPlaceTower(Vector2 worldPos, int towerSize = 1)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            return CanPlaceTower(gridPos, towerSize);
        }
        
        /// <summary>
        /// 检查网格位置是否可以放置塔（使用矩形大小）
        /// </summary>
        public bool CanPlaceTower(Vector2Int gridPos, int towerSize = 1)
        {
            if (m_grid == null) return false;
            
            // 检查所有需要的单元是否可用
            int halfSize = towerSize / 2;
            for (int x = gridPos.x - halfSize; x <= gridPos.x + halfSize; x++)
            {
                for (int y = gridPos.y - halfSize; y <= gridPos.y + halfSize; y++)
                {
                    var cell = m_grid.GetValue(x, y);
                    if (cell == null || !cell.isPlaceable || cell.isOccupied)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region 放置/移除塔
        
        /// <summary>
        /// 在网格位置放置塔（使用不规则形状）
        /// </summary>
        public bool PlaceTower(Vector2Int gridPos, GameActor tower, List<Vector2Int> towerFootprint)
        {
            if (!CanPlaceTower(gridPos, towerFootprint))
            {
                return false;
            }
            
            // 将塔对齐到网格中心
            Vector2 worldPos = GridToWorld(gridPos);
            tower.SetPosition(worldPos);
            
            // 标记占用的单元
            foreach (var offset in towerFootprint)
            {
                Vector2Int cellPos = gridPos + offset;
                var cell = m_grid.GetValue(cellPos.x, cellPos.y);
                if (cell != null)
                {
                    cell.isOccupied = true;
                    cell.isPlaceable = false; // 放置后标记为不可放置
                    cell.tower = tower;
                }
            }
            
            // 通知高亮器更新显示
            NotifyGridChanged();
            
            return true;
        }
        
        /// <summary>
        /// 在网格位置放置塔（使用矩形大小，兼容旧代码）
        /// </summary>
        public bool PlaceTower(Vector2Int gridPos, GameActor tower, int towerSize = 1)
        {
            if (!CanPlaceTower(gridPos, towerSize))
            {
                return false;
            }
            
            // 将塔对齐到网格中心
            Vector2 worldPos = GridToWorld(gridPos);
            tower.SetPosition(worldPos);
            
            // 标记占用的单元
            int halfSize = towerSize / 2;
            for (int x = gridPos.x - halfSize; x <= gridPos.x + halfSize; x++)
            {
                for (int y = gridPos.y - halfSize; y <= gridPos.y + halfSize; y++)
                {
                    var cell = m_grid.GetValue(x, y);
                    if (cell != null)
                    {
                        cell.isOccupied = true;
                        cell.isPlaceable = false; // 放置后标记为不可放置
                        cell.tower = tower;
                    }
                }
            }
            
            // 通知高亮器更新显示
            NotifyGridChanged();
            
            return true;
        }
        
        /// <summary>
        /// 通知网格状态改变（用于更新高亮显示）
        /// </summary>
        private void NotifyGridChanged()
        {
            OnGridChanged?.Invoke();
        }
        
        #endregion
        
        #region 区域管理
       
        /// <summary>
        /// 设置区域是否可放置
        /// </summary>
        public void SetPlaceable(Vector2Int gridPos, bool placeable)
        {
            var cell = m_grid?.GetValue(gridPos.x, gridPos.y);
            if (cell != null)
            {
                cell.isPlaceable = placeable;
            }
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取网格单元
        /// </summary>
        public GridCell GetCellAt(Vector2Int gridPos)
        {
            return m_grid?.GetValue(gridPos.x, gridPos.y);
        }
        
        /// <summary>
        /// 获取指定位置的塔
        /// </summary>
        public GameActor GetTowerAt(Vector2 worldPos)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            var cell = m_grid?.GetValue(gridPos.x, gridPos.y);
            return cell?.tower;
        }
        
        /// <summary>
        /// 获取塔的占用形状（用于高亮显示）
        /// </summary>
        public List<Vector2Int> GetTowerFootprint(Vector2Int gridPos, List<Vector2Int> towerFootprint)
        {
            if (towerFootprint == null || towerFootprint.Count == 0) return null;
            
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (var offset in towerFootprint)
            {
                result.Add(gridPos + offset);
            }
            return result;
        }
        
        /// <summary>
        /// 获取网格设置
        /// </summary>
        public GridSetting GetSetting()
        {
            return m_setting;
        }
        
        #endregion
    }
}

