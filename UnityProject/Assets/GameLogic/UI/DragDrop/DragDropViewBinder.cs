using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 拖拽视图绑定器 - 实现IDragDropView接口
    /// 职责：只负责视图层更新（高亮、网格显示等），不关心业务逻辑
    /// </summary>
    public class DragDropViewBinder : IDragDropView
    {
        private GridRenderer m_gridRenderer;
        private bool m_showGridOnDrag;
        private bool m_hideGridOnDragEnd;
        private bool m_isInitialized = false;
        
        /// <summary>
        /// 初始化视图绑定器
        /// </summary>
        public void Initialize(
            GridRenderer gridRenderer,
            bool showGridOnDrag = true,
            bool hideGridOnDragEnd = true)
        {
            if (gridRenderer == null)
            {
                Debug.LogError("DragDropViewBinder: gridRenderer 不能为空");
                return;
            }
            
            m_gridRenderer = gridRenderer;
            m_showGridOnDrag = showGridOnDrag;
            m_hideGridOnDragEnd = hideGridOnDragEnd;
            
            
            gridRenderer.Refresh();
            
            if (m_hideGridOnDragEnd)
            {
                gridRenderer.gameObject.SetActive(false);
            }
            
            m_isInitialized = true;
        }
        
        public void OnDragBegin(int itemId, Vector2 worldPosition, List<Vector2Int> highlightCells)
        {
            if (!m_isInitialized) return;
            
            // 显示网格
            if (m_showGridOnDrag && m_gridRenderer != null)
            {
                m_gridRenderer.gameObject.SetActive(true);
            }
            
            // 设置拖拽高亮
            if (m_gridRenderer != null && highlightCells != null)
            {
                m_gridRenderer.SetDragHighlight(highlightCells);
            }
        }
        
        public void OnDragUpdate(int itemId, Vector2 worldPosition, List<Vector2Int> highlightCells)
        {
            if (!m_isInitialized || m_gridRenderer == null) return;
            
            // 更新拖拽高亮（如果highlightCells为null则清除高亮）
            if (highlightCells != null && highlightCells.Count > 0)
            {
                m_gridRenderer.SetDragHighlight(highlightCells);
            }
            else
            {
                m_gridRenderer.ClearDragHighlight();
            }
        }
        
        public void OnDragEnd(int itemId, Vector2 worldPosition)
        {
            if (!m_isInitialized) return;
            
            // 清除拖拽高亮
            if (m_gridRenderer != null)
            {
                m_gridRenderer.ClearDragHighlight();
            }
            
            // 隐藏网格
            if (m_hideGridOnDragEnd && m_gridRenderer != null)
            {
                m_gridRenderer.gameObject.SetActive(false);
            }
        }
        
        public void OnPlaceSuccess(int itemId, Vector2 worldPosition)
        {
            if (!m_isInitialized || m_gridRenderer == null) return;
            
            // 放置成功，刷新网格显示（确保显示更新后的状态）
            m_gridRenderer.Refresh();
            
            // 清除高亮并隐藏网格
            m_gridRenderer.ClearDragHighlight();
            if (m_hideGridOnDragEnd)
            {
                m_gridRenderer.gameObject.SetActive(false);
            }
        }
        
        public void OnPlaceFailed(int itemId, Vector2 worldPosition, string reason)
        {
            if (!m_isInitialized) return;
            
            // 清除拖拽高亮
            if (m_gridRenderer != null)
            {
                m_gridRenderer.ClearDragHighlight();
            }
            
            // 隐藏网格
            if (m_hideGridOnDragEnd && m_gridRenderer != null)
            {
                m_gridRenderer.gameObject.SetActive(false);
            }
        }
    }
}
