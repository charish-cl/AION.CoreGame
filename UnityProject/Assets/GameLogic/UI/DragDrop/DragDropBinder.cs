using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 拖拽绑定器 - 提供简单的绑定方法，将 DragDropEventHandler 与 View 和 Logic 绑定
    /// </summary>
    public static class DragDropBinder
    {
        /// <summary>
        /// 绑定拖拽组件到视图和逻辑层（一键完成所有绑定）
        /// </summary>
        /// <param name="handler">拖拽事件处理器</param>
        /// <param name="itemData">拖拽项数据</param>
        /// <param name="logicHandler">逻辑处理器</param>
        public static void Bind(DragDropEventHandler handler, DragItemData itemData, DragDropLogicHandler logicHandler)
        {
            if (handler == null || itemData == null || logicHandler == null)
            {
                Debug.LogError("DragDropBinder.Bind: handler, itemData 和 logicHandler 不能为空");
                return;
            }
            
            // 设置拖拽项数据
            handler.SetDragItemData(itemData);
            
            // 注册到逻辑处理器（会自动绑定事件）
            logicHandler.RegisterDragItem(handler, itemData);
        }
        
        /// <summary>
        /// 为UI元素创建并绑定拖拽功能（完整流程）
        /// </summary>
        /// <param name="uiElement">UI元素（GameObject）</param>
        /// <param name="itemData">拖拽项数据</param>
        /// <param name="logicHandler">逻辑处理器</param>
        /// <param name="worldCamera">世界相机（可选）</param>
        /// <returns>创建的 DragDropEventHandler 组件</returns>
        public static DragDropEventHandler CreateAndBind(
            GameObject uiElement, 
            DragItemData itemData, 
            DragDropLogicHandler logicHandler,
            Camera worldCamera = null)
        {
            if (uiElement == null || itemData == null || logicHandler == null)
            {
                Debug.LogError("DragDropBinder.CreateAndBind: uiElement, itemData 和 logicHandler 不能为空");
                return null;
            }
            
            // 添加拖拽事件处理器组件
            DragDropEventHandler handler = uiElement.GetComponent<DragDropEventHandler>();
            if (handler == null)
            {
                handler = uiElement.AddComponent<DragDropEventHandler>();
            }
            
            // 设置世界相机
            if (worldCamera != null)
            {
                handler.worldCamera = worldCamera;
            }
            
            // 绑定
            Bind(handler, itemData, logicHandler);
            
            return handler;
        }
    }
}

