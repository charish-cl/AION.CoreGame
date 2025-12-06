using System;
using System.Collections.Generic;
using UnityEngine;
using GameDevKit;
using AION.CoreFramework;
using Sirenix.OdinInspector;

namespace GameLogic
{
    /// <summary>
    /// 塔防网格系统 - 核心网格逻辑，用于管理塔的放置、碰撞检测
    /// </summary>
    public class TowerDefenseGridSystem : MonoBehaviour
    {
        [Header("网格设置")]
        [Tooltip("网格单元大小")]
        public Vector2 cellSize = new Vector2(1f, 1f);
        
        [Tooltip("网格原点（世界坐标）")]
        public Vector2 gridOrigin = Vector2.zero;
        
        [Tooltip("网格尺寸（单元数量）")]
        public Vector2Int gridSize = new Vector2Int(50, 50);
        
        [Header("调试工具")]
        [Tooltip("用于计算网格尺寸的SpriteRenderer（以左下角为原点）")]
        public SpriteRenderer debugSpriteRenderer;
        
        // 网格数据
        private Grid<GridCell> m_grid;
        
        // 当前选中的塔（用于显示攻击范围）
        private GameActor m_selectedTower;
        

        public event Action OnGridChanged; // 网格状态改变事件（塔放置/移除时触发）
        
        // 单例
        private static TowerDefenseGridSystem s_instance;
        public static TowerDefenseGridSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<TowerDefenseGridSystem>();
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
            public bool isPlaceable{get; set; } // 是否可以放置
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
        /// 初始化网格系统（必须手动调用，不在Awake/Start中自动初始化）
        /// </summary>
        public void Initialize()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }
            else if (s_instance != this)
            {
                Log.Warning("TowerDefenseGridSystem: 已存在实例，跳过初始化");
                return;
            }
            
            InitializeGrid();
            
        }
        
        /// <summary>
        /// 初始化网格
        /// </summary>
        private void InitializeGrid()
        {
            m_grid = new Grid<GridCell>(
                (grid, coord) => new GridCell(coord),
                gridSize,
                cellSize,
                gridOrigin,
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
            
            return m_grid.GetCellWorldPosition(gridPos.x, gridPos.y) + cellSize * 0.5f;
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
        
        #endregion
  
        
        #region 调试工具方法
        
        /// <summary>
        /// 根据SpriteRenderer自动计算网格尺寸（以Sprite左下角为原点）
        /// 在Inspector中点击按钮即可使用
        /// </summary>
        [Button("根据SpriteRenderer计算网格尺寸")]
        private void CalculateGridFromSpriteRenderer()
        {
            if (debugSpriteRenderer == null)
            {
                Log.Warning("TowerDefenseGridSystem.CalculateGridFromSpriteRenderer: debugSpriteRenderer为空，请先设置SpriteRenderer");
                return;
            }
            
            if (debugSpriteRenderer.sprite == null)
            {
                Log.Warning("TowerDefenseGridSystem.CalculateGridFromSpriteRenderer: SpriteRenderer的Sprite为空");
                return;
            }
            
            // 获取Sprite的世界尺寸
            Vector2 spriteSize = debugSpriteRenderer.bounds.size;
            
            // 计算Sprite左下角的世界坐标
            Vector3 spritePos = debugSpriteRenderer.transform.position;
            Vector2 spriteBottomLeft = new Vector2(
                spritePos.x - spriteSize.x * 0.5f,
                spritePos.y - spriteSize.y * 0.5f
            );
            
            // 计算水平和垂直的cell数量
            int width = Mathf.CeilToInt(spriteSize.x / cellSize.x);
            int height = Mathf.CeilToInt(spriteSize.y / cellSize.y);
            
            Vector2Int calculatedSize = new Vector2Int(width, height);
            
            // 更新gridOrigin（以Sprite左下角为原点）
            gridOrigin = spriteBottomLeft;
            
            // 更新gridSize
            this.gridSize = calculatedSize;
            
            // 重新初始化网格
            InitializeGrid();
            
            Log.Info($"TowerDefenseGridSystem.CalculateGridFromSpriteRenderer: " +
                     $"Sprite尺寸={spriteSize}, Cell大小={cellSize}, " +
                     $"计算出的网格尺寸={calculatedSize}, 网格原点={gridOrigin}");
        }
        
        #endregion
    }
}
