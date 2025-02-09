using System;
using System.Collections.Generic;
using System.Linq;

namespace AION.CoreFramework
{
    public class MultiSelectModule<T>
    {
        private Dictionary<int, T> selectedItems;
        private int maxSelectionCount;

        public int CurrentSelectionCount => selectedItems.Count;
        public int MaxSelectionCount => maxSelectionCount;

        public Action<T> OnItemSelected { get; set; }
        public Action<T> OnItemUnselected { get; set; }

        /// <summary>
        /// 是否有选中项
        /// </summary>
        public bool HasSelect => selectedItems.Count > 0;

        /// <summary>
        /// 是否达到最大选择数量
        /// </summary>
        public bool HasMaxSelect => CurrentSelectionCount >= MaxSelectionCount;

        // 带最大选择数量参数的构造函数
        public MultiSelectModule(int maxSelectionCount)
        {
            this.maxSelectionCount = maxSelectionCount;
            selectedItems = new Dictionary<int, T>();
        }

        // 带最大选择数量和回调函数参数的构造函数
        public MultiSelectModule(int maxSelectionCount, Action<T> onItemSelected, Action<T> onItemUnselected)
        {
            this.maxSelectionCount = maxSelectionCount;
            selectedItems = new Dictionary<int, T>();
            OnItemSelected = onItemSelected;
            OnItemUnselected = onItemUnselected;
        }

        public List<T> GetSelectList()
        {
            return selectedItems.Values.ToList();
        }

        /// <summary>
        /// 获取选中对象的序号集合
        /// </summary>
        /// <returns>选中对象的序号集合</returns>
        public List<int> GetSelectedIds()
        {
            return selectedItems.Keys.OrderBy(id => id).ToList();
        }

        /// <summary>
        /// 返回根据序号排序的选中对象列表
        /// </summary>
        /// <returns>根据序号排序的选中对象列表</returns>
        public List<T> GetSortedSelectList()
        {
            return selectedItems.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        }

        public bool SelectItem(int id, T item, bool IsAutoDisSelect = true)
        {
            if (selectedItems.ContainsKey(id))
            {
                // Item is already selected
                if (IsAutoDisSelect)
                {
                    DeselectItem(id);
                }
                return false;
            }

            if (selectedItems.Count >= maxSelectionCount)
            {
                // Maximum selection count reached
                return false;
            }

            selectedItems.Add(id, item);
            OnItemSelected?.Invoke(item);
            return true;
        }

        public bool DeselectItem(int id)
        {
            if (!selectedItems.ContainsKey(id))
            {
                // Item is not selected
                return false;
            }

            T item = selectedItems[id];
            selectedItems.Remove(id);
            OnItemUnselected?.Invoke(item);
            return true;
        }

        public bool IsItemSelected(int id)
        {
            return selectedItems.ContainsKey(id);
        }

        public void ResetSelection()
        {
            foreach (var kvp in selectedItems)
            {
                OnItemUnselected?.Invoke(kvp.Value);
            }
            selectedItems.Clear();
        }

        public void Clear()
        {
            selectedItems.Clear();
            OnItemSelected = null;
            OnItemUnselected = null;
            maxSelectionCount = 0;
        }

        public void ResetMaxSelectCount(int maxSelectCount)
        {
            maxSelectionCount = maxSelectCount;
            selectedItems.Clear();
        }
    }
}