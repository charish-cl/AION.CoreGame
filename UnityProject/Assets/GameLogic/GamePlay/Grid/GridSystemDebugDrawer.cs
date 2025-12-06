using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 网格系统调试绘制器 - 用于在Scene视图中绘制网格线
    /// </summary>
    [RequireComponent(typeof(TowerDefenseGridSystem))]
    public class GridSystemDebugDrawer : MonoBehaviour
    {
        [Header("调试绘制设置")]
        [Tooltip("是否显示网格线")]
        public bool showGridLines = true;
        
        [Tooltip("是否显示网格区域（矩形）")]
        public bool showGridAreas = true;
        
        [Tooltip("是否只显示镜头内的网格（性能优化）")]
        public bool showOnlyVisibleGrid = true;
        
        [Tooltip("网格线颜色")]
        public Color gridLineColor = new Color(1f, 1f, 1f, 0.3f);
        
        [Tooltip("可放置区域颜色")]
        public Color validPlaceColor = new Color(0f, 1f, 0f, 0.3f);
        
        [Tooltip("不可放置区域颜色")]
        public Color invalidPlaceColor = new Color(1f, 0f, 0f, 0.3f);
        
        [Tooltip("已占用区域颜色")]
        public Color occupiedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        private TowerDefenseGridSystem m_gridSystem;
        private Vector2Int m_hoveredCell = new Vector2Int(-1, -1);
        
        private void Awake()
        {
            m_gridSystem = GetComponent<TowerDefenseGridSystem>();
            if (m_gridSystem == null)
            {
                Log.Warning("GridSystemDebugDrawer: 未找到TowerDefenseGridSystem组件");
            }
        }
        
        private void Update()
        {
            // 更新悬停的网格单元
            if (m_gridSystem != null)
            {
                Vector2Int currentHover = m_gridSystem.GetMouseGridPosition();
                if (currentHover != m_hoveredCell)
                {
                    m_hoveredCell = currentHover;
                }
            }
        }
        
        /// <summary>
        /// 检查网格位置是否在镜头内
        /// </summary>
        private bool IsGridVisible(Vector2 worldPos, Camera camera = null)
        {
            if (!showOnlyVisibleGrid) return true;
            if (camera == null) camera = Camera.main;
            if (camera == null) return true;
            
            Vector3 viewportPos = camera.WorldToViewportPoint(new Vector3(worldPos.x, worldPos.y, 0));
            return viewportPos.x >= -0.1f && viewportPos.x <= 1.1f && 
                   viewportPos.y >= -0.1f && viewportPos.y <= 1.1f;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGridLines || m_gridSystem == null) return;
            
            Camera camera = Camera.main;
            if (camera == null) camera = FindObjectOfType<Camera>();
            
            Vector2Int gridSize = m_gridSystem.gridSize;
            Vector2 cellSize = m_gridSystem.cellSize;
            Vector2 gridOrigin = m_gridSystem.gridOrigin;
            
            // 计算可见区域
            Vector2Int minVisible = Vector2Int.zero;
            Vector2Int maxVisible = gridSize;
            
            if (showOnlyVisibleGrid && camera != null)
            {
                // 获取镜头边界
                Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
                Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
                
                Vector2Int minGrid = m_gridSystem.WorldToGrid(new Vector2(bottomLeft.x, bottomLeft.y));
                Vector2Int maxGrid = m_gridSystem.WorldToGrid(new Vector2(topRight.x, topRight.y));
                
                minVisible = new Vector2Int(
                    Mathf.Max(0, minGrid.x - 1),
                    Mathf.Max(0, minGrid.y - 1)
                );
                maxVisible = new Vector2Int(
                    Mathf.Min(gridSize.x, maxGrid.x + 1),
                    Mathf.Min(gridSize.y, maxGrid.y + 1)
                );
            }
            
            // 绘制网格区域（矩形）
            if (showGridAreas)
            {
                for (int x = minVisible.x; x < maxVisible.x; x++)
                {
                    for (int y = minVisible.y; y < maxVisible.y; y++)
                    {
                        var cell = m_gridSystem.GetCellAt(new Vector2Int(x, y));
                        if (cell == null) continue;
                        
                        // 计算cell左下角的世界坐标（与Shader保持一致）
                        Vector2 cellBottomLeft = gridOrigin + new Vector2(x * cellSize.x, y * cellSize.y);
                        // cell中心点（用于DrawCube）
                        Vector2 cellCenter = cellBottomLeft + cellSize * 0.5f;
                        
                        // 根据状态选择颜色
                        Color areaColor;
                        if (cell.isOccupied)
                        {
                            areaColor = occupiedColor;
                        }
                        else if (!cell.isPlaceable)
                        {
                            areaColor = invalidPlaceColor; // 红色 - 不可放置
                        }
                        else
                        {
                            areaColor = validPlaceColor; // 绿色 - 可放置
                        }
                        
                        // 绘制矩形区域（使用cell中心点）
                        Gizmos.color = areaColor;
                        Gizmos.DrawCube(cellCenter, cellSize);
                    }
                }
            }
            
            // 绘制网格线（只绘制可见区域）
            if (showGridLines)
            {
                Gizmos.color = gridLineColor;
                for (int x = minVisible.x; x <= maxVisible.x; x++)
                {
                    Vector2 start = gridOrigin + new Vector2(x * cellSize.x, minVisible.y * cellSize.y);
                    Vector2 end = gridOrigin + new Vector2(x * cellSize.x, maxVisible.y * cellSize.y);
                    if (IsGridVisible(start, camera) || IsGridVisible(end, camera))
                    {
                        Gizmos.DrawLine(start, end);
                    }
                }
                
                for (int y = minVisible.y; y <= maxVisible.y; y++)
                {
                    Vector2 start = gridOrigin + new Vector2(minVisible.x * cellSize.x, y * cellSize.y);
                    Vector2 end = gridOrigin + new Vector2(maxVisible.x * cellSize.x, y * cellSize.y);
                    if (IsGridVisible(start, camera) || IsGridVisible(end, camera))
                    {
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
            
            // 绘制悬停的单元（高亮边框）
            if (m_hoveredCell.x >= 0 && m_hoveredCell.y >= 0)
            {
                // 计算cell左下角的世界坐标（与Shader保持一致）
                Vector2 cellBottomLeft = gridOrigin + new Vector2(m_hoveredCell.x * cellSize.x, m_hoveredCell.y * cellSize.y);
                // cell中心点（用于DrawWireCube）
                Vector2 cellCenter = cellBottomLeft + cellSize * 0.5f;
                
                var cell = m_gridSystem.GetCellAt(m_hoveredCell);
                if (cell != null)
                {
                    // 绘制高亮边框
                    Gizmos.color = cell.isPlaceable && !cell.isOccupied ? validPlaceColor : invalidPlaceColor;
                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f); // 更不透明
                    
                    // 绘制边框（使用DrawWireCube）
                    Gizmos.DrawWireCube(cellCenter, cellSize);
                }
            }
        }
    }
}

