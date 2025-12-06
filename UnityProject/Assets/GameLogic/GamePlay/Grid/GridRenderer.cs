using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    /// <summary>
    /// 网格渲染器 - 使用Shader直接绘制整个网格，无需为每个cell创建GameObject
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class GridRenderer : MonoBehaviour
    {
        [Header("网格系统引用")]
        [Tooltip("网格系统（如果不设置则自动查找）")]
        public TowerDefenseGridSystem gridSystem;
        
        [Header("渲染设置")]
        [Tooltip("是否显示网格线")]
        public bool showGridLines = true;
        
        [Tooltip("网格线宽度")]
        [Range(0f, 0.1f)]
        public float lineWidth = 0.02f;
        
        [Tooltip("渲染层级（确保在UI之上）")]
        public int sortingOrder = 32767;
        
        private MeshRenderer m_meshRenderer;
        private MeshFilter m_meshFilter;
        private Material m_material;
        private Texture2D m_gridStateTexture; // 用于传递cell状态的纹理
        private Texture2D m_highlightTexture; // 用于传递拖拽高亮的纹理（B通道）
        private bool m_isInitialized = false;
        
        [Header("拖拽高亮设置")]
        [Tooltip("拖拽高亮颜色（深绿色）")]
        public Color dragHighlightColor = new Color(0f, 0.7f, 0f, 0.6f);
        
        private void Awake()
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_meshFilter = GetComponent<MeshFilter>();
            
            // 创建材质
            Shader shader = Shader.Find("Custom/GridRenderer");
            if (shader != null)
            {
                m_material = new Material(shader);
                m_meshRenderer.material = m_material;
            }
            else
            {
                Log.Error("GridRenderer: 未找到Custom/GridRenderer shader");
            }
            
            // 设置渲染顺序
            m_meshRenderer.sortingOrder = sortingOrder;
        }
        
        private void OnDestroy()
        {
            // 清理资源
            if (m_gridStateTexture != null)
            {
                Destroy(m_gridStateTexture);
            }
            if (m_highlightTexture != null)
            {
                Destroy(m_highlightTexture);
            }
            if (m_material != null)
            {
                Destroy(m_material);
            }
        }
        
        /// <summary>
        /// 初始化网格渲染
        /// </summary>
        private void InitializeGrid()
        {
            if (gridSystem == null || m_material == null)
            {
                Log.Warning("GridRenderer: 网格系统或材质为空，无法初始化");
                return;
            }
            
            Vector2Int gridSize = gridSystem.gridSize;
            Vector2 cellSize = gridSystem.cellSize;
            Vector2 gridOrigin = gridSystem.gridOrigin;
            
            // 创建网格状态纹理（每个像素代表一个cell）
            // R通道：isOccupied (1.0 = occupied, 0.0 = not occupied)
            // G通道：isPlaceable (1.0 = placeable, 0.0 = not placeable)
            if (m_gridStateTexture == null || 
                m_gridStateTexture.width != gridSize.x || 
                m_gridStateTexture.height != gridSize.y)
            {
                if (m_gridStateTexture != null)
                {
                    Destroy(m_gridStateTexture);
                }
                
                m_gridStateTexture = new Texture2D(gridSize.x, gridSize.y, TextureFormat.RGBA32, false);
                m_gridStateTexture.filterMode = FilterMode.Point; // 使用点过滤，确保精确读取
                m_gridStateTexture.wrapMode = TextureWrapMode.Clamp;
            }
            
            // 创建高亮纹理（B通道：是否在拖拽高亮范围内）
            if (m_highlightTexture == null || 
                m_highlightTexture.width != gridSize.x || 
                m_highlightTexture.height != gridSize.y)
            {
                if (m_highlightTexture != null)
                {
                    Destroy(m_highlightTexture);
                }
                
                m_highlightTexture = new Texture2D(gridSize.x, gridSize.y, TextureFormat.RGBA32, false);
                m_highlightTexture.filterMode = FilterMode.Point;
                m_highlightTexture.wrapMode = TextureWrapMode.Clamp;
                
                // 初始化为全0（无高亮）
                Color[] clearPixels = new Color[gridSize.x * gridSize.y];
                for (int i = 0; i < clearPixels.Length; i++)
                {
                    clearPixels[i] = new Color(0f, 0f, 0f, 1f);
                }
                m_highlightTexture.SetPixels(clearPixels);
                m_highlightTexture.Apply();
            }
            
            // 更新网格状态纹理
            UpdateGridStateTexture();
            
            // 创建渲染网格（覆盖整个网格区域）
            CreateRenderMesh(gridSize, cellSize, gridOrigin);
            
            // 设置shader参数
            m_material.SetVector("_CellSize", cellSize);
            m_material.SetVector("_GridOrigin", gridOrigin);
            m_material.SetVector("_GridSize", new Vector4(gridSize.x, gridSize.y, 0, 0));
            m_material.SetTexture("_GridStateTex", m_gridStateTexture);
            m_material.SetTexture("_HighlightTex", m_highlightTexture);
            m_material.SetFloat("_ShowGridLines", showGridLines ? 1.0f : 0.0f);
            m_material.SetFloat("_LineWidth", lineWidth);
            
            // 设置颜色（降低透明度，避免太亮）
            m_material.SetColor("_ValidColor", new Color(0f, 1f, 0f, 0.15f));
            m_material.SetColor("_InvalidColor", new Color(1f, 0f, 0f, 0.15f));
            m_material.SetColor("_OccupiedColor", new Color(0.5f, 0.5f, 0.5f, 0.15f));
            m_material.SetColor("_GridLineColor", new Color(1f, 1f, 1f, 0.2f));
            m_material.SetColor("_DragHighlightColor", new Color(dragHighlightColor.r, dragHighlightColor.g, dragHighlightColor.b, 0.4f));
            
            m_isInitialized = true;
            Log.Info($"GridRenderer: 初始化完成 - GridSize={gridSize}, CellSize={cellSize}, GridOrigin={gridOrigin}");
        }
        
        /// <summary>
        /// 创建渲染网格
        /// </summary>
        private void CreateRenderMesh(Vector2Int gridSize, Vector2 cellSize, Vector2 gridOrigin)
        {
            Mesh mesh = new Mesh();
            mesh.name = "GridRendererMesh";
            
            // 计算网格总大小
            Vector2 totalSize = new Vector2(gridSize.x * cellSize.x, gridSize.y * cellSize.y);
            
            // 创建顶点（覆盖整个网格区域）
            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(gridOrigin.x, gridOrigin.y, 0);
            vertices[1] = new Vector3(gridOrigin.x + totalSize.x, gridOrigin.y, 0);
            vertices[2] = new Vector3(gridOrigin.x + totalSize.x, gridOrigin.y + totalSize.y, 0);
            vertices[3] = new Vector3(gridOrigin.x, gridOrigin.y + totalSize.y, 0);
            
            // 创建UV（用于世界坐标计算）
            Vector2[] uv = new Vector2[4];
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(1, 1);
            uv[3] = new Vector2(0, 1);
            
            // 创建三角形
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            m_meshFilter.mesh = mesh;
        }
        
        /// <summary>
        /// 更新网格状态纹理
        /// </summary>
        private void UpdateGridStateTexture()
        {
            if (gridSystem == null || m_gridStateTexture == null) return;
            
            Vector2Int gridSize = gridSystem.gridSize;
            
            // 遍历所有cell，更新纹理
            Color[] pixels = new Color[gridSize.x * gridSize.y];
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    var cell = gridSystem.GetCellAt(new Vector2Int(x, y));
                    if (cell != null)
                    {
                        // R通道：isOccupied
                        // G通道：isPlaceable
                        pixels[y * gridSize.x + x] = new Color(
                            cell.isOccupied ? 1f : 0f,
                            cell.isPlaceable ? 1f : 0f,
                            0f,
                            1f
                        );
                    }
                    else
                    {
                        pixels[y * gridSize.x + x] = new Color(0f, 0f, 0f, 1f);
                    }
                }
            }
            
            m_gridStateTexture.SetPixels(pixels);
            m_gridStateTexture.Apply();
        }
        
        /// <summary>
        /// 设置拖拽高亮区域（深绿色显示可放置范围）
        /// </summary>
        /// <param name="highlightCells">要高亮的网格位置列表</param>
        public void SetDragHighlight(List<Vector2Int> highlightCells)
        {
            if (!m_isInitialized || m_highlightTexture == null || gridSystem == null) return;
            
            Vector2Int gridSize = gridSystem.gridSize;
            
            // 先清除所有高亮
            Color[] pixels = new Color[gridSize.x * gridSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 1f);
            }
            
            // 设置高亮区域（B通道 = 1.0 表示高亮）
            if (highlightCells != null)
            {
                foreach (var cellPos in highlightCells)
                {
                    if (cellPos.x >= 0 && cellPos.x < gridSize.x && 
                        cellPos.y >= 0 && cellPos.y < gridSize.y)
                    {
                        int index = cellPos.y * gridSize.x + cellPos.x;
                        pixels[index] = new Color(0f, 0f, 1f, 1f); // B通道 = 1.0
                    }
                }
            }
            
            m_highlightTexture.SetPixels(pixels);
            m_highlightTexture.Apply();
        }
        
        /// <summary>
        /// 清除拖拽高亮
        /// </summary>
        public void ClearDragHighlight()
        {
            if (!m_isInitialized || m_highlightTexture == null || gridSystem == null) return;
            
            Vector2Int gridSize = gridSystem.gridSize;
            Color[] pixels = new Color[gridSize.x * gridSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 1f);
            }
            
            m_highlightTexture.SetPixels(pixels);
            m_highlightTexture.Apply();
        }
        
        /// <summary>
        /// 手动更新网格显示（外部调用）
        /// </summary>
        public void Refresh()
        {
            if (m_isInitialized)
            {
                UpdateGridStateTexture();
            }
            else
            {
                InitializeGrid();
            }
        }
    }
}

