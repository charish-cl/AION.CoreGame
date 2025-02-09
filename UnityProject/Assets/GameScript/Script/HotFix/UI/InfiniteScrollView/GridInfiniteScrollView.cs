using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using AION.CoreFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace GameKit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class GridInfiniteScrollView : SerializedMonoBehaviour
    {
        [TabGroup("$SettingGroupTitle")] [OnValueChanged("EditorUpdate")]
        public int TopPadding = 0;


        [OnValueChanged("EditorUpdate")] [TabGroup("$SettingGroupTitle")]
        public float LeftSpace = 0;

        [TabGroup("$SettingGroupTitle")] [OnValueChanged("EditorUpdate")]
        public Vector2 Space;

        [TabGroup("$SettingGroupTitle")] public GameObject ItemPrefab;

        [TabGroup("$SettingGroupTitle")] [OnValueChanged("EditorUpdate")]
        public int Columns = 2;

        [TabGroup("$SettingGroupTitle")] [OnValueChanged("EditorUpdate")]
        public int Rows = 2;

        [HideInInspector] public Action<GameObject> CreateItemCallBack; // 创建Item的回调：传入路径，返回GameObject 
        [HideInInspector] public Func<int, Vector2> UpdateSizeFunc; // 获取元素尺寸的回调：传入序号，返回尺寸
        [HideInInspector] public Action<int> UpdateMaxIndexFunc; //当前视野最大索引,这个配合缓存下一页，配合缓存模块

        [HideInInspector]
        public Action<int, GameObject> UpdateDataFunc = delegate { }; // 填充数据回调：传入序号和Item的GameObject，无返回

        [TabGroup("$DebugGroupTile")] public Dictionary<int, Vector2> _sizes; // index->size
        [TabGroup("$DebugGroupTile")] public Dictionary<int, Vector2> _positions; // index->position
        private int _count; //生成的数量

        private ScrollRect _scroll;
        public RectTransform _content;
        private GameObject[] _items;
        private RectTransform[] _rects;
        private Vector2 _viewSize;

        public int TotalCount => _positions.Count;
        public float itemWidth { get; private set; }
        public float itemHeight { get; private set; }

        string GroupTitle = "预览工具";
        string SettingGroupTitle = "设置";
        string AnimGroupTile = "移动";
        string DebugGroupTile = "调试";

        [TabGroup("$GroupTitle")]
        [Button("添加元素")]
        public void TestPushItems()
        {
            _scroll = null;
            _items = null;
            _sizes = null;
            _positions = null;
            PushItem(20);
        }

        [TabGroup("$GroupTitle")]
        [Button("销毁元素")]
        public void TestDestroyItems()
        {
            // _positions = null;
        }

        [OnValueChanged("EditorSetLayout")] public bool isVertical = true; // 是否是垂直滚动

        public void EditorUpdate()
        {
            if (_sizes == null || _sizes.Count == 0)
            {
                return;
            }
        
            PushItem(0);
        }

        public void EditorSetLayout()
        {
            _scroll = GetComponent<ScrollRect>();
            _content = _scroll.content;
            _scroll.vertical = isVertical;
            _scroll.horizontal = !isVertical;

            if (isVertical)
            {
                _content.anchorMin = new Vector2(0, 1);
                _content.anchorMax = new Vector2(1, 1);
                _content.pivot = new Vector2(0, 1);
            }
            else
            {
                _content.anchorMin = new Vector2(0, 0);
                _content.anchorMax = new Vector2(0, 1);
                _content.pivot = new Vector2(0, 1);
            }

            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private async UniTask Initialize()
        {
            _scroll = GetComponent<ScrollRect>();
            _content = _scroll.content;

            _sizes = new Dictionary<int, Vector2>();
            _positions = new Dictionary<int, Vector2>();
            // _scroll.onValueChanged.RemoveAllListeners();
            _scroll.onValueChanged.AddListener(UpdateItemData);

            //这里如果子物体是自适应的（添加了ContentSizeFilter组件），有问题的 itemWidth是0
            itemWidth = ItemPrefab.GetComponent<RectTransform>().rect.width;
            itemHeight = ItemPrefab.GetComponent<RectTransform>().rect.height;

            if (Math.Abs(itemWidth) < 1e-10)
            {
                throw new Exception("生成Item宽度为 0,子物体是自适应的（添加了ContentSizeFilter组件）");
            }

            // 自适应的物体加载出来，加载的那一帧是不会自适应的，尺寸是（0，0）所以需要等下一帧
            var rect = _scroll.viewport.rect;
            await UniTask.NextFrame();
            _viewSize = new Vector2(rect.width, rect.height);


            isVertical = _scroll.vertical && !_scroll.horizontal; // 只允许一个方向滚动

            //根据水平或者垂直计算LeftSpace

            LeftSpace = (rect.width - itemWidth * Columns - Space.x * (Columns - 1)) / 2;

            // TopPadding = (int) (rect.height - itemHeight * Rows - Space.y * (Rows - 1)) / 2;
        }

        private void InitItems()
        {
            // 计算需要的数量
            var cacheCount =
                (isVertical
                    ? (Mathf.CeilToInt(_viewSize.y / itemHeight * Columns)+Columns*2)
                    :( Mathf.CeilToInt(_viewSize.x / itemWidth * Columns))+Rows*2);
            Debug.Log($"初始生成数量 {cacheCount}");

            // 初始化数组
            _items = new GameObject[cacheCount];
            _rects = new RectTransform[cacheCount];

            // 获取现有的子物体并补充不足的物体
            for (int i = 0; i < cacheCount; i++)
            {
                GameObject item;
                RectTransform rect;

                if (i < _content.childCount)
                {
                    item = _content.GetChild(i).gameObject;
                    rect = item.GetComponent<RectTransform>();
                }
                else
                {
                    // Debug.Log($"生成第{i}个物体");
                    item = Instantiate(ItemPrefab, _content);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localScale = Vector3.one;
                    rect = item.GetComponent<RectTransform>();
                }

                // 设置左上角对齐
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                _items[i] = item;
                _rects[i] = rect;

                CreateItemCallBack?.Invoke(item);
            }
        }

        public void ForceUpdateItemData()
        {
            _lastTime = interval;
            UpdateItemData(Vector2.zero);
        }

        float _lastTime = 0.1f;
        float interval = 0.05f;

        private void UpdateItemData(Vector2 value)
        {
            if (_items == null) return;

            // _lastTime += Time.deltaTime;
            // // Debug.Log(Time.deltaTime);
            // if (_lastTime < interval)
            // {
            //     return;
            // }
            //
            // _lastTime = 0;

            int visibleMaxIdx = GetMaxIndexInView();
            if (UpdateMaxIndexFunc != null)
            {
                UpdateMaxIndexFunc(visibleMaxIdx);
            }

            // Debug.Log($"刷新 {visibleMaxIdx}");
            //i对应对象池，j对应数据

           // Stopwatch stopwatch = new Stopwatch();
           // stopwatch.Start();
            for (int i = _items.Length - 1, j = visibleMaxIdx; i >= 0; j--, i--)
            {
                //不在视野范围内的物体不显示
                if (j < 0)
                {
                    _items[i].SetActive(false);
                    continue;
                }

                //在视野范围内的物体显示
                if (!_items[i].activeSelf)
                {
                    _items[i].SetActive(true);
                }

                //单列垂直才需要设置高度
                if (isVertical && Columns == 1)
                {
                    //高度不一致时才会更新高度，小优化
                    if (Math.Abs(_rects[i].rect.y - _sizes[j].y) > float.Epsilon)
                    {
                        _rects[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _sizes[j].y);
                    }
                }

                if (UpdateDataFunc != null)
                {
                    UpdateDataFunc(j, _items[i]);
                }

                TempItems[j] = _items[i];
                _rects[i].anchoredPosition = _positions[j];
            }
            // stopwatch.Stop();
            // Debug.Log($"刷新耗时 {stopwatch.ElapsedMilliseconds} ms");
        
        }

        Dictionary<int, GameObject> TempItems = new();

        public async UniTask PushItem(int count)
        {
            if (count < 0) return;
            if (_scroll == null)
            {
                await Initialize();
            }
            Debug.Log($"PushItem {count}");
            ReCalcSize(count);
            UpdateItemData(Vector2.zero);

            PageCnt = GetMaxIndexInView();
        }

        private int PageCnt;

        public void PopItem(int popCount)
        {
            if (popCount <= 0) return;
            ReCalcSize(-popCount);
            UpdateItemData(Vector2.zero);
        }

        public void PopAllItem()
        {
            PopItem(_count);
        }

        private void ReCalcSize(int count)
        {
            _count += count;
            if (_count < 0) _count = 0;
            if (_count>200)
            {
                Debug.Log("dwada ");
            }
            Vector2 size = CalcSizesAndPositions(); // 这里使用完_count后会被修改_count的值

            if (_items == null) InitItems();

            _content.sizeDelta = size;
            ContentAnchorPos = _content.anchoredPosition;
        }

        private Vector2 TempPosition = Vector2.one;

        private Vector2 CalcSizesAndPositions()
        {
            _sizes.Clear();
            _positions.Clear();

            float xPos = 0;
            float yPos = 0;
            int row = 0, colum = 0;
            //网格大部分情况下元素是一致的，不会出现每行不一致情况，所以这个
            float sum = 0;
            for (int i = 0; i < _count; i++)
            {
                if (isVertical)
                {
                    row = i / Columns;
                    colum = i % Columns;

                    xPos = LeftSpace + colum * (itemWidth + Space.x);

                    //只有一列的情况 
                    if (Columns == 1)
                    {
                        if (i == 0)
                        {
                            sum += TopPadding;
                        }
                        else
                        {
                            sum += _sizes[i - 1].y + Space.y;
                        }

                        yPos = -sum;
                    }
                    //TODO:多列未处理 选择列数中最大的？
                    else
                    {
                        yPos = -(TopPadding + row * (itemHeight + Space.y));
                    }
                }
                else
                {
                    row = i % Rows;
                    colum = i / Rows;
                    xPos = LeftSpace + colum * (itemWidth + Space.x);
                    yPos = -(TopPadding + row * (itemHeight + Space.y));
                }

                TempPosition.x = xPos;
                TempPosition.y = yPos;

                _sizes[i] = UpdateSizeFunc?.Invoke(i) ?? GetDefaultSize();

                _positions[i] = TempPosition;
            }

            if (_count == 0)
            {
                return Vector2.zero;
            }

            if (isVertical)
            {
                return new Vector2(_content.sizeDelta.x,
                    Mathf.Abs(_positions[_count - 1].y) + _sizes[_count - 1].y);
            }

            return new Vector2(_positions[_count - 1].x + _sizes[_count - 1].x + LeftSpace, _content.sizeDelta.y);
        }

        private Vector2 GetDefaultSize()
        {
            return new Vector2(itemWidth, itemHeight);
        }

        private int GetMaxIndexInView()
        {
            float size = isVertical
                ? (_content.anchoredPosition.y + _viewSize.y)
                : (-_content.anchoredPosition.x + _viewSize.x);

            for (int i = 0; i < _positions.Count; i++)
            {
                if ((isVertical && size <= -_positions[i].y) || (!isVertical && size <= _positions[i].x))
                {
                    return i;
                }
            }

            return _positions.Count - 1;
        }


        public enum Pos
        {
            Top,
            Mid,
            Bottom,
        }


        private float CalculateVerticalTargetPos(int index, Pos targetPos)
        {
            index = Mathf.Clamp(index, 0, _positions.Count - 1);

            var offset = -_positions[index].y;

            //如果是最后一页，默认对齐底部
            //-20704
            if (targetPos == Pos.Mid)
            {
                offset += _sizes[index].y / 2;
            }
            else if (targetPos == Pos.Bottom)
            {
                offset += _sizes[index].y;
            }

            return offset;
        }

        private float CalculateHorizontalTargetPos(int index, Pos targetPos)
        {
            index = Mathf.Clamp(index, 0, _positions.Count - 1);
            var offset = -_positions[index].x;

            if (targetPos == Pos.Mid)
            {
                offset -= _sizes[index].x / 2;
            }
            else if (targetPos == Pos.Bottom)
            {
                offset -= _sizes[index].x;
            }

            //如果是最后一页，默认对齐底部
            offset = Mathf.Clamp(offset, -(_content.rect.width - _viewSize.x), 0);

            return offset;
        }

        private Vector2 ContentAnchorPos;

        [TabGroup("$AnimGroupTile")]
        [Button]
        public async UniTask MoveToIndexAnim(int index, Pos targetPos = Pos.Mid, float time = 1,
            bool IsFromLast = false)
        {
            if (IsFromLast)
            {
                _scroll.verticalNormalizedPosition = 0;
            }

            float targetNormalizedPos;
            if (isVertical)
            {
                targetNormalizedPos = CalculateVerticalTargetPos(index, targetPos);

                ContentAnchorPos.y = targetNormalizedPos;
            }
            else
            {
                targetNormalizedPos = CalculateHorizontalTargetPos(index, targetPos);
                ContentAnchorPos.x = targetNormalizedPos;
            }

            //默认屏幕移动到中部
            await MoveContent(targetNormalizedPos, Pos.Mid, true);

            Refresh();
        }

        /// <summary>
        /// TODO:移动到指定索引，靠下还是靠上还是居中，靠下时是边缘对齐还是中心对齐
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetPos"></param>
        [TabGroup("$AnimGroupTile")]
        [Button]
        public void MoveToIndex(int index, Pos targetPos = Pos.Mid)
        {
            float targetNormalizedPos;
            if (isVertical)
            {
                targetNormalizedPos = CalculateVerticalTargetPos(index, targetPos);

                ContentAnchorPos.y = targetNormalizedPos;
            }
            else
            {
                targetNormalizedPos = CalculateHorizontalTargetPos(index, targetPos);
                ContentAnchorPos.x = targetNormalizedPos;
            }

            //默认屏幕移动到中部
            MoveContent(targetNormalizedPos, Pos.Mid, false);

            Refresh();
            // UpdateItemData(Vector2.zero);
        }


        public void Refresh()
        {
            //滑的太快防止检测不到，刷新下页面元素
            _scroll.onValueChanged.Invoke(Vector2.zero);
        }

        public GameObject GetItemAtIndex(int index)
        {
            if (TempItems.ContainsKey(index) && TempItems[index].activeInHierarchy)
            {
                return TempItems[index];
            }

            return null;
        }

        [TabGroup("$AnimGroupTile")]
        [Button]
        public async UniTask MovePreviousPage(bool IsNeedAnim = false, float time = 1)
        {
            //中间的那个元素
            var index = GetMaxIndexInView() - PageCnt / 2;
            float targetPos = 0;
            if (isVertical)
            {
                index -= PageCnt;
                targetPos = CalculateVerticalTargetPos(index, Pos.Mid);
            }
            else
            {
                index -= PageCnt;
                targetPos = CalculateHorizontalTargetPos(index, Pos.Mid);
            }

            MoveContent(targetPos, Pos.Mid, IsNeedAnim, time);
        }

        [TabGroup("$AnimGroupTile")]
        [Button]
        public async UniTask MoveNextPage(bool IsNeedAnim = false, float time = 1)
        {
            //中间的那个元素
            var index = GetMaxIndexInView() - PageCnt / 2;
            float targetPos = 0;
            if (isVertical)
            {
                index += PageCnt;
                targetPos = CalculateVerticalTargetPos(index, Pos.Mid);
            }
            else
            {
                index += PageCnt;
                targetPos = CalculateHorizontalTargetPos(index, Pos.Mid);
            }

            MoveContent(targetPos, Pos.Mid, IsNeedAnim, time);
        }

        /// <summary>
        /// 移动Panel到指定位置，以左上为起始点
        /// </summary>
        /// <param name="position"></param>
        /// <param name="targetPos"></param>
        /// <param name="IsNeedAnim"></param>
        /// <param name="time"></param>
        public async UniTask MoveContent(float position, Pos targetPos, bool IsNeedAnim = true, float time = 1)
        {
            if (isVertical)
            {
                // 根据 Pos 参数计算目标位置
                float targetY = position;
                switch (targetPos)
                {
                    case Pos.Top:
                        //本来就在最上面 以顶部为基准
                        break;
                    case Pos.Mid:
                        targetY -= _viewSize.y / 2;
                        break;
                    case Pos.Bottom:
                        targetY -= _viewSize.y;
                        break;
                }

                // 判断是否超出边界
                targetY = Mathf.Clamp(targetY, 0, _content.sizeDelta.y - _viewSize.y);
                ContentAnchorPos.y = targetY;
            }
            else
            {
                // 根据 Pos 参数计算目标位置
                float targetX = position;
                switch (targetPos)
                {
                    case Pos.Top:
                        break;
                    case Pos.Mid:
                        targetX += _viewSize.x / 2;
                        break;
                    case Pos.Bottom:
                        targetX += _viewSize.x;
                        break;
                }

                // 判断是否超出边界
                targetX = Mathf.Clamp(targetX, -_content.sizeDelta.x + _viewSize.x, 0);
                ContentAnchorPos.x = targetX;
            }

            // 移动到目标位置
            if (IsNeedAnim)
            {
                await _content.DOAnchorPos(ContentAnchorPos, time);
            }
            else
            {
                _content.anchoredPosition = ContentAnchorPos;
            }
        }
        
        
    }
}