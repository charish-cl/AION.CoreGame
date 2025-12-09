using AION.CoreFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 抽卡对象 Widget
    /// </summary>
    public partial class DrawItem : UIWidget
    {
        private int m_towerId;
        private DragItemData m_itemData;
        
        /// <summary>
        /// 初始化抽卡对象
        /// </summary>
        public void Init(int towerId, DragItemData itemData, DragDropLogicHandler logicHandler, Camera worldCamera)
        {
            m_towerId = towerId;
            m_itemData = itemData;
            
            // 绑定拖拽组件
            if (logicHandler != null && worldCamera != null && gameObject != null)
            {
                DragDropBinder.CreateAndBind(gameObject, itemData, logicHandler, worldCamera);
            }
        }
    }
}

