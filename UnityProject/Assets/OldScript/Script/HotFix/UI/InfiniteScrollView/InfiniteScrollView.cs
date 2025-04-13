using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameKit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    
    public class InfiniteScrollView : SerializedMonoBehaviour
    {
        [LabelText("顶部间隔")] public int TopPadding = 0;

        [LabelText("底部间隔")] public int BottomPadding = 0;

        [LabelText("物体间隔")] public int ItemSpacing = 0;

        [LabelText("列表物品预制体")] public GameObject ItemPrefab;

        [LabelText("初始大小")] public int InitSize = 10;


        [HideInInspector]
        //设置Item的回调
        public Func<int, int> UpdateSizeFunc; //获取元素高度的回调：传入序号，返回高度

        [HideInInspector]
        public Action<int, GameObject> UpdateDataFunc = delegate { }; //填充数据回调：传入序号、和Item的GameObject，无返回

        //所有Item的信息
        private Dictionary<int, float> _heights; // index->height
        private Dictionary<int, float> _positions; // index->position
        private int _count;

        //可视范围的Item
        private ScrollRect _scroll; //自动获取的ScrollRect组件
        private RectTransform _content; //自动获取的Content
        private GameObject[] Items; //自动生产的Item（在Content下）
        private RectTransform[] _rects; //Item的RectTransform（提高效率）应该是不用每次GetComponent吧
        private float viewPointHeight; // Viewport的高度

        void Awake()
        {
            _scroll = GetComponent<ScrollRect>();
            _content = _scroll.viewport.transform.GetChild(0).GetComponent<RectTransform>();
            viewPointHeight = GetComponent<RectTransform>().sizeDelta.y; //View的高度
            _heights = new Dictionary<int, float>();
            _positions = new Dictionary<int, float>();
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, 0);
            _scroll.onValueChanged.AddListener(delegate { UpdateItemData(); });
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="height">容器高度</param>
        void InitItems(float height)
        {
            GameObject clone;
            float avgH = height / _heights.Count;
            //生成Item数量大小是固定的,根据View计算 额外加5个
            int fillCount = Mathf.RoundToInt(viewPointHeight / avgH) + 5;
            Items = new GameObject[fillCount];
            _rects = new RectTransform[fillCount];

            for (int i = 0; i < fillCount; i++)
            {
                clone = (GameObject)Instantiate(ItemPrefab, Vector3.zero, Quaternion.identity);
                clone.transform.SetParent(_content);
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;
                Items[i] = clone;

                //设置左右自适应
                _rects[i] = Items[i].gameObject.GetComponent<RectTransform>();
                _rects[i].pivot = new Vector2(0.5f, 1f);
                _rects[i].anchorMin = new Vector2(0f, 1f);
                _rects[i].anchorMax = Vector2.one;
                //这里设置一下是为了让Prefab子物体与自身重合 如果Prefab不是绝对布局的话，不过应该过了一帧，被content修改了
                _rects[i].offsetMax = Vector2.zero;
                _rects[i].offsetMin = Vector2.zero;
            }
        }

        Vector2 tempAnchoredPosition = Vector2.zero;
        Vector2 tempAnchoredSizeDelta = Vector2.zero;

        /// <summary>
        /// 更新Item数据,这个实在刷新高度后调用的
        /// </summary>
        void UpdateItemData()
        {
            if (Items == null) return;
            //if (Time.frameCount % 2 != 0) return;
            // 可以选择每隔一帧执行一次该方法，提升性能（这里被注释掉了）。
            int visibleMaxIdx = GetMaxIndexInView();

            // 逆序是为了方便对于 Item 的隐藏
            for (int i = Items.Length - 1, j = visibleMaxIdx; i >= 0; j--, i--)
            {
                if (j < 0)
                {
                    Items[i].SetActive(false);
                    // 如果索引小于 0，则隐藏该 Item。
                    continue;
                }

                if (!Items[i].activeSelf)
                    Items[i].SetActive(true);

                UpdateDataFunc(j, Items[i]);

                //对尺寸和位置进行赋值
                tempAnchoredPosition = _rects[i].anchoredPosition;
                tempAnchoredPosition.y = _positions[j];
                // 获取当前 Item 的 anchoredPosition，并设置其 y 坐标为 _positions 中的值。

                _rects[i].anchoredPosition = tempAnchoredPosition;
                // 将修改后的 anchoredPosition 应用到当前 Item。

                tempAnchoredSizeDelta = _rects[i].sizeDelta;
                tempAnchoredSizeDelta.y = _heights[j];
                // 获取当前 Item 的 sizeDelta，并设置其 y 坐标为 _heights 中的值。

                _rects[i].sizeDelta = tempAnchoredSizeDelta;
                // 将修改后的 sizeDelta 应用到当前 Item。
            }
        }


        /// <summary>
        /// 入栈新的Item
        /// </summary>
        /// <param name="count">新的Item数量</param>
        public void PushItem(int count)
        {
            if (count < 0) return;

            ReCalcHeight(count);
            UpdateItemData();
        }

        /// <summary>
        /// 出栈Item
        /// </summary>
        /// <param name="popCount">需移除数量</param>
        public void PopItem(int popCount)
        {
            if (popCount <= 0) return;

            ReCalcHeight(-popCount);
            UpdateItemData();
        }

        float ReCalcHeight(int count)
        {
            _count += count;
            if (_count < 0) _count = 0;
            float height = CalcSizesAndPositions();
            if (Items == null) InitItems(height);
            Vector2 temp = _content.sizeDelta;
            temp.y = height;
            _content.sizeDelta = temp;
            return height;
        }


        [Button]
        public void MoveToIndexAnim(int index, float speed = 1)
        {
            var targetNormalizedPos = 1 - (-_positions[index]) / _content.rect.height;
            _scroll.DOVerticalNormalizedPos(targetNormalizedPos, speed).SetSpeedBased();
        }

        [Button]
        public void MoveToIndex(int index)
        {
            var targetNormalizedPos = 1 - (-_positions[index]) / _content.rect.height;
            _scroll.verticalNormalizedPosition = targetNormalizedPos;
        }

        /// <summary>
        /// 计算并更新位置信息
        /// </summary>
        /// <returns>容器高度</returns>
        float CalcSizesAndPositions()
        {
            _heights.Clear();
            _positions.Clear();
            float sum = 0;
            for (int i = 0; i < _count; i++)
            {
                if (UpdateSizeFunc != null)
                {
                    _heights[i] = UpdateSizeFunc(i); //高度是自己决定的
                }
                else
                {
                    _heights[i] = ItemPrefab.GetComponent<RectTransform>().rect.height;
                }

                _positions[i] = -(TopPadding + i * ItemSpacing + sum); //Position是根据高度和间隔计算的 负值
                sum += _heights[i];
            }

            return sum + TopPadding + BottomPadding + (_count == 0 ? 0 : (_count - 1) * ItemSpacing);
        }

        /// <summary>
        /// 获取最大可见下标
        /// </summary>
        /// <returns>最大可见下标</returns>
        int GetMaxIndexInView()
        {
            float height = _content.anchoredPosition.y + viewPointHeight;
            for (int i = 0; i < _positions.Count; i++)
                if (height <= -_positions[i])
                    return i;
            return _positions.Count - 1;
        }
    }
}