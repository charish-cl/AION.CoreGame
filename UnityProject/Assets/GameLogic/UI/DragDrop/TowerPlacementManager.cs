using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 塔放置管理器 - 统一管理点击和拖拽两种放置模式，统一管理所有Action回调
    /// </summary>
    public class TowerPlacementManager : MonoBehaviour
    {
        [Header("放置模式")]
        [Tooltip("放置模式：点击模式、拖拽模式或两种模式都支持")]
        public PlacementMode placementMode = PlacementMode.Both;
        
        [Header("引用")]
        [Tooltip("网格系统")]
        public TowerDefenseGridSystem gridSystem;
        
        [Tooltip("世界相机")]
        public Camera worldCamera;
        
        [Tooltip("视图层（统一管理渲染和UI）")]
        public GridDragView gridDragView;
        
        [Header("当前选中的塔（点击模式）")]
        [Tooltip("当前选中的塔ID（用于放置）")]
        public int selectedTowerId = 0;
        
        [Tooltip("当前选中的塔的占用形状（相对于锚点的偏移坐标列表）")]
        public List<Vector2Int> selectedTowerFootprint;
        
        // 拖拽状态
        private Dictionary<int, DragItemInfo> m_dragItems = new Dictionary<int, DragItemInfo>(); // 拖拽项信息（towerId -> info）
        private Vector2Int m_lastGridPos = new Vector2Int(-1, -1); // 上次高亮的网格位置
        
        // 点击模式状态
        private bool m_isPlacingMode = false;
        private Vector2Int m_hoveredGridPos = new Vector2Int(-1, -1);
        
        // ========== 统一的Action回调（拖拽开始、结束、停止、成功、失败） ==========
        public event Action<int> OnDragBegin; // 拖拽开始 (towerId)
        public event Action<int> OnDragEnd; // 拖拽结束 (towerId)
        public event Action<int> OnDragCancel; // 拖拽取消/停止 (towerId)
        public event Action<int, Vector2, GameActor> OnPlaceSuccess; // 放置成功 (towerId, worldPos, tower)
        public event Action<int, Vector2, string> OnPlaceFailed; // 放置失败 (towerId, worldPos, reason)
        
        public enum PlacementMode
        {
            Click,      // 点击模式：点击tower图标后，点击网格放置
            Drag,       // 拖拽模式：拖拽tower图标到网格放置
            Both        // 两种模式都支持
        }
        
        /// <summary>
        /// 拖拽项信息
        /// </summary>
        private class DragItemInfo
        {
            public int towerId;
            public List<Vector2Int> footprint;
            public int towerSize;
            public WorldDragDrop dragDrop;
        }
        
        private void Awake()
        {
            if (gridSystem == null)
            {
                gridSystem = TowerDefenseGridSystem.Instance;
            }
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }
        
        /// <summary>
        /// 注册拖拽项（拖拽模式使用）
        /// </summary>
        public void RegisterDragItem(WorldDragDrop dragDrop, int towerId, List<Vector2Int> footprint = null, int towerSize = 1)
        {
            if (dragDrop == null) return;
            
            // 网格对齐由Manager统一处理（考虑gridOrigin），WorldDragDrop不再处理对齐
            
            // 注册拖拽项信息
            var info = new DragItemInfo
            {
                towerId = towerId,
                footprint = footprint,
                towerSize = towerSize,
                dragDrop = dragDrop
            };
            m_dragItems[towerId] = info;
            
            // 绑定事件
            dragDrop.OnDragBegin += (id) => HandleDragBegin(id);
            dragDrop.OnDragUpdate += (id, worldPos, canPlace) => HandleDragUpdate(id, worldPos, canPlace);
            dragDrop.OnDragEnd += (id, worldPos, isSuccess) => HandleDragEnd(id, worldPos, isSuccess);
            dragDrop.OnDragFailed += (id) => HandleDragCancel(id);
        }
        
        /// <summary>
        /// 处理拖拽开始
        /// </summary>
        private void HandleDragBegin(int dragItemId)
        {
            if (!m_dragItems.ContainsKey(dragItemId)) return;
            
            var info = m_dragItems[dragItemId];
            m_lastGridPos = new Vector2Int(-1, -1);
            
            // 通知视图层
            if (gridDragView != null && info.dragDrop != null)
            {
                Vector2 worldPos = info.dragDrop.GetWorldPosition();
                List<Vector2Int> highlightCells = GetHighlightCells(dragItemId, worldPos);
                gridDragView.OnDragBegin(dragItemId, worldPos, highlightCells);
            }
            
            OnDragBegin?.Invoke(dragItemId);
        }
        
        /// <summary>
        /// 处理拖拽更新
        /// </summary>
        private void HandleDragUpdate(int dragItemId, Vector2 worldPosition, bool canPlace)
        {
            if (!m_dragItems.ContainsKey(dragItemId) || gridSystem == null) return;
            
            var info = m_dragItems[dragItemId];
            
            // 对齐到网格（考虑gridOrigin）
            Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);
            Vector2 alignedWorldPos = gridSystem.GridToWorld(gridPos);
            
            // 检查是否可以放置
            bool canPlaceResult = CheckCanPlace(dragItemId, alignedWorldPos);
            
            // 更新拖拽系统的可放置状态
            info.dragDrop?.SetCanPlace(canPlaceResult);
            
            // 更新高亮显示
            UpdateDragHighlight(dragItemId, alignedWorldPos, canPlaceResult);
        }
        
        /// <summary>
        /// 处理拖拽结束
        /// </summary>
        private void HandleDragEnd(int dragItemId, Vector2 worldPosition, bool isSuccess)
        {
            if (!m_dragItems.ContainsKey(dragItemId)) return;
            
            var info = m_dragItems[dragItemId];
            
            // 清除高亮
            ClearDragHighlight(dragItemId);
            m_lastGridPos = new Vector2Int(-1, -1);
            
            // 如果成功，先执行放置逻辑（更新网格状态）
            if (isSuccess)
            {
                TryPlaceTower(dragItemId, worldPosition, info);
            }
            
            // 然后通知视图层（放置完成后，网格状态已更新，再隐藏网格）
            if (gridDragView != null)
            {
                gridDragView.OnDragEnd(dragItemId, worldPosition, isSuccess);
            }
            
            OnDragEnd?.Invoke(dragItemId);
        }
        
        /// <summary>
        /// 处理拖拽取消
        /// </summary>
        private void HandleDragCancel(int dragItemId)
        {
            ClearDragHighlight(dragItemId);
            OnDragCancel?.Invoke(dragItemId);
        }
        
        /// <summary>
        /// 尝试放置塔（拖拽模式）
        /// </summary>
        private void TryPlaceTower(int towerId, Vector2 worldPosition, DragItemInfo info)
        {
            if (gridSystem == null) return;
            
            // 对齐到网格
            Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);
            Vector2 alignedWorldPos = gridSystem.GridToWorld(gridPos);
            
            // 检查是否可以放置
            if (!CheckCanPlace(towerId, alignedWorldPos))
            {
                OnPlaceFailed?.Invoke(towerId, alignedWorldPos, "位置不可放置");
                return;
            }
            
            // 创建塔
            GameActor tower = TowerCreator.CreateTower(towerId, alignedWorldPos, gridSystem);
            if (tower == null)
            {
                OnPlaceFailed?.Invoke(towerId, alignedWorldPos, "创建塔失败");
                return;
            }
            
            // 放置到网格
            bool placed = false;
            if (info.footprint != null && info.footprint.Count > 0)
            {
                placed = gridSystem.PlaceTower(gridPos, tower, info.footprint);
            }
            else
            {
                placed = gridSystem.PlaceTower(gridPos, tower, info.towerSize);
            }
            
            if (!placed)
            {
                OnPlaceFailed?.Invoke(towerId, alignedWorldPos, "网格系统放置失败");
                return;
            }
            
            // 成功
            OnPlaceSuccess?.Invoke(towerId, alignedWorldPos, tower);
        }
        
        /// <summary>
        /// 检查是否可以放置
        /// </summary>
        private bool CheckCanPlace(int towerId, Vector2 worldPosition)
        {
            if (gridSystem == null) return true;
            
            Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);
            
            // 拖拽模式：从注册信息获取footprint
            if (m_dragItems.ContainsKey(towerId))
            {
                var info = m_dragItems[towerId];
                if (info.footprint != null && info.footprint.Count > 0)
                {
                    return gridSystem.CanPlaceTower(gridPos, info.footprint);
                }
                else
                {
                    return gridSystem.CanPlaceTower(gridPos, info.towerSize);
                }
            }
            
            // 点击模式：使用selectedTowerFootprint
            if (selectedTowerFootprint != null && selectedTowerFootprint.Count > 0)
            {
                return gridSystem.CanPlaceTower(gridPos, selectedTowerFootprint);
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取高亮网格列表
        /// </summary>
        private List<Vector2Int> GetHighlightCells(int towerId, Vector2 worldPosition)
        {
            if (gridSystem == null) return null;
            
            Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);
            List<Vector2Int> footprint = null;
            
            // 拖拽模式：从注册信息获取
            if (m_dragItems.ContainsKey(towerId))
            {
                var info = m_dragItems[towerId];
                if (info.footprint != null && info.footprint.Count > 0)
                {
                    footprint = gridSystem.GetTowerFootprint(gridPos, info.footprint);
                }
                else
                {
                    int halfSize = info.towerSize / 2;
                    footprint = new List<Vector2Int>();
                    for (int x = gridPos.x - halfSize; x <= gridPos.x + halfSize; x++)
                    {
                        for (int y = gridPos.y - halfSize; y <= gridPos.y + halfSize; y++)
                        {
                            footprint.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            // 点击模式：使用selectedTowerFootprint
            else if (selectedTowerFootprint != null && selectedTowerFootprint.Count > 0)
            {
                footprint = gridSystem.GetTowerFootprint(gridPos, selectedTowerFootprint);
            }
            
            return footprint;
        }
        
        /// <summary>
        /// 更新拖拽高亮
        /// </summary>
        private void UpdateDragHighlight(int towerId, Vector2 worldPosition, bool canPlace)
        {
            if (gridSystem == null || gridDragView == null) return;
            
            Vector2Int gridPos = gridSystem.WorldToGrid(worldPosition);
            if (gridPos != m_lastGridPos)
            {
                m_lastGridPos = gridPos;
            }
            
            List<Vector2Int> highlightCells = GetHighlightCells(towerId, worldPosition);
            if (canPlace && highlightCells != null)
            {
                gridDragView.OnDragUpdate(towerId, worldPosition, highlightCells);
            }
            else
            {
                gridDragView.OnDragUpdate(towerId, worldPosition, null);
            }
        }
        
        /// <summary>
        /// 清除拖拽高亮
        /// </summary>
        private void ClearDragHighlight(int towerId)
        {
            if (gridDragView != null)
            {
                gridDragView.OnDragUpdate(towerId, Vector2.zero, null);
            }
        }
        
        // ========== 点击模式 ==========
        
        private void Update()
        {
            if (m_isPlacingMode && (placementMode == PlacementMode.Click || placementMode == PlacementMode.Both))
            {
                // 检查是否正在拖拽
                bool isDragging = IsAnyDragActive();
                if (!isDragging)
                {
                    UpdateClickModeHighlight();
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                        {
                            TryPlaceTowerClick();
                        }
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelPlacement();
                }
            }
        }
        
        /// <summary>
        /// 检查是否有拖拽正在进行
        /// </summary>
        private bool IsAnyDragActive()
        {
            WorldDragDrop[] dragDrops = FindObjectsOfType<WorldDragDrop>();
            foreach (var dragDrop in dragDrops)
            {
                if (dragDrop.IsDragging) return true;
            }
            return false;
        }
        
        /// <summary>
        /// 更新点击模式的高亮
        /// </summary>
        private void UpdateClickModeHighlight()
        {
            if (gridSystem == null || selectedTowerFootprint == null || IsAnyDragActive()) return;
            
            Vector2Int currentGridPos = gridSystem.GetMouseGridPosition(worldCamera);
            if (currentGridPos != m_hoveredGridPos)
            {
                m_hoveredGridPos = currentGridPos;
                bool canPlace = gridSystem.CanPlaceTower(currentGridPos, selectedTowerFootprint);
                // 高亮显示通过GridRenderer实现（如果需要）
            }
        }
        
        /// <summary>
        /// 尝试放置塔（点击模式）
        /// </summary>
        private void TryPlaceTowerClick()
        {
            if (selectedTowerId == 0 || selectedTowerFootprint == null || gridSystem == null) return;
            
            Vector2Int gridPos = gridSystem.GetMouseGridPosition(worldCamera);
            if (!gridSystem.CanPlaceTower(gridPos, selectedTowerFootprint))
            {
                Vector2 worldPos = gridSystem.GridToWorld(gridPos);
                OnPlaceFailed?.Invoke(selectedTowerId, worldPos, "位置不可放置");
                return;
            }
            
            Vector2 worldPos2 = gridSystem.GridToWorld(gridPos);
            GameActor tower = TowerCreator.CreateTower(selectedTowerId, worldPos2, gridSystem);
            if (tower == null)
            {
                OnPlaceFailed?.Invoke(selectedTowerId, worldPos2, "创建塔失败");
                return;
            }
            
            if (!gridSystem.PlaceTower(gridPos, tower, selectedTowerFootprint))
            {
                OnPlaceFailed?.Invoke(selectedTowerId, worldPos2, "网格系统放置失败");
                return;
            }
            
            OnPlaceSuccess?.Invoke(selectedTowerId, worldPos2, tower);
        }
        
        /// <summary>
        /// 取消放置（点击模式）
        /// </summary>
        public void CancelPlacement()
        {
            m_isPlacingMode = false;
            selectedTowerId = 0;
            selectedTowerFootprint = null;
        }
    }
}
