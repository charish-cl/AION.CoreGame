using System;

namespace AION.CoreFramework
{
    public class SingleSelectModule<T>
    {
        private int? selectedId;
        private T selectedItem;

        /// <summary>
        /// 是否有选中项
        /// </summary>
        public bool HasSelect => selectedId.HasValue;

        public Action<T> OnItemSelected { get; set; }
        public Action<T> OnItemUnselected { get; set; }

        public int? CurrentSelectionId => selectedId;
        public T CurrentSelectionItem => selectedItem;

        // 无参构造函数
        public SingleSelectModule()
        {
            selectedId = null;
            selectedItem = default(T);
        }

        // 带回调函数的构造函数
        public SingleSelectModule(Action<T> onItemSelected, Action<T> onItemUnselected)
        {
            selectedId = null;
            selectedItem = default(T);
            OnItemSelected = onItemSelected;
            OnItemUnselected = onItemUnselected;
        }

        public bool SelectItem(int id, T item)
        {
            if (selectedId.HasValue && selectedId.Value == id)
            {
                // Item is already selected
                return false;
            }

            if (selectedId.HasValue)
            {
                // Deselect the currently selected item
                OnItemUnselected?.Invoke(selectedItem);
            }

            // Select the new item
            selectedId = id;
            selectedItem = item;
            OnItemSelected?.Invoke(item);
            return true;
        }

        public bool DeselectItem(int id)
        {
            if (!selectedId.HasValue || selectedId.Value != id)
            {
                // Item is not selected
                return false;
            }

            var item = selectedItem;
            selectedId = null;
            selectedItem = default(T);
            OnItemUnselected?.Invoke(item);
            return true;
        }

        public bool IsItemSelected(int id)
        {
            return selectedId.HasValue && selectedId.Value == id;
        }

        public void ResetSelection()
        {
            if (selectedId.HasValue)
            {
                OnItemUnselected?.Invoke(selectedItem);
                selectedId = null;
                selectedItem = default(T);
            }
        }

        public void Clear()
        {
            selectedId = null;
            selectedItem = default(T);
            OnItemSelected = null;
            OnItemUnselected = null;
        }
    }
}