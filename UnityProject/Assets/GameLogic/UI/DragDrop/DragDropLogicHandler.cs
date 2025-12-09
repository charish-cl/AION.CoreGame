using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLogic
{
    /// <summary>
    /// 拖拽逻辑处理器 - 将Logic层与Action绑定
    /// 职责：只负责业务逻辑（放置、检查等），不关心视图更新
    /// </summary>
    public class DragDropLogicHandler
    {
        private IDragDropView m_view;
        private Dictionary<int, DragItemData> m_dragItems = new Dictionary<int, DragItemData>();
        
        // 逻辑层Action：供外部调用
        public Func<DragItemData, Vector2, List<Vector2Int>> OnCalculateHighlight;
        public Func<DragItemData, Vector2, bool> OnCheckCanPlace;
        public Func<DragItemData, Vector2, GameActor> OnPlaceItem;
        
        /// <summary>
        /// 初始化逻辑处理器
        /// </summary>
        public void Initialize(IDragDropView view)
        {
            m_view = view;
            
            // 绑定逻辑层Action实现
            OnCalculateHighlight = CalculateHighlight;
            OnCheckCanPlace = CheckCanPlace;
            OnPlaceItem = PlaceItem;
        }
        
        /// <summary>
        /// 注册拖拽项
        /// </summary>
        public void RegisterDragItem(DragDropEventHandler handler, DragItemData itemData)
        {
            if (handler == null || itemData == null) return;
            
            m_dragItems[itemData.itemId] = itemData;
            
            // 绑定事件处理器的事件到逻辑处理
            handler.OnBeginDragEvent += HandleDragBegin;
            handler.OnDragEvent += HandleDragUpdate;
            handler.OnEndDragEvent += HandleDragEnd;
        }
        
        private void HandleDragBegin(DragItemData itemData)
        {
            if (itemData == null) return;
            
            Vector2 worldPos = GetCurrentWorldPosition();
            List<Vector2Int> highlightCells = OnCalculateHighlight?.Invoke(itemData, worldPos);
            m_view?.OnDragBegin(itemData.itemId, worldPos, highlightCells);
        }
        
        private void HandleDragUpdate(DragItemData itemData, Vector2 worldPosition)
        {
            if (itemData == null) return;
            
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return;
            
            // 对齐到网格
            Vector2Int gridPos = gridHelper.WorldToGrid(worldPosition);
            Vector2 alignedWorldPos = gridHelper.GridToWorld(gridPos);
            
            // 检查是否可以放置
            bool canPlace = OnCheckCanPlace?.Invoke(itemData, alignedWorldPos) ?? false;
            
            // 计算高亮
            List<Vector2Int> highlightCells = null;
            
            if (canPlace)
            {
                highlightCells = OnCalculateHighlight?.Invoke(itemData, alignedWorldPos);
            }
            
            // 更新视图
            m_view?.OnDragUpdate(itemData.itemId, alignedWorldPos, highlightCells);
        }
        
        private void HandleDragEnd(DragItemData itemData, Vector2 worldPosition)
        {
            if (itemData == null) return;
            
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return;
            
            // 对齐到网格
            Vector2Int gridPos = gridHelper.WorldToGrid(worldPosition);
            Vector2 alignedWorldPos = gridHelper.GridToWorld(gridPos);
            
            // 检查是否可以放置
            bool canPlace = OnCheckCanPlace?.Invoke(itemData, alignedWorldPos) ?? false;
            
            if (canPlace)
            {
                // 执行放置逻辑
                GameActor placedItem = OnPlaceItem?.Invoke(itemData, alignedWorldPos);
                if (placedItem != null)
                {
                    // 放置成功，消耗数量
                    itemData.Consume();
                    m_view?.OnPlaceSuccess(itemData.itemId, alignedWorldPos);
                }
                else
                {
                    // 放置失败（创建失败）
                    m_view?.OnPlaceFailed(itemData.itemId, alignedWorldPos, "创建物品失败");
                }
            }
            else
            {
                // 放置失败（位置不可放置）
                m_view?.OnPlaceFailed(itemData.itemId, alignedWorldPos, "位置不可放置");
            }
            
            // 通知视图拖拽结束
            m_view?.OnDragEnd(itemData.itemId, alignedWorldPos);
        }
        
        private List<Vector2Int> CalculateHighlight(DragItemData itemData, Vector2 worldPosition)
        {
            if (itemData == null) return null;
            
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return null;
            
            Vector2Int gridPos = gridHelper.WorldToGrid(worldPosition);
            return gridHelper.GetTowerFootprint(gridPos, itemData.footprint);
        }
        
        private bool CheckCanPlace(DragItemData itemData, Vector2 worldPosition)
        {
            if (itemData == null) return false;
            
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return false;
            
            Vector2Int gridPos = gridHelper.WorldToGrid(worldPosition);
            return gridHelper.CanPlaceTower(gridPos, itemData.footprint);
        }
        
        private GameActor PlaceItem(DragItemData itemData, Vector2 worldPosition)
        {
            if (itemData == null) return null;
            
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return null;
            
            Vector2Int gridPos = gridHelper.WorldToGrid(worldPosition);
            
            // 创建物品（由外部实现）
            GameActor item = TowerCreator.CreateTower(itemData.itemId, worldPosition, gridHelper);
            if (item == null) return null;
            
            // 放置到网格
            bool placed = gridHelper.PlaceTower(gridPos, item, itemData.footprint);
            
            if (!placed)
            {
                // 放置失败，销毁物品
                if (item.m_Owner != null)
                {
                    Object.Destroy(item.m_Owner);
                }
                item.OnDestroy();
                return null;
            }
            
            return item;
        }
        
        private Vector2 GetCurrentWorldPosition()
        {
            var gridHelper = GridHelper.Instance;
            if (gridHelper == null || !gridHelper.IsInitialized) return Vector2.zero;
            return gridHelper.GetMouseWorldPositionAligned();
        }
    }
}
