using System;

namespace GameDevKit.PathFinding.FlowField
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// 流场调试可视化组件
    /// </summary>
    class FlowFieldDebug : SerializedMonoBehaviour
    {
        [BoxGroup("UI配置")] public Canvas targetCanvas;

        [BoxGroup("UI配置")] public GridLayoutGroup gridLayout;

        [BoxGroup("外观设置")] public Color fontColor = Color.white;

        [BoxGroup("外观设置")] public float fontSize = 50;

        [BoxGroup("外观设置")] public Color gridBgColor = new Color(1, 1, 1, 0.2f);

        [BoxGroup("网格属性")] public int xSize = 10;

        [BoxGroup("网格属性")] public int ySize = 10;

        [OnValueChanged("UpdateGrid")]
        [BoxGroup("网格属性")] [Range(1, 100)] public int Step = 1;

        [LabelText("塔")]
        public Vector2Int[] Towers;

        private FlowField flowField;
        private DebugGridCell[,] gridCells;

        // 单元格组件类
        public class DebugGridCell : MonoBehaviour
        {
            [SerializeField] private Text coordinateText;
            [SerializeField] private Image iconImag;

            Button button;

            public void Initialize()
            {
                iconImag = GetComponent<Image>();
                coordinateText = GetComponentInChildren<Text>();
            }

            public void SetCoordinateText(string text, Color textColor, float fontSize)
            {
                if (coordinateText != null)
                {
                    coordinateText.text = text;
                    coordinateText.color = textColor;
                    coordinateText.fontSize = Mathf.RoundToInt(fontSize);
                    coordinateText.alignment = TextAnchor.MiddleCenter;
                }
            }

            public void SetCoordinateText(string text)
            {
                if (coordinateText != null)
                {
                    coordinateText.text = text;
                }
            }

            public void SetImageIcon(Sprite sprite)
            {
                if (iconImag != null)
                {
                    iconImag.sprite = sprite;
                    iconImag.preserveAspect = true;
                    iconImag.color = Color.white;
                }
            }
        }

        void OnEnable()
        {
            InitializeFlowField();
        }

        void InitializeFlowField()
        {
            flowField = GetComponent<FlowField>();
            if (flowField != null)
            {
                xSize = flowField.XNum;
                ySize = flowField.YNum;
            }
        }


        [Button("生成/刷新网格", ButtonHeight = 30)]
        public void CreateDebugGrid()
        {
            flowField = GetComponent<FlowField>();
            flowField.Init(xSize, ySize, Step);
            flowField.CreateTowers(Towers);
            flowField.MultiBfs();

            CleanupGrid();
            CreateCanvasIfNeeded();
            SetupGridLayout();
            GenerateAllCells();
        }

      
        [HorizontalGroup("调试按钮")]
        [Button("上一步", ButtonHeight = 30)]
        public void PreStep()
        {
            Step--;
            UpdateGrid();
        }
        [HorizontalGroup("调试按钮")]
        [Button("下一步", ButtonHeight = 30)]
        public void NextStep()
        {
            Step++;
            UpdateGrid();
        }
        void CleanupGrid()
        {
            if (gridLayout != null)
            {
                DestroyImmediate(gridLayout.gameObject);
            }

            if (targetCanvas != null)
            {
                DestroyImmediate(targetCanvas.gameObject);
            }

            gridCells = null;
        }

        void CreateCanvasIfNeeded()
        {
            if (targetCanvas == null)
            {
                GameObject canvasObj = new GameObject("DebugCanvas");
                targetCanvas = canvasObj.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (gridLayout == null)
            {
                GameObject gridObj = new GameObject("DebugGrid");
                gridLayout = gridObj.AddComponent<GridLayoutGroup>();
                gridObj.transform.SetParent(targetCanvas.transform);
            }
        }

        void SetupGridLayout()
        {
            RectTransform gridRt = gridLayout.GetComponent<RectTransform>();
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.sizeDelta = Vector2.zero;
            gridRt.anchoredPosition = Vector2.zero;

            gridLayout.cellSize = new Vector2(200, 200);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = ySize;
        }

        void GenerateAllCells()
        {
            gridCells = new DebugGridCell[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    gridCells[x, y] = CreateSingleCell(x, y);
                }
            }
        }

        private void OnClickCell(int x, int y, DebugGridCell cell)
        {
        }

        public void UpdateGrid()
        {
            
            if (gridCells == null) return;
            flowField.ResetGrid();
            flowField.MultiBfs(Step);
            
            UpdateCellsDirection();
        }

        /// <summary>
        /// 更新所有cells的方向
        /// </summary>
        public void UpdateCellsDirection()
        {
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    var cell = gridCells[i, j];
                    OnShowText(i, j, cell);
                }
            }
        }

        private void OnShowText(int x, int y, DebugGridCell cell)
        {
            var direction = flowField[x, y].direction;

            //根据方向设置箭头
            string[] directionSymbol = { "←", "→", "↑", "↓", };

            var index = (int)direction;
            string text = "";
            if (index >= 0 && directionSymbol.Length > index)
            {
                text = directionSymbol[index];
            }

            cell.SetCoordinateText($"{text}");
        }

        DebugGridCell CreateSingleCell(int x, int y)
        {
            // 创建单元格对象及核心组件
            var cellObj = new GameObject($"Cell_{x}_{y}");
            var cell = cellObj.AddComponent<DebugGridCell>();

            // 设置父级关系（需先设置父级再操作UI组件）
            cellObj.transform.SetParent(gridLayout.transform);
            cellObj.transform.localScale = Vector3.one;

            Button button = cellObj.AddComponent<Button>();
            button.onClick.AddListener(() => { OnClickCell(x, y, cell); });
            // 添加背景组件及样式配置
            Image bg = cellObj.AddComponent<Image>();
            bg.color = gridBgColor;

            // 创建坐标文本组件
            var textObj = new GameObject("Coordinate");
            var textTransform = textObj.transform;
            textTransform.SetParent(cellObj.transform);

            // 配置文本布局
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            // 设置文本属性
            Text textComp = textObj.AddComponent<Text>();
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = fontColor;
            textComp.fontSize = Mathf.RoundToInt(fontSize);

            // 初始化单元格逻辑组件
            cell.Initialize();

            OnShowText(x, y, cell);
            return cell;
        }


        bool IsValidCell(int x, int y)
        {
            return gridCells != null &&
                   x >= 0 && x < xSize &&
                   y >= 0 && y < ySize;
        }

        // 自动刷新机制
        void OnValidate()
        {
            if (Application.isPlaying && gridCells != null)
            {
                RefreshAllCells();
            }
        }

        void RefreshAllCells()
        {
            if (gridCells == null) return;

            foreach (var cell in gridCells)
            {
                if (cell != null)
                {
                    // 更新现有单元格样式
                    cell.GetComponent<Image>().color = gridBgColor;
                    Text text = cell.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.color = fontColor;
                        text.fontSize = Mathf.RoundToInt(fontSize);
                    }
                }
            }
        }
    }
}