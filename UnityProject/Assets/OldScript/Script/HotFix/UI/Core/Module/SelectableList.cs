namespace AION.CoreFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 泛型可选列表，支持单选/多选模式与选择限制
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    class SelectableList<T>
    {
        private readonly List<T> rawList; // ++ 改为只读防止外部篡改
        private List<int> selectedIndices;
        private int maxSelectionCount;
        private readonly bool isSingleSelection;

        // ++ 添加索引校验方法
        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= rawList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// 当前已选中的元素数量
        /// </summary>
        public int CurrentSelectionCount => selectedIndices.Count;

        /// <summary>
        /// 最大可选数量（单选模式下固定为1）
        /// </summary>
        public int MaxSelectionCount => maxSelectionCount;

        /// <summary>
        /// 当元素被选中时触发（参数为被选元素）
        /// </summary>
        public Action<T> OnItemSelected { get; set; }

        /// <summary>
        /// 当元素被取消选中时触发（参数为被取消元素）
        /// </summary>
        public Action<T> OnItemUnselected { get; set; }

        /// <summary>
        /// 当元素被切换选择状态时触发（参数为被切换元素）
        /// </summary>
        public Action<T,bool> OnToggleSelected
        {
            set
            {
                OnItemSelected += (item) => value?.Invoke(item, true);
                OnItemUnselected += (item) => value?.Invoke(item, false);
            }
        }
        /// <summary>
        /// 是否存在已选中的元素
        /// </summary>
        public bool HasSelect => selectedIndices.Count > 0;

        /// <summary>
        /// 是否已达到最大选择数量
        /// </summary>
        public bool HasMaxSelect => CurrentSelectionCount >= MaxSelectionCount;

        /// <summary>
        /// 构造多选可选列表
        /// </summary>
        /// <param name="rawList">原始数据列表</param>
        /// <param name="maxSelectionCount">最大可选数量</param>
        /// <param name="isSingleSelection">是否为单选模式</param>
        /// <exception cref="ArgumentNullException">当rawList为null时抛出</exception>
        public SelectableList(
            List<T> rawList,
            int maxSelectionCount)
        {
            this.rawList = rawList ?? throw new ArgumentNullException(nameof(rawList));
            this.isSingleSelection = maxSelectionCount == 1;
            this.maxSelectionCount = isSingleSelection ? 1 : Math.Max(1, maxSelectionCount); // ++ 保证至少能选1个
            selectedIndices = new List<int>(this.maxSelectionCount); // ++ 预设容量
        }

        /// <summary>
        /// 构造单选列表（默认最大可选数量为1）
        /// </summary>
        /// <param name="rawList"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SelectableList(List<T> rawList)
        {
            this.rawList = rawList ?? throw new ArgumentNullException(nameof(rawList));
            selectedIndices = new List<int>();
            maxSelectionCount = 1;
            this.isSingleSelection = true;
        }

        /// <summary>
        /// 获取当前选中的元素列表（按选择顺序）
        /// </summary>
        public List<T> GetSelectedItems() => selectedIndices.Select(index => rawList[index]).ToList();

        /// <summary>
        /// 获取当前选中的索引列表（按选择顺序）
        /// </summary>
        public List<int> GetSelectedIndices() => new List<int>(selectedIndices); // ++ 返回副本防止外部修改

        /// <summary>
        /// 获取按原始列表顺序排序的选中元素
        /// </summary>
        public List<T> GetSortedSelectedItems() => selectedIndices.OrderBy(i => i).Select(i => rawList[i]).ToList();

        /// <summary>
        /// 选择指定索引的元素
        /// </summary>
        /// <param name="index">要选择的元素索引</param>
        /// <exception cref="ArgumentOutOfRangeException">索引越界时抛出</exception>
        public void SelectItem(int index)
        {
            ValidateIndex(index);

            // 处理单选模式逻辑
            if (isSingleSelection)
            {
                // ++ 单选模式下直接替换选中项
                if (selectedIndices.Count == 1 && selectedIndices[0] == index)
                {
                    DeselectItem(index); // 点击已选中项时取消选择
                    return;
                }

                // 清空原有选择
                if (selectedIndices.Count > 0)
                {
                    var oldIndex = selectedIndices[0];
                    selectedIndices.Clear();
                    OnItemUnselected?.Invoke(rawList[oldIndex]);
                }
            }
            else if (selectedIndices.Contains(index))
            {
                return; // 多选模式下重复选择无操作
            }

            // 检查选择数量限制
            if (selectedIndices.Count >= maxSelectionCount) return;

            selectedIndices.Add(index);
            OnItemSelected?.Invoke(rawList[index]);
        }

        /// <summary>
        /// 取消选择指定索引的元素
        /// </summary>
        /// <param name="index">要取消选择的元素索引</param>
        /// <exception cref="ArgumentOutOfRangeException">索引越界时抛出</exception>
        public void DeselectItem(int index)
        {
            ValidateIndex(index);

            if (!selectedIndices.Remove(index)) return;
            OnItemUnselected?.Invoke(rawList[index]);
        }

        /// <summary>
        /// 切换指定索引的选择状态
        /// </summary>
        /// <param name="index">要切换的元素索引</param>
        public void ToggleItem(int index)
        {
            if (IsItemSelected(index)) DeselectItem(index);
            else SelectItem(index);
        }

        /// <summary>
        /// 判断指定索引是否已被选中
        /// </summary>
        public bool IsItemSelected(int index) => selectedIndices.Contains(index);

        /// <summary>
        /// 重置所有选择状态
        /// </summary>
        public void ResetSelection()
        {
            foreach (var index in selectedIndices)
            {
                OnItemUnselected?.Invoke(rawList[index]);
            }

            selectedIndices.Clear();
        }

        /// <summary>
        /// 清空列表及所有选择状态
        /// </summary>
        public void Clear()
        {
            rawList.Clear();
            ResetSelection();
            maxSelectionCount = 0;
        }

        /// <summary>
        /// 修改最大可选数量（会自动取消超出限制的选择）
        /// </summary>
        /// <param name="newMax">新的最大可选数量</param>
        public void ResetMaxSelectCount(int newMax)
        {
            if (newMax < 0) throw new ArgumentException("Max selection count cannot be negative");

            maxSelectionCount = isSingleSelection ? 1 : newMax;

            if (selectedIndices.Count <= maxSelectionCount) return;

            // ++ 触发取消选择事件
            var removed = selectedIndices.Skip(maxSelectionCount).ToList();
            selectedIndices = selectedIndices.Take(maxSelectionCount).ToList();
            removed.ForEach(i => OnItemUnselected?.Invoke(rawList[i]));
        }

        // ++ 新增批量操作方法
        /// <summary>
        /// 批量选择索引范围内的元素
        /// </summary>
        /// <param name="startIndex">起始索引（包含）</param>
        /// <param name="endIndex">结束索引（包含）</param>
        public void SelectRange(int startIndex, int endIndex)
        {
            ValidateIndex(startIndex);
            ValidateIndex(endIndex);
            if (startIndex > endIndex) (startIndex, endIndex) = (endIndex, startIndex);

            for (var i = startIndex; i <= endIndex; i++)
            {
                if (!IsItemSelected(i) && selectedIndices.Count < maxSelectionCount)
                {
                    SelectItem(i);
                }
            }
        }
        
        /// <summary>
        /// 一键取消所有已选中的元素
        /// </summary>
        public void ClearSelection()
        {
            if (selectedIndices.Count == 0) return; // 如果没有选中项，直接返回

            // 触发取消选择事件
            foreach (var index in selectedIndices)
            {
                OnItemUnselected?.Invoke(rawList[index]);
            }

            // 清空选择列表
            selectedIndices.Clear();
        }

    }
}