using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 拖拽视图接口 - 定义所有视图相关的回调
    /// 逻辑层通过此接口与视图层交互，实现完全解耦
    /// </summary>
    public interface IDragDropView
    {
        /// <summary>
        /// 开始拖拽
        /// </summary>
        void OnDragBegin(int itemId, Vector2 worldPosition, List<Vector2Int> highlightCells);
        
        /// <summary>
        /// 拖拽中（更新高亮）
        /// </summary>
        void OnDragUpdate(int itemId, Vector2 worldPosition, List<Vector2Int> highlightCells);
        
        /// <summary>
        /// 拖拽结束
        /// </summary>
        void OnDragEnd(int itemId, Vector2 worldPosition);
        
        /// <summary>
        /// 放置成功
        /// </summary>
        void OnPlaceSuccess(int itemId, Vector2 worldPosition);
        
        /// <summary>
        /// 放置失败
        /// </summary>
        void OnPlaceFailed(int itemId, Vector2 worldPosition, string reason);
    }
}

