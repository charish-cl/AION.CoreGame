using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 网格拖拽视图层 - 统一管理网格渲染和UI显示
    /// 负责响应逻辑层的回调，更新视图显示
    /// </summary>
    public class GridDragView : MonoBehaviour
    {
        [Header("依赖组件")]
        [Tooltip("网格系统（必须设置）")]
        public TowerDefenseGridSystem gridSystem;
        
        [Tooltip("网格渲染器（必须设置）")]
        public GridRenderer gridRenderer;
        
        [Header("视图设置")]
        [Tooltip("拖拽开始时是否显示网格")]
        public bool showGridOnDrag = true;
        
        [Tooltip("拖拽结束时是否隐藏网格")]
        public bool hideGridOnDragEnd = true;
        
        private bool m_isInitialized = false;
        
        /// <summary>
        /// 初始化视图层
        /// </summary>
        public void Initialize(TowerDefenseGridSystem gridSystem, GridRenderer gridRenderer)
        {
            if (gridSystem == null)
            {
                Log.Error("GridDragView.Initialize: gridSystem 不能为空");
                return;
            }
            
            if (gridRenderer == null)
            {
                Log.Error("GridDragView.Initialize: gridRenderer 不能为空");
                return;
            }
            
            this.gridSystem = gridSystem;
            this.gridRenderer = gridRenderer;
            
            // 设置GridRenderer的gridSystem（移除GridRenderer中的自动查找）
            gridRenderer.gridSystem = gridSystem;
            
            // 初始化GridRenderer
            gridRenderer.Refresh();
            
            // 注册网格状态改变事件（由View层统一管理）
            gridSystem.OnGridChanged += OnGridChanged;
            
            // 初始隐藏网格
            if (hideGridOnDragEnd)
            {
                gridRenderer.gameObject.SetActive(false);
            }
            
            m_isInitialized = true;
            Log.Info("GridDragView: 视图层初始化完成");
        }
        
        private void OnDestroy()
        {
            // 取消事件注册
            if (gridSystem != null)
            {
                gridSystem.OnGridChanged -= OnGridChanged;
            }
        }
        
        /// <summary>
        /// 网格状态改变回调（由GridSystem触发）
        /// </summary>
        private void OnGridChanged()
        {
            if (m_isInitialized && gridRenderer != null)
            {
                gridRenderer.Refresh();
            }
        }
        
        /// <summary>
        /// 拖拽开始 - 显示网格和高亮
        /// </summary>
        public void OnDragBegin(int dragItemId, Vector2 worldPosition, List<Vector2Int> highlightCells)
        {
            if (!m_isInitialized) return;
            
            // 显示网格
            if (showGridOnDrag && gridRenderer != null)
            {
                gridRenderer.gameObject.SetActive(true);
            }
            
            // 设置拖拽高亮
            if (gridRenderer != null && highlightCells != null)
            {
                gridRenderer.SetDragHighlight(highlightCells);
            }
        }
        
        /// <summary>
        /// 拖拽更新 - 更新高亮位置
        /// </summary>
        public void OnDragUpdate(int dragItemId, Vector2 worldPosition, List<Vector2Int> highlightCells)
        {
            if (!m_isInitialized) return;
            
            // 更新拖拽高亮（如果highlightCells为null则清除高亮）
            if (gridRenderer != null)
            {
                if (highlightCells != null && highlightCells.Count > 0)
                {
                    gridRenderer.SetDragHighlight(highlightCells);
                }
                else
                {
                    gridRenderer.ClearDragHighlight();
                }
            }
        }
        
        /// <summary>
        /// 拖拽结束 - 隐藏网格和清除高亮
        /// </summary>
        public void OnDragEnd(int dragItemId, Vector2 worldPosition, bool isSuccess)
        {
            if (!m_isInitialized) return;
            
            // 清除拖拽高亮
            if (gridRenderer != null)
            {
                gridRenderer.ClearDragHighlight();
            }
            
            // 如果放置成功，先刷新网格显示（确保显示更新后的状态）
            if (isSuccess && gridRenderer != null)
            {
                gridRenderer.Refresh();
            }
            
            // 隐藏网格
            if (hideGridOnDragEnd && gridRenderer != null)
            {
                gridRenderer.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 拖拽失败 - 隐藏网格和清除高亮
        /// </summary>
        public void OnDragFailed(int dragItemId)
        {
            if (!m_isInitialized) return;
            
            // 清除拖拽高亮
            if (gridRenderer != null)
            {
                gridRenderer.ClearDragHighlight();
            }
            
            // 隐藏网格
            if (hideGridOnDragEnd && gridRenderer != null)
            {
                gridRenderer.gameObject.SetActive(false);
            }
        }
    }
}

