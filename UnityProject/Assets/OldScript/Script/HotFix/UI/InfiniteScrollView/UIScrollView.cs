using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace GameKit
{
    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollView : SerializedMonoBehaviour
    {
        RectTransform m_Content;
        ScrollRect m_ScrollRect;
        public LinkedList<RectTransform> m_ActiveItems;
        Action<int, GameObject> UpdateDataFunc = delegate { }; // 填充数据回调：传入序号和Item的GameObject，无返回
        Func<int, Vector2> UpdateSizeFunc; // 获取元素尺寸的回调：传入序号，返回尺寸

        public int DataCnt;
        public float Space;
        public float TopSpace;
        float ItemHeight;
        float ItemWidth;
        private float LeftSpace;

        public GameObject m_ItemPrefab;
        public List<ItemData> m_ItemDatas;


        public Vector2 viewSize;
        
        public Dictionary<RectTransform, int> m_ItemIndexDict=new(); // 记录Item的索引
        [HideInInspector] public Action<GameObject> CreateItemCallBack; // 创建Item的回调：传入路径，返回GameObject 

        [Serializable]
        public class ItemData
        {
            public Vector2 Position;

            public Vector2 Size;

            public ItemData(Vector2 position, Vector2 size)
            {
                Position = position;
                Size = size;
            }
        }

        private float lastVerticalPosition; // 上一帧的垂直位置
        private float lastHorizontalPosition; // 上一帧的水平位置


        public List<RectTransform> CacheRectTransforms;
        public void CreateItem()
        {
            var _count = (int)(viewSize.y / ItemHeight) + 4;
       
            Debug.Log($"初始生成数量：{_count}");
            CacheRectTransforms = new List<RectTransform>();
            //生成Item
            for (int i = 0; i < _count && i < DataCnt; i++)
            {
                GameObject item;
                RectTransform rect;

                if (i < m_Content.childCount)
                {
                    item = m_Content.GetChild(i).gameObject;
                    rect = item.GetComponent<RectTransform>();
                }
                else
                {
                    // Debug.Log($"生成第{i}个物体");
                    item = Instantiate(m_ItemPrefab, m_Content);
                    if (CreateItemCallBack != null)
                    {
                        CreateItemCallBack(item);
                    }
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localScale = Vector3.one;
                    rect = item.GetComponent<RectTransform>();
                }
                CacheRectTransforms.Add(rect);
                // 设置左上角对齐
                rect.pivot = new Vector2(0, 1);
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                //设置位置和尺寸
                item.GetComponent<RectTransform>().anchoredPosition = m_ItemDatas[i].Position;
                item.GetComponent<RectTransform>().sizeDelta = m_ItemDatas[i].Size;
                //添加到激活列表
                m_ActiveItems.AddLast(item.transform as RectTransform);
                //填充数据
                if (UpdateDataFunc != null)
                {
                    UpdateDataFunc(i, item);
                }
                UpdateItemIndex(item.transform as RectTransform, i);
            }
            
        }

        public float TotalHeight;
        public void CalculateContentSize()
        {
            m_ItemDatas.Clear();
            float height = TopSpace;
            //计算每个Item的位置和尺寸
            for (int i = 0; i < DataCnt; i++)
            {
                Vector2 position = new Vector2(LeftSpace, height);
            
                Vector2 size;
                if (UpdateSizeFunc != null)
                {
                    size = UpdateSizeFunc(i);
                }
                else
                {
                    size = new Vector2(ItemWidth, ItemHeight);
                }
                height += size.y + Space;
                position.y = -position.y;
                m_ItemDatas.Add(new ItemData(position, size));
            }

            if (TotalHeight!=0 && Math.Abs(TotalHeight - height) > float.Epsilon)
            {
                m_ScrollRect.verticalNormalizedPosition = 1;
                Debug.Log("重置content位置");
            }
            TotalHeight = height;
            m_Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }


        [Button]
        public void SetVerticalNormalizedPosition(float value)
        {
            m_ScrollRect.verticalNormalizedPosition = value;
        }
        public void Initalize()
        {
            m_ScrollRect = GetComponent<ScrollRect>();
            m_Content = m_ScrollRect.content;
            viewSize = m_ScrollRect.viewport.rect.size;
            m_ActiveItems = new LinkedList<RectTransform>();
            m_ItemDatas = new List<ItemData>();
          
            lastVerticalPosition = m_ScrollRect.verticalNormalizedPosition;
            lastHorizontalPosition = m_ScrollRect.horizontalNormalizedPosition;
            ItemHeight = m_ItemPrefab.GetComponent<RectTransform>().rect.height;
            ItemWidth = m_ItemPrefab.GetComponent<RectTransform>().rect.width;
            //计算LeftSpace
            LeftSpace = (viewSize.x - ItemWidth) / 2;
        }
        bool IsInit = false;
        public   void Init(int dataCnt,Func<int, Vector2> updateSizeFunc = null, 
            Action<int, GameObject> updateDataFunc = null,Action<GameObject> createItemCallBack = null)
        {
          

            DataCnt = dataCnt;
            CreateItemCallBack = createItemCallBack;
            UpdateSizeFunc = updateSizeFunc;
            UpdateDataFunc = updateDataFunc;
            
            if (!IsInit)
            {
                IsInit = true;
                Initalize();
                CalculateContentSize();
                CreateItem();
            }
            else
            {
             
                //更新下方的Item的位置和尺寸
                m_ScrollRect.onValueChanged.RemoveAllListeners();
        
                CalculateContentSize();
                RearrangeItems(GetMaxViewIndexWhenReArrange());
                SetVisibleItems();
            }
            

            m_ScrollRect.onValueChanged.RemoveAllListeners();
            m_ScrollRect.onValueChanged.AddListener(UpdateItems);

        }

        [Button]
        //根据最大索引重新排列Item
        public void RearrangeItems(int maxIndex)
        {
            foreach (var mActiveItem in m_ActiveItems)
            {
                mActiveItem.gameObject.SetActive(false);
            }
            //遍历ItemDatas
            m_ActiveItems.Clear();
            int j = 0;
            for (int i = maxIndex; i >= 0 && j < CacheRectTransforms.Count;j++ ,i--)
            {
                //更新位置和尺寸
                CacheRectTransforms[j].anchoredPosition = m_ItemDatas[i].Position;
                CacheRectTransforms[j].sizeDelta = m_ItemDatas[i].Size;
                m_ActiveItems.AddFirst(CacheRectTransforms[j]);
                UpdateItemIndex(CacheRectTransforms[j], i);
                //填充数据
                if (UpdateDataFunc != null)
                {
                    UpdateDataFunc(i, CacheRectTransforms[j].gameObject);
                }
            }

            for (int i = maxIndex + 1; i < DataCnt && j < CacheRectTransforms.Count; j++, i++)
            {
                
                //更新位置和尺寸
                CacheRectTransforms[j].anchoredPosition = m_ItemDatas[i].Position;
                CacheRectTransforms[j].sizeDelta = m_ItemDatas[i].Size;
                m_ActiveItems.AddLast(CacheRectTransforms[j]);
                UpdateItemIndex(CacheRectTransforms[j], i);
                //填充数据
                if (UpdateDataFunc != null)
                {
                    UpdateDataFunc(i, CacheRectTransforms[j].gameObject);
                }
            }
            SetVisibleItems();
           
            UpdateItems(Vector2.zero);
        }
        
        [Button]
        public int GetMaxViewIndex()
        {
            //从后往前遍历ItemDatas
            //根据content的位置和尺寸，计算出最大可见的Item的序号
            //直接遍历链表，找到第一个不在屏幕上的Item，返回它的索引
            //从后往前遍历ItemDatas
            //根据content的位置和尺寸，计算出最大可见的Item的序号
            LinkedListNode<RectTransform> listNode = m_ActiveItems.Last;
            for (int i = m_ActiveItems.Count-1; i >= 0; i--)
            {
                int j =GetItemIndex(listNode.Value);
                
                if (!IsOverBottom(m_ItemDatas[j]) )
                {
                   return j;
                }
                listNode = listNode.Previous;
            }
            return 0;
        }

        public int GetMaxViewIndexWhenReArrange()
        {
            for (int i = m_ItemDatas.Count-1; i >= 0; i--)
            {
                if (!IsOverBottom(m_ItemDatas[i]) )
                {
                    return i;
                }
            }
            return 0;
        }
        int topCnt = 0;
        int bottomCnt = 0;
   
        private void UpdateItems(Vector2 value)
        {
            Profiler.BeginSample("UIScrollView.UpdateItems");
            if (m_ActiveItems.Count == 0)
            {
                return;
            }
            topCnt = 0;
            bottomCnt = 0;
            //遍历激活列表
            LinkedListNode<RectTransform> firstVisibleNode = m_ActiveItems.First;
            while (firstVisibleNode!= null &&!IsActive(firstVisibleNode.Value))
            {
                topCnt++;
                firstVisibleNode = firstVisibleNode.Next;
            }
            LinkedListNode<RectTransform> lastVisibleNode = m_ActiveItems.Last;
            while (lastVisibleNode!= null &&!IsActive(lastVisibleNode.Value))
            {
                bottomCnt++;
                lastVisibleNode = lastVisibleNode.Previous;
            }
            //上滑 
            bool IsRefresh = false;
            //判断是否超过了上边界
            while (firstVisibleNode!= null && (IsOverTop(firstVisibleNode.Value)))
            {
                int lastIndex = GetItemIndex(m_ActiveItems.Last.Value);
                if (lastIndex == m_ItemDatas.Count - 1)
                {
                    break; //已经是最后一个了，不需要再添加
                }
                topCnt++;
                firstVisibleNode = firstVisibleNode.Next;
                IsRefresh = true;
            }
            while(!IsRefresh && lastVisibleNode!= null && IsOverBottom(lastVisibleNode.Value))
            {
                int firstIndex = GetItemIndex(m_ActiveItems.First.Value);
                if (firstIndex == 0)
                {
                    break; //已经是第一个了，不需要再添加
                }
                bottomCnt++;
                lastVisibleNode = lastVisibleNode.Previous;
            }

            if (topCnt > 0 && bottomCnt > 0 && Mathf.Abs(topCnt - bottomCnt)<=2)
            {
                // Debug.Log("不用刷新");
            }
            else
            {
                // Debug.Log("刷新");
                while (topCnt-bottomCnt>2)
                {
                    var firstNode = m_ActiveItems.First;
                    if (AddLast(firstNode))
                    {
                        topCnt--;
                        bottomCnt++;
                    }
                    else
                    {
                       break;
                    }
                }
                while (bottomCnt - topCnt > 2)
                {
                    var lastNode = m_ActiveItems.Last;
                    if (AddFirst(lastNode))
                    {
                        bottomCnt--;
                        topCnt++;
                    }
                    else
                    {
                       break;
                    }
                }
            }
            SetVisibleItems();
            Profiler.EndSample();
        }
        [Button]
        // 方法：根据索引获取Item的RectTransform
        public int GetItemIndex(RectTransform rect)
        {
            if (m_ItemIndexDict.TryGetValue(rect, out int index))
            {
                return index;
            }
            return -1; // 未找到
        }

        // 示例：添加到末尾
        public bool AddLast(LinkedListNode<RectTransform> node)
        {
            var lastItem = m_ActiveItems.Last;
            var index = GetItemIndex(lastItem.Value);

            if (index >= m_ItemDatas.Count - 1)
            {
                return false; // 最后一个，不添加
            }

            m_ActiveItems.Remove(node);
            
            SetNodeData(node, index + 1);
            
            m_ActiveItems.AddLast(node);
            UpdateItemIndex(node.Value, index + 1);
            return true;
        }

        // 方法：更新Item的索引
        public void UpdateItemIndex(RectTransform rect, int index)
        {
            m_ItemIndexDict[rect] = index;
        }
        // 示例：添加到开头
        public bool AddFirst(LinkedListNode<RectTransform> node)
        {
            var firstItem = m_ActiveItems.First;
            var index = GetItemIndex(firstItem.Value);

            if (index <= 0)
            {
                return false; // 第一个，不添加
            }
            
            m_ActiveItems.Remove(node);
            SetNodeData(node, index - 1);
            m_ActiveItems.AddFirst(node);
            
            UpdateItemIndex(node.Value, index - 1);
            return true;
        }

  
        // 更新节点数据
        public void SetNodeData(LinkedListNode<RectTransform> node, int index)
        {
            var itemData = m_ItemDatas[index];
            node.Value.anchoredPosition = itemData.Position;
            node.Value.sizeDelta = itemData.Size;
            if (UpdateDataFunc != null)
            {
                UpdateDataFunc(index, node.Value.gameObject);
            }
        }

        [Button("刷新显示")]
        private void SetVisibleItems()
        {
            LinkedListNode<RectTransform> temp = m_ActiveItems.First;
            while (temp != null)
            {
                bool isInView = IsInView(temp.Value);
                SetActive(temp.Value,isInView);
                temp = temp.Next;
            }
        }

   
        public void SetActive(RectTransform go,bool active)
        {
            if (active)
            {
                go.localScale = Vector3.one;
            }
            else
            {
                go.localScale = Vector3.zero;
            }
            // go.SetActive(active);
        }
        
        [Button]
        public bool IsActive(RectTransform item)
        {
            return item.localScale.x > 0.01f;
        }
        [Button]
        //是否超过了上边界
        public bool IsOverTop(RectTransform item)
        {
            bool isOverTop = item.anchoredPosition.y + m_Content.anchoredPosition.y - item.rect.height > 0;
            return isOverTop;
        }

        
        //是否超过了下边界
        [Button]
        public bool IsOverBottom(RectTransform item)
        {
            var isOverBottom = item.anchoredPosition.y + m_Content.anchoredPosition.y + viewSize.y < 0;
            return isOverBottom;
        }
        public bool IsOverBottom(ItemData item)
        {
            var isOverBottom = item.Position.y + m_Content.anchoredPosition.y + viewSize.y < 0;
            return isOverBottom;
        }

        [Button]
        public bool IsInView(RectTransform item)
        {
            return !IsOverTop(item) && !IsOverBottom(item);
        }

      
    }
}