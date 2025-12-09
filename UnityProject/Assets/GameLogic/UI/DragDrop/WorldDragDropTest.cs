using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AION.CoreFramework;
using Sirenix.OdinInspector;

namespace GameLogic
{
    /// <summary>
    /// 拖拽系统测试脚本 - 使用新架构，完全自动化
    /// </summary>
    public class WorldDragDropTest : SerializedMonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("测试拖拽项数据列表")]
        public DragItemData[] testDragItems = new DragItemData[]
        {
            new DragItemData(1, 1),
            new DragItemData(2, 1),
            new DragItemData(3, 1)
        };
        
        [Header("随机不可放置区域")]
        [Tooltip("是否随机生成不可放置区域")]
        public bool generateRandomUnplaceableAreas = true;
        
        [Tooltip("生成不可放置区域的比例（总网格的百分比）")]
        [Range(0.01f, 0.5f)]
        public float unplaceableAreaPercentage = 0.05f;
        
        [Header("自动创建设置")]
        [Tooltip("是否在Start时自动创建所有测试内容")]
        public bool autoCreateOnStart = true;
        
        [Header("调试信息")]
        [ReadOnly]
        [Tooltip("已创建的UI按钮数量")]
        public int uiButtonCount = 0;
        
        // 核心组件
        private GridHelper m_gridHelper;
        private DragDropViewBinder m_viewBinder;
        private DragDropLogicHandler m_logicHandler;
        private GridRenderer m_gridRenderer;
        private Camera m_worldCamera;
        
        // UI相关
        private List<GameObject> m_createdUIButtons = new List<GameObject>();
        private Dictionary<int, DragItemData> m_dragItemDataMap = new Dictionary<int, DragItemData>();
        
        private void Start()
        {
            if (autoCreateOnStart)
            {
                AutoSetupTest();
            }
        }
        
        /// <summary>
        /// 自动设置测试（一键完成所有设置）
        /// </summary>
        [Button("自动设置测试", ButtonSizes.Large)]
        public void AutoSetupTest()
        {
            Log.Info("WorldDragDropTest: 开始自动设置测试...");
            
            // 1. 初始化网格系统
            InitializeGridSystem();
            
            // 2. 初始化拖拽项数据
            InitializeDragItemData();
            
            // 3. 随机生成不可放置区域
            if (generateRandomUnplaceableAreas)
            {
                GenerateRandomUnplaceableAreas();
            }
            
            // 4. 创建必要组件
            CreateComponents();
            
            // 5. 创建测试UI按钮
            CreateTestUIButtons();
            
            Log.Info($"WorldDragDropTest: ✅ 自动设置完成！创建了 {uiButtonCount} 个测试按钮");
        }
        
        /// <summary>
        /// 初始化网格系统
        /// </summary>
        private void InitializeGridSystem()
        {
            m_gridHelper = GridHelper.Instance;
            m_gridHelper.Initialize();
            Log.Info("WorldDragDropTest: 初始化网格系统");
        }
        
        /// <summary>
        /// 初始化拖拽项数据
        /// </summary>
        private void InitializeDragItemData()
        {
            m_dragItemDataMap.Clear();
            
            if (testDragItems == null || testDragItems.Length == 0)
            {
                testDragItems = new DragItemData[]
                {
                    new DragItemData(1, 1),
                    new DragItemData(2, 1),
                    new DragItemData(3, 1)
                };
            }
            
            // 为每个拖拽项生成随机footprint（如果没有）
            foreach (var item in testDragItems)
            {
                if (item != null)
                {
                    if (item.footprint == null || item.footprint.Count == 0 || 
                        (item.footprint.Count == 1 && item.footprint[0] == Vector2Int.zero))
                    {
                        item.footprint = GenerateRandomFootprint();
                    }
                    m_dragItemDataMap[item.itemId] = item;
                }
            }
            
            Log.Info($"WorldDragDropTest: 初始化了 {m_dragItemDataMap.Count} 个拖拽项数据");
        }
        
        /// <summary>
        /// 生成随机footprint
        /// </summary>
        private List<Vector2Int> GenerateRandomFootprint()
        {
            List<Vector2Int> footprint = new List<Vector2Int>();
            int shapeType = Random.Range(0, 2);
            
            if (shapeType == 0)
            {
                // L形状
                footprint.Add(new Vector2Int(0, 0));
                footprint.Add(new Vector2Int(1, 0));
                footprint.Add(new Vector2Int(0, 1));
                footprint.Add(new Vector2Int(0, 2));
            }
            else
            {
                // 2x3矩形
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        footprint.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return footprint;
        }
        
        /// <summary>
        /// 随机生成不可放置区域
        /// </summary>
        private void GenerateRandomUnplaceableAreas()
        {
            if (m_gridHelper == null) return;
            
            var setting = m_gridHelper.GetSetting();
            if (setting == null) return;
            
            Vector2Int gridSize = setting.gridSize;
            List<Vector2Int> placeableCells = new List<Vector2Int>();
            
            // 收集所有可放置的网格位置
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int cellPos = new Vector2Int(x, y);
                    var cell = m_gridHelper.GetCellAt(cellPos);
                    if (cell != null && cell.isPlaceable)
                    {
                        placeableCells.Add(cellPos);
                    }
                }
            }
            
            if (placeableCells.Count == 0) return;
            
            // 计算要生成的数量
            int targetCount = Mathf.FloorToInt(placeableCells.Count * unplaceableAreaPercentage);
            
            // 随机选择并设置为不可放置
            int generated = 0;
            while (generated < targetCount && placeableCells.Count > 0)
            {
                int randomIndex = Random.Range(0, placeableCells.Count);
                Vector2Int selectedCell = placeableCells[randomIndex];
                m_gridHelper.SetPlaceable(selectedCell, false);
                placeableCells.RemoveAt(randomIndex);
                generated++;
            }
            
            Log.Info($"WorldDragDropTest: 随机生成了 {generated} 个不可放置网格单元");
        }
        
        /// <summary>
        /// 创建必要组件
        /// </summary>
        private void CreateComponents()
        {
            // 查找或创建世界相机
            m_worldCamera = Camera.main;
            if (m_worldCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                m_worldCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }
            
            // 查找或创建GridRenderer
            m_gridRenderer = FindObjectOfType<GridRenderer>();
            if (m_gridRenderer == null)
            {
                GameObject gridObj = new GameObject("GridRenderer");
                m_gridRenderer = gridObj.AddComponent<GridRenderer>();
            }
            
            // 创建视图绑定器
            m_viewBinder = new DragDropViewBinder();
            m_viewBinder.Initialize(m_gridRenderer);
            
            // 创建逻辑处理器
            m_logicHandler = new DragDropLogicHandler();
            m_logicHandler.Initialize(m_viewBinder);
            
            Log.Info("WorldDragDropTest: 创建核心组件完成");
        }
        
        /// <summary>
        /// 创建测试UI按钮
        /// </summary>
        private void CreateTestUIButtons()
        {
            CleanupUIButtons();
            
            // 查找或创建Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // 创建按钮容器
            Transform container = canvas.transform.Find("TestDragButtons");
            if (container == null)
            {
                GameObject containerObj = new GameObject("TestDragButtons");
                containerObj.transform.SetParent(canvas.transform, false);
                
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0f, 0f);
                containerRect.anchorMax = new Vector2(1f, 0f);
                containerRect.pivot = new Vector2(0.5f, 0f);
                containerRect.sizeDelta = new Vector2(0f, 150f);
                containerRect.anchoredPosition = new Vector2(0f, 75f);
                
                container = containerObj.transform;
            }
            
            // 为每个拖拽项创建按钮
            float buttonSpacing = 120f;
            float startX = -(testDragItems.Length - 1) * buttonSpacing * 0.5f;
            
            for (int i = 0; i < testDragItems.Length; i++)
            {
                var itemData = testDragItems[i];
                if (itemData == null || itemData.IsEmpty) continue;
                
                GameObject buttonObj = CreateUIButton(container, itemData, i);
                
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.anchoredPosition = new Vector2(startX + i * buttonSpacing, 0f);
                }
                
                // 使用新架构一键绑定
                DragDropBinder.CreateAndBind(buttonObj, itemData, m_logicHandler, m_worldCamera);
                
                m_createdUIButtons.Add(buttonObj);
            }
            
            uiButtonCount = m_createdUIButtons.Count;
            Log.Info($"WorldDragDropTest: 创建了 {uiButtonCount} 个测试UI按钮");
        }
        
        /// <summary>
        /// 创建单个UI按钮
        /// </summary>
        private GameObject CreateUIButton(Transform parent, DragItemData itemData, int index)
        {
            GameObject buttonObj = new GameObject($"TestDragButton_{itemData.itemId}");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);
            
            Image image = buttonObj.AddComponent<Image>();
            float hue = (index * 0.3f) % 1f;
            image.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
            
            buttonObj.AddComponent<Button>();
            
            // 添加文本（显示ID）
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = $"T{itemData.itemId}";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // 添加数量文本
            GameObject countTextObj = new GameObject("CountText");
            countTextObj.transform.SetParent(buttonObj.transform, false);
            var countText = countTextObj.AddComponent<Text>();
            countText.text = itemData.CurrentCount > 1 ? itemData.CurrentCount.ToString() : "";
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 18;
            countText.color = Color.yellow;
            countText.alignment = TextAnchor.UpperRight;
            countText.fontStyle = FontStyle.Bold;
            
            RectTransform countTextRect = countTextObj.GetComponent<RectTransform>();
            countTextRect.anchorMin = new Vector2(0.7f, 0.7f);
            countTextRect.anchorMax = new Vector2(0.95f, 0.95f);
            countTextRect.sizeDelta = Vector2.zero;
            countTextRect.anchoredPosition = Vector2.zero;
            
            // 设置DragItemData的UI引用
            itemData.SetUIElement(buttonObj, countText);
            
            return buttonObj;
        }
        
        /// <summary>
        /// 清理测试
        /// </summary>
        [Button("清理测试", ButtonSizes.Medium)]
        public void CleanupTest()
        {
            CleanupUIButtons();
            Log.Info("WorldDragDropTest: 清理完成");
        }
        
        /// <summary>
        /// 清理UI按钮
        /// </summary>
        private void CleanupUIButtons()
        {
            foreach (var button in m_createdUIButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            m_createdUIButtons.Clear();
            uiButtonCount = 0;
        }
        
        private void OnDestroy()
        {
            CleanupTest();
        }
    }
}
