using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AION.CoreFramework;
using Sirenix.OdinInspector;

namespace GameLogic
{
    /// <summary>
    /// 世界拖拽系统测试脚本 - 完全自动化，无需手动设置
    /// </summary>
    public class WorldDragDropTest : SerializedMonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("测试塔数据列表（包含ID、数量、形状等）")]
        public DragItemData[] testDragItems = new DragItemData[]
        {
            new DragItemData(1, 1),
            new DragItemData(2, 1),
            new DragItemData(3, 1)
        };
        
        [Header("随机不可放置区域")]
        [Tooltip("是否随机生成不可放置区域")]
        public bool generateRandomUnplaceableAreas = true;
        
        [Tooltip("随机不可放置区域数量")]
        [Range(5, 50)]
        public int randomUnplaceableAreaCount = 20;

        [Tooltip("生成不可放置区域的比例（总网格的百分比）")]
        [Range(0.01f, 0.5f)]
        public float unplaceableAreaPercentage = 0.05f; // 默认5%的网格不可放置
        
        [Header("自动创建设置")]
        [Tooltip("是否在Start时自动创建所有测试内容")]
        public bool autoCreateOnStart = true;
        
        [Header("调试信息")]
        [ReadOnly]
        [Tooltip("已创建的UI按钮数量")]
        public int uiButtonCount = 0;
        
        // 内部变量
        private TowerPlacementManager m_placementManager;
        private List<GameObject> m_createdUIButtons = new List<GameObject>();
        private Dictionary<int, DragItemData> m_dragItemDataMap = new Dictionary<int, DragItemData>();
        private GameObject m_previewPrefab;
        private TowerDefenseGridSystem m_gridSystem;
        private Camera m_worldCamera;
        private GridRenderer m_gridRenderer;
        private GridDragView m_gridDragView; // 视图层管理器
        private Material m_attackRangeHighlightMaterial;
        
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
            
            m_gridSystem = TowerDefenseGridSystem.Instance;
            
            // 手动初始化网格系统（统一入口，确保执行顺序）
            if (m_gridSystem != null)
            {
                
                m_gridSystem.Initialize();
                Log.Info("WorldDragDropTest: 初始化网格系统");
            }

       
            // 1. 初始化拖拽项数据
            InitializeDragItemData();
            
            // 2. 随机生成不可放置区域（需要网格系统已初始化）
            if (generateRandomUnplaceableAreas)
            {
                Test_GenerateRandomUnplaceableAreas();
            }
            // 3. 自动查找/创建必要组件（必须先创建网格系统）
            AutoFindOrCreateComponents();

            // 4. 创建预览预制体
            CreatePreviewPrefab();
            
            // 5. 创建测试UI按钮
            CreateAllTestUIButtons();
            
            // 6. 初始化拖拽系统
            InitializeDragDropSystem();
            
            Log.Info($"WorldDragDropTest: ✅ 自动设置完成！创建了 {uiButtonCount} 个测试按钮");
        }
        
        /// <summary>
        /// 初始化拖拽项数据
        /// </summary>
        private void InitializeDragItemData()
        {
            m_dragItemDataMap.Clear();
            
            if (testDragItems == null || testDragItems.Length == 0)
            {
                // 如果没有配置，创建默认数据（会自动生成随机footprint）
                testDragItems = new DragItemData[]
                {
                    new DragItemData(1, 1),
                    new DragItemData(2, 1),
                    new DragItemData(3, 1)
                };
            }
            
            // 为每个拖拽项随机生成footprint
            foreach (var item in testDragItems)
            {
                if (item != null)
                {
                    // 如果footprint为空或只有默认的(0,0)，则随机生成
                    if (item.footprint == null || item.footprint.Count == 0 || 
                        (item.footprint.Count == 1 && item.footprint[0] == Vector2Int.zero))
                    {
                        item.footprint = GenerateRandomFootprint();
                        Log.Info($"WorldDragDropTest: 为拖拽项 {item.itemId} 生成随机footprint，包含 {item.footprint.Count} 个单元格");
                    }
                    m_dragItemDataMap[item.itemId] = item;
                }
            }
            
            Log.Info($"WorldDragDropTest: 初始化了 {m_dragItemDataMap.Count} 个拖拽项数据");
        }
        
        /// <summary>
        /// 生成随机footprint（L形状或2x3矩形）
        /// </summary>
        private List<Vector2Int> GenerateRandomFootprint()
        {
            List<Vector2Int> footprint = new List<Vector2Int>();
            
            // 随机选择形状类型：0=L形状，1=2x3矩形
            int shapeType = Random.Range(0, 2);
            
            if (shapeType == 0)
            {
                // L形状（俄罗斯方块）
                // 随机选择L的朝向：0=左上，1=右上，2=左下，3=右下
                int orientation = Random.Range(0, 4);
                
                switch (orientation)
                {
                    case 0: // 左上L
                        footprint.Add(new Vector2Int(0, 0)); // 锚点
                        footprint.Add(new Vector2Int(1, 0));
                        footprint.Add(new Vector2Int(0, 1));
                        footprint.Add(new Vector2Int(0, 2));
                        break;
                    case 1: // 右上L
                        footprint.Add(new Vector2Int(0, 0)); // 锚点
                        footprint.Add(new Vector2Int(1, 0));
                        footprint.Add(new Vector2Int(1, 1));
                        footprint.Add(new Vector2Int(1, 2));
                        break;
                    case 2: // 左下L
                        footprint.Add(new Vector2Int(0, 0)); // 锚点
                        footprint.Add(new Vector2Int(0, 1));
                        footprint.Add(new Vector2Int(0, 2));
                        footprint.Add(new Vector2Int(1, 2));
                        break;
                    case 3: // 右下L
                        footprint.Add(new Vector2Int(0, 0)); // 锚点
                        footprint.Add(new Vector2Int(0, 1));
                        footprint.Add(new Vector2Int(0, 2));
                        footprint.Add(new Vector2Int(1, 0));
                        break;
                }
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
        /// 测试方法：随机生成不可放置区域（单个网格单元）
        /// </summary>
        public void Test_GenerateRandomUnplaceableAreas()
        {
            if (m_gridSystem == null) return;

            Vector2Int gridSize = m_gridSystem.gridSize;
            int generated = 0;
            int attempts = 0;
            int maxAttempts = randomUnplaceableAreaCount * 10;

            // 收集所有可放置的网格位置
            List<Vector2Int> placeableCells = new List<Vector2Int>();
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int cellPos = new Vector2Int(x, y);
                    var cell = m_gridSystem.GetCellAt(cellPos);
                    if (cell != null && cell.isPlaceable)
                    {
                        placeableCells.Add(cellPos);
                    }
                }
            }

            // 如果没有可放置的网格，直接返回
            if (placeableCells.Count == 0)
            {
                Log.Warning("WorldDragDropTest: 没有可放置的网格用于生成不可放置区域");
                return;
            }

            // 计算要生成的数量（基于百分比或固定数量）
            int targetCount = Mathf.Min(
                randomUnplaceableAreaCount,
                Mathf.FloorToInt(placeableCells.Count * unplaceableAreaPercentage)
            );

            // 随机选择指定数量的网格设为不可放置
            while (generated < targetCount && generated < placeableCells.Count && attempts < maxAttempts)
            {
                attempts++;

                // 随机选择一个可放置的网格
                int randomIndex = Random.Range(0, placeableCells.Count);
                Vector2Int selectedCell = placeableCells[randomIndex];

                // 检查是否已经不可放置
                var cell = m_gridSystem.GetCellAt(selectedCell);
                if (cell != null && cell.isPlaceable)
                {
                    // 设置为不可放置
                    m_gridSystem.SetPlaceable(selectedCell, false);
                    generated++;

                    // 从列表中移除，避免重复选择
                    placeableCells.RemoveAt(randomIndex);
                }
            }

            Log.Info($"WorldDragDropTest: 随机生成了 {generated} 个不可放置网格单元（目标: {targetCount}，总网格: {placeableCells.Count}）");
        }
        
        /// <summary>
        /// 自动查找或创建必要组件
        /// </summary>
        private void AutoFindOrCreateComponents()
        {
            // 查找网格系统
            m_gridSystem = TowerDefenseGridSystem.Instance;
            if (m_gridSystem == null)
            {
                GameObject gridObj = new GameObject("TowerDefenseGridSystem");
                m_gridSystem = gridObj.AddComponent<TowerDefenseGridSystem>();
                
                // 自动添加调试绘制器
                var debugDrawer = gridObj.AddComponent<GridSystemDebugDrawer>();
                debugDrawer.showGridLines = true;
                debugDrawer.showOnlyVisibleGrid = true;
                
                Log.Info("WorldDragDropTest: 自动创建 TowerDefenseGridSystem 和 GridSystemDebugDrawer");
            }
            else
            {
                // 如果网格系统已存在，确保有调试绘制器
                var debugDrawer = m_gridSystem.GetComponent<GridSystemDebugDrawer>();
                if (debugDrawer == null)
                {
                    debugDrawer = m_gridSystem.gameObject.AddComponent<GridSystemDebugDrawer>();
                    debugDrawer.showGridLines = true;
                    debugDrawer.showOnlyVisibleGrid = true;
                    Log.Info("WorldDragDropTest: 为现有网格系统添加 GridSystemDebugDrawer");
                }
            }
            
    
            // 无论网格系统是新创建还是已存在，都确保有GridRenderer
            m_gridRenderer = m_gridSystem.GetComponent<GridRenderer>();
            if (m_gridRenderer == null)
            {
                m_gridRenderer = m_gridSystem.gameObject.AddComponent<GridRenderer>();
                Log.Info("WorldDragDropTest: 为网格系统添加 GridRenderer（Shader渲染）");
            }
            
            // 创建视图层管理器
            m_gridDragView = m_gridSystem.GetComponent<GridDragView>();
            if (m_gridDragView == null)
            {
                m_gridDragView = m_gridSystem.gameObject.AddComponent<GridDragView>();
                Log.Info("WorldDragDropTest: 为网格系统添加 GridDragView（视图层管理器）");
            }
            
            // 初始化视图层（统一管理渲染和UI）
            if (m_gridDragView != null && m_gridRenderer != null)
            {
                m_gridDragView.Initialize(m_gridSystem, m_gridRenderer);
            }
            
            // 查找世界相机
            m_worldCamera = Camera.main;
            if (m_worldCamera == null)
            {
                m_worldCamera = FindObjectOfType<Camera>();
            }
            if (m_worldCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                m_worldCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                Log.Info("WorldDragDropTest: 自动创建 Main Camera");
            }
            
            // 创建Shader材质
            CreateShaderMaterials();
            
            // 查找或创建放置管理器
            m_placementManager = FindObjectOfType<TowerPlacementManager>();
            if (m_placementManager == null)
            {
                GameObject managerObj = new GameObject("TowerPlacementManager");
                m_placementManager = managerObj.AddComponent<TowerPlacementManager>();
                m_placementManager.gridSystem = m_gridSystem;
                m_placementManager.worldCamera = m_worldCamera;
                m_placementManager.gridDragView = m_gridDragView; // 必须设置视图层，否则拖拽时不会显示网格
                Log.Info("WorldDragDropTest: 自动创建 TowerPlacementManager");
            }
            else
            {
                // 确保引用正确
                m_placementManager.gridSystem = m_gridSystem;
                m_placementManager.worldCamera = m_worldCamera;
                m_placementManager.gridDragView = m_gridDragView; // 必须设置视图层，否则拖拽时不会显示网格
            }
        }
        
        /// <summary>
        /// 创建Shader材质
        /// </summary>
        private void CreateShaderMaterials()
        {
            // 创建AttackRangeHighlight材质（用于攻击范围高亮）
            Shader attackRangeShader = Shader.Find("Custom/AttackRangeHighlight");
            if (attackRangeShader != null)
            {
                m_attackRangeHighlightMaterial = new Material(attackRangeShader);
                m_attackRangeHighlightMaterial.name = "AttackRangeHighlightMaterial";
                Log.Info($"WorldDragDropTest: ✅ 创建 AttackRangeHighlight 材质 - Shader={attackRangeShader.name}");
            }
            else
            {
                Log.Warning("WorldDragDropTest: 未找到 Custom/AttackRangeHighlight shader，将使用默认材质");
            }
        }
        
        /// <summary>
        /// 创建预览预制体
        /// </summary>
        private void CreatePreviewPrefab()
        {
            if (m_previewPrefab != null) return;
            
            // 创建预览预制体
            m_previewPrefab = new GameObject("TowerPreview");
            m_previewPrefab.SetActive(false); // 设为非激活，作为预制体模板
            
            // 添加SpriteRenderer
            SpriteRenderer renderer = m_previewPrefab.AddComponent<SpriteRenderer>();
            
            // 创建白色方块Sprite
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "PreviewSprite";
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 1f, 1f, 0.6f);
            
            // 设置大小（根据网格单元大小）
            if (m_gridSystem != null)
            {
                Vector2 cellSize = m_gridSystem.cellSize;
                m_previewPrefab.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);
            }
            else
            {
                m_previewPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            
            Log.Info("WorldDragDropTest: 创建预览预制体");
        }
        
        /// <summary>
        /// 创建所有测试UI按钮
        /// </summary>
        private void CreateAllTestUIButtons()
        {
            // 清理旧的按钮
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
                Log.Info("WorldDragDropTest: 自动创建 Canvas");
            }
            
            // 创建测试按钮容器
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
                
                GameObject buttonObj = CreateTestUIButton(container, itemData, i);
                
                // 手动设置位置（不使用LayoutGroup）
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.anchoredPosition = new Vector2(startX + i * buttonSpacing, 0f);
                }
                
                m_createdUIButtons.Add(buttonObj);
            }
            
            uiButtonCount = m_createdUIButtons.Count;
            Log.Info($"WorldDragDropTest: 创建了 {uiButtonCount} 个测试UI按钮");
        }
        
        /// <summary>
        /// 创建单个测试UI按钮
        /// </summary>
        private GameObject CreateTestUIButton(Transform parent, DragItemData itemData, int index)
        {
            GameObject buttonObj = new GameObject($"TestDragButton_{itemData.itemId}");
            buttonObj.transform.SetParent(parent, false);
            
            // 添加RectTransform
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);
            
            // 添加Image
            Image image = buttonObj.AddComponent<Image>();
            // 使用不同颜色区分不同按钮
            float hue = (index * 0.3f) % 1f;
            image.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
            
            // 添加Button
            Button button = buttonObj.AddComponent<Button>();
            
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
            
            // 添加数量文本（显示在右上角）
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
            
            // 添加WorldDragDrop组件
            WorldDragDrop dragDrop = buttonObj.AddComponent<WorldDragDrop>();
            dragDrop.SetDragItemId(itemData.itemId);
            dragDrop.worldCamera = m_worldCamera;
            dragDrop.enableBounceBack = true;
            dragDrop.previewPrefab = m_previewPrefab;
            // 网格对齐由TowerPlacementManager统一处理，不需要在这里设置
            
            return buttonObj;
        }
        
        /// <summary>
        /// 初始化拖拽系统
        /// </summary>
        private void InitializeDragDropSystem()
        {
            if (m_placementManager == null)
            {
                Log.Error("WorldDragDropTest: TowerPlacementManager 为空，无法初始化拖拽系统");
                return;
            }
            
            // 统一绑定所有Action回调（拖拽开始、结束、停止、成功、失败）
            m_placementManager.OnDragBegin += HandleDragBegin;
            m_placementManager.OnDragEnd += HandleDragEnd;
            m_placementManager.OnDragCancel += HandleDragCancel;
            m_placementManager.OnPlaceSuccess += HandlePlaceSuccess;
            m_placementManager.OnPlaceFailed += HandlePlaceFailed;
            
            // 为每个UI按钮注册到放置管理器
            for (int i = 0; i < m_createdUIButtons.Count; i++)
            {
                GameObject buttonObj = m_createdUIButtons[i];
                if (buttonObj == null) continue;
                
                var dragDrop = buttonObj.GetComponent<WorldDragDrop>();
                if (dragDrop == null) continue;
                
                int towerId = dragDrop.dragItemId;
                DragItemData itemData = m_dragItemDataMap.ContainsKey(towerId) ? m_dragItemDataMap[towerId] : null;
                
                if (itemData == null) continue;
                
                // 注册拖拽项到放置管理器
                m_placementManager.RegisterDragItem(dragDrop, towerId, itemData.footprint, 1);
            }
            
            Log.Info($"WorldDragDropTest: 初始化拖拽系统，注册了 {m_createdUIButtons.Count} 个拖拽项");
        }
        
        // ========== 统一的Action回调处理（所有拖拽相关事件统一在这里） ==========
        
        /// <summary>
        /// 处理拖拽开始
        /// </summary>
        private void HandleDragBegin(int towerId)
        {
            if (m_dragItemDataMap.ContainsKey(towerId))
            {
                m_dragItemDataMap[towerId].OnDragStart();
            }
        }
        
        /// <summary>
        /// 处理拖拽结束
        /// </summary>
        private void HandleDragEnd(int towerId)
        {
            // 拖拽结束后的处理（如果需要）
        }
        
        /// <summary>
        /// 处理拖拽取消/停止
        /// </summary>
        private void HandleDragCancel(int towerId)
        {
            if (m_dragItemDataMap.ContainsKey(towerId))
            {
                m_dragItemDataMap[towerId].OnDragFailed();
            }
            
            // 通知视图层拖拽失败
            if (m_gridDragView != null)
            {
                m_gridDragView.OnDragFailed(towerId);
            }
        }
        
        /// <summary>
        /// 处理放置成功
        /// </summary>
        private void HandlePlaceSuccess(int towerId, Vector2 worldPosition, GameActor tower)
        {
            Log.Info($"WorldDragDropTest: ✅ 成功放置塔 {towerId} 在位置 {worldPosition}");
            
            // 消耗数量
            if (m_dragItemDataMap.ContainsKey(towerId))
            {
                var itemData = m_dragItemDataMap[towerId];
                itemData.Consume();
            }
        }
        
        /// <summary>
        /// 处理放置失败
        /// </summary>
        private void HandlePlaceFailed(int towerId, Vector2 worldPosition, string reason)
        {
            Log.Warning($"WorldDragDropTest: ❌ 放置塔失败 {towerId} 在位置 {worldPosition}, 原因: {reason}");
        }
        
        
        /// <summary>
        /// 清理测试
        /// </summary>
        [Button("清理测试", ButtonSizes.Medium)]
        public void CleanupTest()
        {
            // 清理已创建的UI按钮
            foreach (var button in m_createdUIButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            m_createdUIButtons.Clear();
            uiButtonCount = 0;
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
                    DestroyImmediate(button);
                }
            }
            m_createdUIButtons.Clear();
            uiButtonCount = 0;
        }
        
        private void OnDestroy()
        {
            CleanupTest();
            CleanupUIButtons();
            
            if (m_previewPrefab != null)
            {
                DestroyImmediate(m_previewPrefab);
            }
            
            // 清理材质（运行时创建的材质会在场景切换时自动清理，但编辑器模式下需要手动清理）
            if (Application.isPlaying)
            {
                if (m_attackRangeHighlightMaterial != null)
                {
                    Destroy(m_attackRangeHighlightMaterial);
                }
            }
            else
            {
                if (m_attackRangeHighlightMaterial != null)
                {
                    DestroyImmediate(m_attackRangeHighlightMaterial);
                }
            }
        }
        
        
    }
}
