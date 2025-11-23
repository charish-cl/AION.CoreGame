using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//换皮工具，主要为了处理UI换皮的需求，搭配psd2UGUI使用，建立映射关系

//TODO：增加效果图扫描功能，自动生成映射关系，然后解析图片调整布局
public class UISkinChangeWindow : EditorWindow
{
    public UISkinPanel leftPanel;
    public UISkinPanel rightPanel;

    // 分割线相关参数
    private float splitRatio = 0.5f;
    private Rect leftRect;
    private Rect rightRect;
    private bool isDragging;
    private const float SplitterWidth = 2f;

    // 存储键定义
    private const string SplitRatioKey = "UISkinChangeWindow_SplitRatio";

    // 绑定列表
    private List<BindingPair> bindings = new List<BindingPair>();

    // 移除列表（标记为移除的对象，刷新时不加载）
    private HashSet<GameObject> removedObjects = new HashSet<GameObject>();

    // 全局筛选选项
    private bool filterImage = true;
    private bool filterText = true;

    // Item大小调节
    private float itemSizeScale = 1.0f;

    // 隐藏已绑定对象的选项
    private bool hideAlreadyBound = false;

    [MenuItem("Tools/UI换皮工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<UISkinChangeWindow>("UI换皮工具");
    }

    private void OnEnable()
    {
        // 加载存储的分割比例
        splitRatio = EditorPrefs.GetFloat(SplitRatioKey, 0.5f);

        leftPanel = new UISkinPanel("左侧UI", this);
        rightPanel = new UISkinPanel("右侧UI", this);

        // 监听选择变化
        Selection.selectionChanged += OnSelectionChanged;

        // 自动选择当前选中对象的前两个子对象作为左右根对象
        AutoSelectRootObjects();
    }

    private void AutoSelectRootObjects()
    {
        if (Selection.activeGameObject != null)
        {
            var selectedObj = Selection.activeGameObject;

            // 情况1：有且仅有2个子对象时，直接作为左右根对象
            if (selectedObj.transform.childCount == 2)
            {
                var leftChild = selectedObj.transform.GetChild(1).gameObject;
                var rightChild = selectedObj.transform.GetChild(0).gameObject;

                leftPanel.SetRootObject(leftChild);
                rightPanel.SetRootObject(rightChild);

                Debug.Log($"<color=cyan>自动设置根对象: 左={leftChild.name}, 右={rightChild.name}</color>");
            }
            // 情况2：查找名为Canvas的子对象
            else
            {
                Transform canvasTransform = selectedObj.transform.Find("Canvas");
                if (canvasTransform != null)
                {
                    // 左侧：Canvas对象
                    // 右侧：选中的预制体根对象
                    leftPanel.SetRootObject(canvasTransform.gameObject, null);
                    rightPanel.SetRootObject(selectedObj, canvasTransform.gameObject);

                    Debug.Log(
                        $"<color=cyan>自动设置根对象: 左={selectedObj.name}（排除Canvas子物体）, 右={canvasTransform.name}</color>");
                }
            }
        }
    }

    private void OnDisable()
    {
        // 保存分割比例
        EditorPrefs.SetFloat(SplitRatioKey, splitRatio);
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        Repaint();
    }

    public bool GetFilterImage() => filterImage;
    public bool GetFilterText() => filterText;
    public float GetItemSizeScale() => itemSizeScale;
    public bool GetHideAlreadyBound() => hideAlreadyBound;

    private void OnGUI()
    {
        HandleSplitterDrag();

        // 顶部全局筛选区域 - 第一排
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("筛选类型:", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        filterImage = EditorGUILayout.ToggleLeft("Image", filterImage, GUILayout.Width(80));
        filterText = EditorGUILayout.ToggleLeft("Text", filterText, GUILayout.Width(80));
        if (EditorGUI.EndChangeCheck())
        {
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
        }

        GUILayout.FlexibleSpace();

        // 隐藏已绑定对象的按钮
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = hideAlreadyBound ? new Color(0.5f, 0.8f, 0.5f) : originalColor;
        if (GUILayout.Button(hideAlreadyBound ? "显示已绑定" : "隐藏已绑定", GUILayout.Height(25), GUILayout.Width(120)))
        {
            hideAlreadyBound = !hideAlreadyBound;
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }

        GUI.backgroundColor = originalColor;

        // 绑定数量提示
        GUILayout.Label($"已绑定: {bindings.Count}", EditorStyles.boldLabel, GUILayout.Width(80));

        // 刷新按钮
        if (GUILayout.Button("刷新", GUILayout.Height(25), GUILayout.Width(80)))
        {
            OnEnable();
        }

        // Copy按钮
        if (GUILayout.Button("Copy到CopyComponentTool", GUILayout.Height(25), GUILayout.Width(200)))
        {
            CopyToCopyComponentTool();
        }

        EditorGUILayout.EndHorizontal();

        // 第二排 - 清除相关按钮
        EditorGUILayout.BeginHorizontal();

        // 清除绑定按钮
        if (GUILayout.Button("清除所有绑定", GUILayout.Height(25), GUILayout.Width(120)))
        {
            bindings.Clear();
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }

        // 移除所有绑定按钮
        GUI.enabled = bindings.Count > 0;
        if (GUILayout.Button("移除所有绑定", GUILayout.Height(25), GUILayout.Width(120)))
        {
            // 将所有绑定的对象标记为移除
            foreach (var binding in bindings.ToList())
            {
                if (binding.leftObject != null && binding.leftObject.gameObject != null)
                {
                    removedObjects.Add(binding.leftObject.gameObject);
                }

                if (binding.rightObject != null && binding.rightObject.gameObject != null)
                {
                    removedObjects.Add(binding.rightObject.gameObject);
                }
            }

            bindings.Clear();
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }

        GUI.enabled = true;

        // 清除移除标记按钮
        GUI.enabled = removedObjects.Count > 0;
        if (GUILayout.Button($"清除移除标记({removedObjects.Count})", GUILayout.Height(25), GUILayout.Width(150)))
        {
            removedObjects.Clear();
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // 计算左右区域 - 减去顶部和底部区域的高度
        float topAreaHeight = 80; // 增加高度以容纳两排按钮
        float bottomAreaHeight = 30;
        leftRect = new Rect(0, topAreaHeight, position.width * splitRatio,
            position.height - topAreaHeight - bottomAreaHeight);
        rightRect = new Rect(
            position.width * splitRatio + SplitterWidth,
            topAreaHeight,
            position.width * (1 - splitRatio) - SplitterWidth,
            position.height - topAreaHeight - bottomAreaHeight
        );

        // 绘制子窗口
        leftPanel.Draw(leftRect);
        rightPanel.Draw(rightRect);

        // 绘制分割线
        DrawSplitter(topAreaHeight);

        // 底部Item大小调整区域 - 放在整个窗口的底部
        Rect bottomRect = new Rect(0, position.height - bottomAreaHeight, position.width, bottomAreaHeight);
        GUILayout.BeginArea(bottomRect);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Item大小:", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        itemSizeScale = EditorGUILayout.Slider(itemSizeScale, 0.5f, 2.0f);
        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void CopyToCopyComponentTool()
    {
        if (bindings.Count == 0)
        {
            Debug.LogWarning("没有绑定的对象");
            return;
        }

        // 直接调用CopyComponent静态方法，并设置preserveAnchorPivot为true
        foreach (var binding in bindings)
        {
            CopyComponentTool.Copy(
                binding.leftObject.gameObject,
                binding.rightObject.gameObject,
                false, // isExceptRectTransform
                false, // isNeedRemove
                true // preserveAnchorPivot - 保留anchor和pivot
            );
        }

        Debug.Log($"<color=green>成功复制 {bindings.Count} 个绑定对象的组件（保留Anchor和Pivot）</color>");
    }

    private void HandleSplitterDrag()
    {
        // 分割线交互区域
        var splitterRect = new Rect(
            position.width * splitRatio,
            50,
            SplitterWidth,
            position.height - 50
        );

        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

        switch (Event.current.type)
        {
            case EventType.MouseDown when splitterRect.Contains(Event.current.mousePosition):
                isDragging = true;
                Event.current.Use();
                break;

            case EventType.MouseUp:
                isDragging = false;
                break;

            case EventType.MouseDrag when isDragging:
                splitRatio = Mathf.Clamp(
                    Event.current.mousePosition.x / position.width,
                    0.15f,
                    0.85f
                );
                Repaint();
                break;
        }
    }

    private void DrawSplitter(float topOffset)
    {
        var splitterRect = new Rect(
            position.width * splitRatio,
            topOffset,
            SplitterWidth,
            position.height - topOffset
        );

        EditorGUI.DrawRect(splitterRect,
            EditorGUIUtility.isProSkin
                ? new Color(0.12f, 0.12f, 0.12f)
                : new Color(0.6f, 0.6f, 0.6f));
    }

    public void BindSelectedObjects(UISkinPanel sourcePanel)
    {
        var leftObj = leftPanel.GetSelectedObject();
        var rightObj = rightPanel.GetSelectedObject();

        if (leftObj == null || rightObj == null)
        {
            Debug.LogWarning("请在左右两边都选择一个对象");
            return;
        }

        // 通过GameObject检查是否已经存在绑定
        var existingBinding = bindings.FirstOrDefault(b =>
            (b.leftObject != null && b.leftObject.gameObject == leftObj.gameObject) ||
            (b.rightObject != null && b.rightObject.gameObject == rightObj.gameObject));

        if (existingBinding != null)
        {
            Debug.LogWarning("选中的对象已经被绑定");
            return;
        }

        // 添加新绑定（不显示提示）
        bindings.Add(new BindingPair { leftObject = leftObj, rightObject = rightObj });

        // 刷新列表以更新排序和颜色
        leftPanel.RefreshComponentList();
        rightPanel.RefreshComponentList();
        Repaint();
    }

    public bool IsObjectBound(UIComponentNode node)
    {
        // 通过GameObject来判断是否绑定，而不是通过对象引用
        return bindings.Any(b =>
            (b.leftObject != null && b.leftObject.gameObject == node.gameObject) ||
            (b.rightObject != null && b.rightObject.gameObject == node.gameObject));
    }

    public UIComponentNode GetPairedNode(UIComponentNode node)
    {
        // 通过GameObject来查找配对节点
        var binding = bindings.FirstOrDefault(b =>
            (b.leftObject != null && b.leftObject.gameObject == node.gameObject) ||
            (b.rightObject != null && b.rightObject.gameObject == node.gameObject));
        if (binding != null)
        {
            if (binding.leftObject != null && binding.leftObject.gameObject == node.gameObject)
                return binding.rightObject;
            else
                return binding.leftObject;
        }

        return null;
    }

    public void OnNodeClicked(UIComponentNode node, UISkinPanel panel)
    {
        // 如果点击的是已绑定对象，同步选择另一侧的配对对象
        var pairedNode = GetPairedNode(node);
        if (pairedNode != null)
        {
            if (panel == leftPanel)
            {
                rightPanel.SetSelectedNode(pairedNode);
            }
            else
            {
                leftPanel.SetSelectedNode(pairedNode);
            }
        }
    }

    public void UnbindNode(UIComponentNode node)
    {
        // 通过GameObject来查找并移除绑定
        var binding = bindings.FirstOrDefault(b =>
            (b.leftObject != null && b.leftObject.gameObject == node.gameObject) ||
            (b.rightObject != null && b.rightObject.gameObject == node.gameObject));
        if (binding != null)
        {
            bindings.Remove(binding);
            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }
    }

    public void MarkAsRemoved(UIComponentNode node)
    {
        if (node != null && node.gameObject != null)
        {
            removedObjects.Add(node.gameObject);

            // 获取配对节点
            var pairedNode = GetPairedNode(node);

            // 如果有配对节点，也标记为移除
            if (pairedNode != null && pairedNode.gameObject != null)
            {
                removedObjects.Add(pairedNode.gameObject);
            }

            // 移除绑定关系
            UnbindNode(node);

            leftPanel.RefreshComponentList();
            rightPanel.RefreshComponentList();
            Repaint();
        }
    }

    public bool IsMarkedAsRemoved(UIComponentNode node)
    {
        return node != null && node.gameObject != null && removedObjects.Contains(node.gameObject);
    }

    public List<BindingPair> GetBindings()
    {
        return bindings;
    }

    public int GetBindingIndex(UIComponentNode node)
    {
        // 通过GameObject来判断绑定索引，而不是通过对象引用
        for (int i = 0; i < bindings.Count; i++)
        {
            if ((bindings[i].leftObject != null && bindings[i].leftObject.gameObject == node.gameObject) ||
                (bindings[i].rightObject != null && bindings[i].rightObject.gameObject == node.gameObject))
            {
                return i;
            }
        }

        return -1;
    }

    public void AutoMatchAndBind()
    {
        var leftNodes = leftPanel.GetAllNodes();
        var rightNodes = rightPanel.GetAllNodes();

        if (leftNodes.Count == 0 || rightNodes.Count == 0)
        {
            Debug.LogWarning("请先在左右两边都选择UI根对象");
            return;
        }

        // 清除现有绑定
        bindings.Clear();

        // 使用全局最优匹配算法（Hungarian Algorithm的简化版）
        // 创建所有可能的匹配对及其分数
        List<MatchPair> allMatches = new List<MatchPair>();

        foreach (var leftNode in leftNodes)
        {
            foreach (var rightNode in rightNodes)
            {
                // 只匹配相同类型的组件
                if (leftNode.type != rightNode.type)
                    continue;

                float score = CalculateMatchScore(leftNode, rightNode);
                allMatches.Add(new MatchPair
                {
                    left = leftNode,
                    right = rightNode,
                    score = score
                });
            }
        }

        // 按分数从高到低排序
        allMatches = allMatches.OrderByDescending(m => m.score).ToList();

        // 用于追踪已经被绑定的节点
        HashSet<UIComponentNode> boundLeftNodes = new HashSet<UIComponentNode>();
        HashSet<UIComponentNode> boundRightNodes = new HashSet<UIComponentNode>();

        // 贪心选择分数最高且双方都未绑定的匹配对
        foreach (var match in allMatches)
        {
            if (!boundLeftNodes.Contains(match.left) && !boundRightNodes.Contains(match.right))
            {
                bindings.Add(new BindingPair { leftObject = match.left, rightObject = match.right });
                boundLeftNodes.Add(match.left);
                boundRightNodes.Add(match.right);
            }
        }

        Debug.Log($"<color=green>自动匹配完成，共绑定 {bindings.Count} 对对象</color>");

        // 刷新列表以按绑定顺序排序
        leftPanel.RefreshComponentList();
        rightPanel.RefreshComponentList();
        Repaint();
    }

    private class MatchPair
    {
        public UIComponentNode left;
        public UIComponentNode right;
        public float score;
    }

    private float CalculateMatchScore(UIComponentNode left, UIComponentNode right)
    {
        float score = 0f;

        // 获取RectTransform
        RectTransform leftRect = left.gameObject.GetComponent<RectTransform>();
        RectTransform rightRect = right.gameObject.GetComponent<RectTransform>();

        if (leftRect != null && rightRect != null)
        {
            // 获取根对象的RectTransform用于计算相对位置
            RectTransform leftRoot = leftPanel.GetRootObject()?.GetComponent<RectTransform>();
            RectTransform rightRoot = rightPanel.GetRootObject()?.GetComponent<RectTransform>();

            if (leftRoot != null && rightRoot != null)
            {
                // 计算相对于根对象的位置比率
                Vector2 leftRootSize = leftRoot.rect.size;
                // Vector2 rightRootSize = rightRoot.rect.size;
                //
                // 避免除以0
                // if (leftRootSize.x > 0 && leftRootSize.y > 0 && rightRootSize.x > 0 && rightRootSize.y > 0)
                {
                    Vector2 leftRelativePos = new Vector2(
                        leftRect.position.x / leftRootSize.x,
                        leftRect.position.y / leftRootSize.y
                    );
                    Vector2 rightRelativePos = new Vector2(
                        rightRect.position.x / leftRootSize.x,
                        rightRect.position.y / leftRootSize.y
                    );

                    // 相对位置差异越小，分数越高（最大200分）
                    float relativePosDiff = Vector2.Distance(leftRelativePos, rightRelativePos);
                    float posScore = Mathf.Max(0, 200 - relativePosDiff * 100);
                    score += posScore;
                }
                Debug.Log(leftRect.gameObject.name + "相对位置分数：" + rightRect.gameObject.name + "：" + score);
            }

            // 根据组件类型添加额外评分
            if (left.type == UIComponentType.Image)
            {
                var leftImage = left.component as Image;
                var rightImage = right.component as Image;

                if (leftImage != null && rightImage != null)
                {
                    // 如果sprite名称相同，分数拉满
                    if (leftImage.sprite != null && rightImage.sprite != null
                                                 && leftImage.sprite.name == rightImage.sprite.name)
                    {
                        score += 1000f;
                    }

                    // 图片大小评分（尺寸越接近分数越高）- 与位置分数权重相同
                    Vector2 leftSize = leftRect.sizeDelta;
                    Vector2 rightSize = rightRect.sizeDelta;

                    // 计算尺寸差异比例（归一化）
                    float avgSize = (leftSize.magnitude + rightSize.magnitude) / 2f;
                    if (avgSize > 0)
                    {
                        float sizeDiff = Vector2.Distance(leftSize, rightSize) / avgSize;
                        // 尺寸分数：差异越小分数越高（最大200分，与位置分数权重相同）
                        float sizeScore = Mathf.Max(0, 200 - sizeDiff * 200);
                        score += sizeScore;
                    }
                }
            }
            else if (left.type == UIComponentType.Text)
            {
                var leftText = left.component as Text;
                var rightText = right.component as Text;

                if (leftText != null && rightText != null)
                {
                    string leftStr = leftText.text ?? "";
                    string rightStr = rightText.text ?? "";

                    // 文字内容完全相同，分数拉满
                    if (leftStr == rightStr && !string.IsNullOrEmpty(leftStr))
                    {
                        score += 1000f;
                    }
                    else
                    {
                        // 计算文字相似度
                        int commonChars = 0;
                        foreach (char c in leftStr)
                        {
                            if (rightStr.Contains(c.ToString()))
                            {
                                commonChars++;
                            }
                        }

                        // 长度相同加分
                        if (leftStr.Length == rightStr.Length && leftStr.Length > 0)
                        {
                            score += 50f;
                        }

                        // 相同字符比例加分（最大100分）
                        if (leftStr.Length > 0)
                        {
                            float similarity = (float)commonChars / leftStr.Length;
                            score += similarity * 100f;
                        }
                    }
                }
            }
        }

        return score;
    }

    public class BindingPair
    {
        public UIComponentNode leftObject;
        public UIComponentNode rightObject;
    }
}

public class UISkinPanel
{
    private string title;
    private GameObject rootObject;
    private GameObject excludeObject; // 要排除的对象（用于排除Canvas子对象）
    private List<UIComponentNode> componentNodes = new List<UIComponentNode>();
    private List<UIComponentNode> allComponentNodes = new List<UIComponentNode>(); // 保存所有原始节点
    private Vector2 scrollPos;
    private UIComponentNode selectedNode;
    private List<UIComponentNode> multiSelectedNodes = new List<UIComponentNode>(); // 多选列表
    private UISkinChangeWindow parentWindow;

    // 存储原始位置用于复原
    private Vector2? originalAnchoredPosition = null;

    public UISkinPanel(string title, UISkinChangeWindow parentWindow)
    {
        this.title = title;
        this.parentWindow = parentWindow;
    }

    public UIComponentNode GetSelectedObject()
    {
        return selectedNode;
    }

    public GameObject GetRootObject()
    {
        return rootObject;
    }

    public void SetRootObject(GameObject obj, GameObject excludeObj = null)
    {
        rootObject = obj;
        excludeObject = excludeObj;
        RefreshComponentList();
    }

    public void SetSelectedNode(UIComponentNode node, bool updateSelection = false)
    {
        // 通过GameObject查找当前面板中对应的节点
        if (node != null)
        {
            selectedNode = componentNodes.FirstOrDefault(n => n.gameObject == node.gameObject);
            if (selectedNode == null)
            {
                selectedNode = node;
            }

            // 只有在updateSelection为true时才更新Unity的Selection
            if (updateSelection)
            {
                Selection.activeGameObject = node.gameObject;
            }
        }
        else
        {
            selectedNode = null;
        }
    }

    public List<UIComponentNode> GetAllNodes()
    {
        // 返回所有未过滤的节点用于自动匹配
        // 重新收集所有节点，不应用hideAlreadyBound过滤，但要应用excludeObject和removedObjects过滤
        List<UIComponentNode> allNodes = new List<UIComponentNode>();
        if (rootObject == null) return allNodes;

        bool filterImage = parentWindow.GetFilterImage();
        bool filterText = parentWindow.GetFilterText();

        // 收集所有Image和Text组件
        if (filterImage)
        {
            var images = rootObject.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                // 如果有excludeObject，跳过它及其子对象
                if (excludeObject != null && IsChildOf(img.transform, excludeObject.transform))
                {
                    continue;
                }


                allNodes.Add(new UIComponentNode
                {
                    gameObject = img.gameObject,
                    component = img,
                    type = UIComponentType.Image,
                    name = img.gameObject.name
                });
            }
        }

        if (filterText)
        {
            var texts = rootObject.GetComponentsInChildren<Text>(true);
            foreach (var txt in texts)
            {
                // 如果有excludeObject，跳过它及其子对象
                if (excludeObject != null && IsChildOf(txt.transform, excludeObject.transform))
                {
                    continue;
                }

                allNodes.Add(new UIComponentNode
                {
                    gameObject = txt.gameObject,
                    component = txt,
                    type = UIComponentType.Text,
                    name = txt.gameObject.name + " (" + txt.text + ")"
                });
            }
        }

        // 过滤被标记为移除的对象
        allNodes = allNodes.Where(n => !parentWindow.IsMarkedAsRemoved(n)).ToList();

        return allNodes;
    }

    public void Draw(Rect rect)
    {
        GUILayout.BeginArea(rect);

        // 标题和选择区域
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(title, EditorStyles.boldLabel);

        // 选择对象按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("选择UI根对象", GUILayout.Height(25)))
        {
            SelectRootObject();
        }

        if (rootObject != null)
        {
            EditorGUILayout.LabelField($"当前: {rootObject.name}", GUILayout.ExpandWidth(true));
        }

        EditorGUILayout.EndHorizontal();

        // 移动和复原按钮
        if (rootObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("移动", GUILayout.Height(25)))
            {
                MoveRootObject();
            }

            if (GUILayout.Button("复原", GUILayout.Height(25)))
            {
                RestoreRootObject();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        // 绑定和快速复制按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("绑定选中对象", GUILayout.Height(30)))
        {
            parentWindow.BindSelectedObjects(this);
        }

        if (GUILayout.Button("自动匹配绑定", GUILayout.Height(30)))
        {
            parentWindow.AutoMatchAndBind();
        }

        EditorGUILayout.EndHorizontal();

        // 快速复制按钮（1对1，不绑定）
        EditorGUILayout.BeginHorizontal();
        Color quickCopyColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f); // 蓝色
        if (GUILayout.Button("快速Copy", GUILayout.Height(25)))
        {
            QuickCopy();
        }

        GUI.backgroundColor = quickCopyColor;
        EditorGUILayout.EndHorizontal();

        // 批量移除按钮和批处理按钮
        EditorGUILayout.BeginHorizontal();
        string batchButtonText = multiSelectedNodes.Count > 0
            ? $"批量移除已选 ({multiSelectedNodes.Count}个)"
            : "批量移除已选 (0个)";

        GUI.enabled = multiSelectedNodes.Count > 0;
        if (GUILayout.Button(batchButtonText, GUILayout.Height(25)))
        {
            BatchUnbindSelected();
        }

        GUI.enabled = true;

        // 批处理按钮
        if (rootObject != null && componentNodes.Count > 0)
        {
            Color batchColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.8f, 0.4f); // 橙黄色
            if (GUILayout.Button("批处理当前对象", GUILayout.Height(25)))
            {
                OpenBatchProcessWindow();
            }

            GUI.backgroundColor = batchColor;
        }

        EditorGUILayout.EndHorizontal();


        // 组件列表
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (componentNodes.Count > 0)
        {
            DrawComponentTree();
        }
        else if (rootObject != null)
        {
            GUILayout.Label("没有找到Image或Text组件", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            GUILayout.Label("请先选择UI根对象", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private void SelectRootObject()
    {
        if (Selection.activeGameObject != null)
        {
            rootObject = Selection.activeGameObject;
            RefreshComponentList();
        }
        else
        {
            Debug.LogWarning("请先在Hierarchy中选择一个GameObject");
        }
    }

    public void RefreshComponentList()
    {
        componentNodes.Clear();
        if (rootObject == null) return;

        bool filterImage = parentWindow.GetFilterImage();
        bool filterText = parentWindow.GetFilterText();
        bool hideAlreadyBound = parentWindow.GetHideAlreadyBound();

        // 收集所有Image和Text组件
        if (filterImage)
        {
            var images = rootObject.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                // 如果有excludeObject，跳过它及其子对象
                if (excludeObject != null && IsChildOf(img.transform, excludeObject.transform))
                {
                    continue;
                }

                //如果图片是空的，跳过
                if (img.sprite == null)
                {
                    Debug.Log($"<color=green>{img.gameObject.name} 图片为空，已跳过</color>");
                    continue;
                }

                componentNodes.Add(new UIComponentNode
                {
                    gameObject = img.gameObject,
                    component = img,
                    type = UIComponentType.Image,
                    name = img.gameObject.name
                });
            }
        }

        if (filterText)
        {
            var texts = rootObject.GetComponentsInChildren<Text>(true);
            foreach (var txt in texts)
            {
                // 如果有excludeObject，跳过它及其子对象
                if (excludeObject != null && IsChildOf(txt.transform, excludeObject.transform))
                {
                    continue;
                }

                // 如果文字内容为空，跳过
                if (string.IsNullOrEmpty(txt.text))
                {
                    Debug.Log($"<color=green>{txt.gameObject.name} 文字内容为空，已跳过</color>");
                    continue;
                }

                componentNodes.Add(new UIComponentNode
                {
                    gameObject = txt.gameObject,
                    component = txt,
                    type = UIComponentType.Text,
                    name = txt.gameObject.name + " (" + txt.text + ")"
                });
            }
        }

        // 过滤被标记为移除的对象
        componentNodes = componentNodes.Where(n => !parentWindow.IsMarkedAsRemoved(n)).ToList();

        // 如果需要隐藏已绑定对象，进行过滤
        if (hideAlreadyBound)
        {
            componentNodes = componentNodes.Where(n => !parentWindow.IsObjectBound(n)).ToList();
        }

        // 排序：先按绑定顺序，再按层级路径
        componentNodes = componentNodes.OrderBy(n =>
            {
                int bindingIndex = parentWindow.GetBindingIndex(n);
                // 已绑定的排在前面，未绑定的排在后面
                // 绑定索引从0开始，未绑定返回-1，所以未绑定的会得到int.MaxValue
                return bindingIndex >= 0 ? bindingIndex : int.MaxValue;
            })
            .ThenBy(n => GetHierarchyPath(n.gameObject))
            .ToList();
    }

    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null && parent.gameObject != rootObject)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    // 检查transform是否是parentTransform的子对象
    private bool IsChildOf(Transform transform, Transform parentTransform)
    {
        if (transform == parentTransform) return true;

        Transform current = transform.parent;
        while (current != null)
        {
            if (current == parentTransform) return true;
            current = current.parent;
        }

        return false;
    }

    private void DrawComponentTree()
    {
        foreach (var node in componentNodes)
        {
            DrawComponentNode(node);
        }
    }

    private void DrawComponentNode(UIComponentNode node)
    {
        // 获取缩放比例
        float scale = parentWindow.GetItemSizeScale();

        // 根据类型和缩放比例决定行高
        float baseRowHeight = node.type == UIComponentType.Image ? 40 : 20;
        float rowHeight = baseRowHeight * scale;

        EditorGUILayout.BeginHorizontal(GUILayout.Height(rowHeight));

        // 选择状态
        bool isSelected = selectedNode == node;
        bool isMultiSelected = multiSelectedNodes.Contains(node);
        bool isBound = parentWindow.IsObjectBound(node);

        // 背景色 - 多选：橙色，选择：黄色，绑定：浅绿色
        if (isMultiSelected)
        {
            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f); // 橙色表示多选
        }
        else if (isSelected)
        {
            GUI.backgroundColor = Color.yellow;
        }
        else if (isBound)
        {
            GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
        }

        // 计算移除按钮的宽度
        float removeButtonWidth = 25 * scale;

        // 背景按钮 - 减去移除按钮的宽度，让两个按钮并排
        if (GUILayout.Button("", GUILayout.Height(rowHeight), GUILayout.ExpandWidth(true)))
        {
            // Ctrl + 点击：多选/取消选择
            if (Event.current.control)
            {
                if (multiSelectedNodes.Contains(node))
                {
                    // 如果已经在多选列表中，移除
                    multiSelectedNodes.Remove(node);
                }
                else
                {
                    // 添加到多选列表
                    multiSelectedNodes.Add(node);
                }
            }
            // Shift + 点击：框选（从上次选中到当前点击的范围）
            else if (Event.current.shift && selectedNode != null)
            {
                // 清空多选列表
                multiSelectedNodes.Clear();

                // 找到两个节点的索引
                int startIndex = componentNodes.IndexOf(selectedNode);
                int endIndex = componentNodes.IndexOf(node);

                if (startIndex >= 0 && endIndex >= 0)
                {
                    // 确保startIndex小于endIndex
                    if (startIndex > endIndex)
                    {
                        int temp = startIndex;
                        startIndex = endIndex;
                        endIndex = temp;
                    }

                    // 添加范围内的所有节点到多选列表
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        multiSelectedNodes.Add(componentNodes[i]);
                    }
                }
            }
            else
            {
                // 普通点击：单选
                selectedNode = node;
                multiSelectedNodes.Clear();
                Selection.activeGameObject = node.gameObject;
                parentWindow.OnNodeClicked(node, this);
            }
        }

        // 移除按钮 - 始终可点击
        Color buttonBgColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("×", GUILayout.Width(removeButtonWidth), GUILayout.Height(rowHeight)))
        {
            // 如果有多选，移除所有多选的对象
            if (multiSelectedNodes.Count > 0)
            {
                foreach (var selectedNode in multiSelectedNodes.ToList())
                {
                    parentWindow.MarkAsRemoved(selectedNode);
                }

                multiSelectedNodes.Clear();
            }
            else
            {
                // 否则只移除当前对象
                parentWindow.MarkAsRemoved(node);
            }
        }

        GUI.backgroundColor = buttonBgColor;

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // 获取整行的Rect用于绘制内容和右键菜单
        Rect rowRect = GUILayoutUtility.GetLastRect();
        Rect bgRect = new Rect(rowRect.x, rowRect.y, rowRect.width - removeButtonWidth - 2, rowHeight);

        // 右键菜单 - 解除绑定
        if (Event.current.type == EventType.ContextClick && rowRect.Contains(Event.current.mousePosition))
        {
            if (isBound)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("解除绑定"), false, () => parentWindow.UnbindNode(node));
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        // 在背景按钮上绘制内容（图标和文字）
        Rect contentRect = new Rect(bgRect.x + 5, bgRect.y, bgRect.width - 5, bgRect.height);

        // 绘制图标
        Texture icon = GetIconForNode(node);
        float baseIconSize = node.type == UIComponentType.Image ? 36 : 16;
        float iconSize = baseIconSize * scale;
        Rect iconRect = new Rect(contentRect.x, contentRect.y + (rowHeight - iconSize) / 2, iconSize, iconSize);
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon);
        }

        // 绘制名称
        float nameOffset = (node.type == UIComponentType.Image ? 40 : 20) * scale;
        Rect nameRect = new Rect(contentRect.x + nameOffset, contentRect.y, contentRect.width - nameOffset,
            contentRect.height);
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = Mathf.RoundToInt(12 * scale),
            normal = { textColor = isSelected ? Color.white : Color.black }
        };

        GUI.Label(nameRect, node.name, labelStyle);
    }

    private Texture GetIconForNode(UIComponentNode node)
    {
        if (node.type == UIComponentType.Image)
        {
            var image = node.component as Image;
            if (image != null && image.sprite != null)
            {
                return AssetPreview.GetAssetPreview(image.sprite);
            }

            return EditorGUIUtility.IconContent("Image Icon").image;
        }
        else if (node.type == UIComponentType.Text)
        {
            return EditorGUIUtility.IconContent("Text Icon").image;
        }

        return null;
    }

    private void MoveRootObject()
    {
        if (rootObject == null) return;

        var rectTransform = rootObject.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 保存原始位置（如果还没保存的话）
        if (!originalAnchoredPosition.HasValue)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        // 获取宽度并偏移
        float width = rectTransform.rect.width;
        Vector2 newPosition = rectTransform.anchoredPosition;
        newPosition.x -= width;

        rectTransform.anchoredPosition = newPosition;

        // 注意：移动操作不保存，这只是临时预览位置
    }

    private void RestoreRootObject()
    {
        if (rootObject == null) return;

        var rectTransform = rootObject.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 如果有保存的原始位置，恢复它
        if (originalAnchoredPosition.HasValue)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition.Value;
            originalAnchoredPosition = null;
        }
        else
        {
            // 如果没有保存的位置，将x设为0
            Vector2 newPosition = rectTransform.anchoredPosition;
            newPosition.x = 0;
            rectTransform.anchoredPosition = newPosition;
        }

        // 注意：复原操作不保存，这只是临时预览位置
    }

    private void BatchUnbindSelected()
    {
        if (multiSelectedNodes.Count == 0)
        {
            Debug.LogWarning("没有选中任何对象");
            return;
        }

        int count = multiSelectedNodes.Count;

        // 批量标记为移除（先收集再移除，避免在遍历时修改集合）
        foreach (var node in multiSelectedNodes.ToList())
        {
            parentWindow.MarkAsRemoved(node);
        }

        // 清空多选列表
        multiSelectedNodes.Clear();

        Debug.Log($"<color=green>已移除 {count} 个对象</color>");
    }

    private void QuickCopy()
    {
        // 获取两侧的选中对象
        var leftObj = parentWindow.leftPanel.GetSelectedObject();
        var rightObj = parentWindow.rightPanel.GetSelectedObject();

        if (leftObj == null || rightObj == null)
        {
            Debug.LogWarning("请在左右两边都选择一个对象");
            return;
        }

        // 直接执行copy，不绑定
        CopyComponentTool.Copy(
            leftObj.gameObject,
            rightObj.gameObject,
            false, // isExceptRectTransform
            false, // isNeedRemove
            true // preserveAnchorPivot - 保留anchor和pivot
        );

        Debug.Log($"<color=green>快速Copy成功: {leftObj.name} → {rightObj.name}（保留Anchor和Pivot）</color>");
    }

    private void OpenBatchProcessWindow()
    {
        // 打开批处理窗口
        UIBatchProcessWindow.ShowWindow(componentNodes, parentWindow);
    }
}

public class UIComponentNode
{
    public GameObject gameObject;
    public Component component;
    public UIComponentType type;
    public string name;
}

public enum UIComponentType
{
    Image,
    Text
}

// 批处理窗口
public class UIBatchProcessWindow : EditorWindow
{
    private List<UIComponentNode> targetNodes;
    private UISkinChangeWindow parentWindow;

    // Image相关设置
    private bool enableImageColor = false;
    private Color imageColor = Color.white;

    // Text相关设置
    private bool enableTextColor = false;
    private Color textColor = Color.white;
    private bool enableTextSize = false;
    private int textSize = 14;

    private Vector2 scrollPos;

    public static void ShowWindow(List<UIComponentNode> nodes, UISkinChangeWindow parent)
    {
        var window = GetWindow<UIBatchProcessWindow>("批处理UI对象");
        window.minSize = new Vector2(400, 400);
        window.targetNodes = new List<UIComponentNode>(nodes);
        window.parentWindow = parent;
        window.Show();
    }

    private void OnGUI()
    {
        if (targetNodes == null || targetNodes.Count == 0)
        {
            EditorGUILayout.HelpBox("没有可处理的对象", MessageType.Warning);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 统计信息
        int imageCount = targetNodes.Count(n => n.type == UIComponentType.Image);
        int textCount = targetNodes.Count(n => n.type == UIComponentType.Text);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("对象统计", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Image: {imageCount} 个");
        EditorGUILayout.LabelField($"Text: {textCount} 个");
        EditorGUILayout.LabelField($"总计: {targetNodes.Count} 个");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Image设置区域
        if (imageCount > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Image 批处理设置", EditorStyles.boldLabel);

            enableImageColor = EditorGUILayout.ToggleLeft("设置Image颜色", enableImageColor);
            if (enableImageColor)
            {
                EditorGUI.indentLevel++;
                imageColor = EditorGUILayout.ColorField("颜色", imageColor);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        // Text设置区域
        if (textCount > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Text 批处理设置", EditorStyles.boldLabel);

            enableTextColor = EditorGUILayout.ToggleLeft("设置Text颜色", enableTextColor);
            if (enableTextColor)
            {
                EditorGUI.indentLevel++;
                textColor = EditorGUILayout.ColorField("颜色", textColor);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            enableTextSize = EditorGUILayout.ToggleLeft("设置Text字体大小", enableTextSize);
            if (enableTextSize)
            {
                EditorGUI.indentLevel++;
                textSize = EditorGUILayout.IntSlider("字体大小", textSize, 8, 100);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(20);

        // 应用按钮
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Color originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
        if (GUILayout.Button("应用批处理", GUILayout.Height(35), GUILayout.Width(150)))
        {
            ApplyBatchProcess();
        }

        GUI.backgroundColor = originalBgColor;

        if (GUILayout.Button("取消", GUILayout.Height(35), GUILayout.Width(100)))
        {
            Close();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void ApplyBatchProcess()
    {
        int processedCount = 0;

        // 记录Undo
        Undo.RecordObjects(targetNodes.Select(n => n.gameObject).ToArray(), "批处理UI对象");

        foreach (var node in targetNodes)
        {
            if (node.type == UIComponentType.Image && enableImageColor)
            {
                var image = node.component as Image;
                if (image != null)
                {
                    image.color = imageColor;
                    EditorUtility.SetDirty(image);
                    processedCount++;
                }
            }
            else if (node.type == UIComponentType.Text)
            {
                var text = node.component as Text;
                if (text != null)
                {
                    bool modified = false;

                    if (enableTextColor)
                    {
                        text.color = textColor;
                        modified = true;
                    }

                    if (enableTextSize)
                    {
                        text.fontSize = textSize;
                        modified = true;
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(text);
                        processedCount++;
                    }
                }
            }
        }

        Debug.Log($"<color=green>批处理完成！已处理 {processedCount} 个对象</color>");

        // 刷新父窗口
        if (parentWindow != null)
        {
            parentWindow.Repaint();
        }

        Close();
    }
}

