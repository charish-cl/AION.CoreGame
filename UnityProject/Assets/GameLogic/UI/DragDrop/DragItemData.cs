using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 拖拽项数据 - 管理拖拽项的信息和数量
    /// </summary>
    [System.Serializable]
    public class DragItemData
    {
        [Header("基础信息")]
        [Tooltip("拖拽项ID（例如：塔ID）")]
        public int itemId;
        
        [Tooltip("初始数量（默认1）")]
        public int count = 1;
        
        [Tooltip("塔的占用形状（相对于锚点的偏移坐标列表）")]
        public List<Vector2Int> footprint = new List<Vector2Int> { Vector2Int.zero };
        
        [Header("UI设置")]
        [Tooltip("UI元素（可选，如果为空则自动创建）")]
        public GameObject uiElement;
        
        [Tooltip("显示数量的Text组件（可选）")]
        public UnityEngine.UI.Text countText;
        
        /// <summary>
        /// 当前剩余数量
        /// </summary>
        public int CurrentCount { get; private set; }
        
        /// <summary>
        /// 是否已用完
        /// </summary>
        public bool IsEmpty => CurrentCount <= 0;
        
        /// <summary>
        /// 是否是最后一个
        /// </summary>
        public bool IsLast => CurrentCount == 1;
        
        public DragItemData(int itemId, int count = 1, List<Vector2Int> footprint = null)
        {
            this.itemId = itemId;
            this.count = count;
            this.CurrentCount = count;
            
            if (footprint != null && footprint.Count > 0)
            {
                this.footprint = new List<Vector2Int>(footprint);
            }
            else
            {
                this.footprint = new List<Vector2Int> { Vector2Int.zero };
            }
        }
        
        /// <summary>
        /// 消耗一个数量
        /// </summary>
        public bool Consume()
        {
            if (CurrentCount <= 0) return false;
            
            CurrentCount--;
            UpdateUI();
            return true;
        }
        
        /// <summary>
        /// 增加数量
        /// </summary>
        public void Add(int amount = 1)
        {
            CurrentCount += amount;
            UpdateUI();
        }
        
        /// <summary>
        /// 重置数量
        /// </summary>
        public void Reset()
        {
            CurrentCount = count;
            UpdateUI();
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (countText != null)
            {
                countText.text = CurrentCount > 1 ? CurrentCount.ToString() : "";
            }
            
            // 如果数量为0，隐藏UI元素
            if (uiElement != null)
            {
                bool shouldShow = CurrentCount > 0;
                if (uiElement.activeSelf != shouldShow)
                {
                    uiElement.SetActive(shouldShow);
                }
            }
        }
        
        /// <summary>
        /// 开始拖拽（如果是最后一个，隐藏UI）
        /// </summary>
        public void OnDragStart()
        {
            // if (IsLast && uiElement != null)
            // {
            //     uiElement.SetActive(false);
            // }
        }
        
        /// <summary>
        /// 拖拽失败（恢复UI显示）
        /// </summary>
        public void OnDragFailed()
        {
            if (IsLast && uiElement != null && CurrentCount > 0)
            {
                uiElement.SetActive(true);
            }
        }
        
        /// <summary>
        /// 设置UI元素
        /// </summary>
        public void SetUIElement(GameObject element, UnityEngine.UI.Text text = null)
        {
            uiElement = element;
            countText = text;
            UpdateUI();
        }
    }
}

