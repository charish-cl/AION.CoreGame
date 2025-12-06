using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;

public class XMLUIGeneratorWindow : EditorWindow
{
    private TextAsset selectedXML;
    private string uiName = "";
    private Vector2 scrollPosition;
    private string previewContent = "";
    private bool autoCreateCanvas = true;
    private Canvas targetCanvas;

    [MenuItem("Tools/XML UI Generator")]
    public static void ShowWindow()
    {
        XMLUIGeneratorWindow window = GetWindow<XMLUIGeneratorWindow>("XML UI 生成器");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("XML UI 生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // XML 文件选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("XML 文件:", GUILayout.Width(80));
        selectedXML = (TextAsset)EditorGUILayout.ObjectField(selectedXML, typeof(TextAsset), false);
        EditorGUILayout.EndHorizontal();

        if (selectedXML != null && string.IsNullOrEmpty(uiName))
        {
            uiName = selectedXML.name;
        }

        GUILayout.Space(10);

        // UI 名称
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("UI 名称:", GUILayout.Width(80));
        uiName = EditorGUILayout.TextField(uiName);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Canvas 设置
        autoCreateCanvas = EditorGUILayout.Toggle("自动创建 Canvas", autoCreateCanvas);
        if (!autoCreateCanvas)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标 Canvas:", GUILayout.Width(80));
            targetCanvas = (Canvas)EditorGUILayout.ObjectField(targetCanvas, typeof(Canvas), true);
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // XML 预览
        if (selectedXML != null)
        {
            GUILayout.Label("XML 预览:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.TextArea(selectedXML.text, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        // 生成按钮
        GUI.enabled = selectedXML != null && !string.IsNullOrEmpty(uiName);
        if (GUILayout.Button("生成 UI", GUILayout.Height(40)))
        {
            GenerateUI();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // 帮助信息
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 选择 Resources/UI 文件夹下的 XML 文件\n" +
            "2. 输入 UI 名称（默认使用文件名）\n" +
            "3. 选择是否自动创建 Canvas\n" +
            "4. 点击「生成 UI」按钮\n\n" +
            "生成的 UI 会出现在当前场景的 Hierarchy 中",
            MessageType.Info);
    }

    void GenerateUI()
    {
        if (selectedXML == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择 XML 文件！", "确定");
            return;
        }

        try
        {
            Canvas canvas;
            if (autoCreateCanvas)
            {
                // 创建新的 Canvas
                GameObject canvasObj = new GameObject(uiName);
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                // 设置 CanvasScaler 为自适应全屏，设计分辨率 750x1334
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(750, 1334);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();

                // 确保有 EventSystem
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                Undo.RegisterCreatedObjectUndo(canvasObj, "Create UI Canvas");
            }
            else
            {
                if (targetCanvas == null)
                {
                    EditorUtility.DisplayDialog("错误", "请指定目标 Canvas！", "确定");
                    return;
                }
                canvas = targetCanvas;
            }

            // 解析并生成 UI
            XDocument doc = XDocument.Parse(selectedXML.text);
            XMLUIGenerator generator = new XMLUIGenerator();

            // 直接创建所有根元素到 Canvas，不再需要检查 Panel
            var rootElements = doc.Root.Elements().ToList();
            foreach (var element in rootElements)
            {
                generator.CreateUIElement(element, canvas.transform);
            }

            // 选中生成的 Canvas
            Selection.activeGameObject = canvas.gameObject;
            EditorGUIUtility.PingObject(canvas.gameObject);

            Debug.Log($"UI 生成成功：{uiName}");
            EditorUtility.DisplayDialog("成功", $"UI 已生成：{uiName}\n\n可以在 Hierarchy 中查看", "确定");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("生成失败", $"生成 UI 时出错：\n{e.Message}", "确定");
            Debug.LogError($"生成 UI 失败：{e}");
        }
    }
}

// UI 生成器核心类（从 XMLUILoader 提取，适配 Editor 使用）
public class XMLUIGenerator
{
    private Dictionary<string, string> prefabAliases = new Dictionary<string, string>();
    private List<string> prefabSearchPaths = new List<string>();
    private List<PrefabMatchRule> prefabMatchRules = new List<PrefabMatchRule>();
    private bool configLoaded = false;

    private void LoadPrefabConfig()
    {
        if (configLoaded) return;
        configLoaded = true;

        string configPath = "Assets/Editor/XMLUIGenerator/XMLUIGeneratorPrefabConfig.json";
        if (File.Exists(configPath))
        {
            try
            {
                string json = File.ReadAllText(configPath);
                
                // 使用简单的 JSON 解析（Unity JsonUtility 不支持 Dictionary）
                // 解析 prefabSearchPaths
                var searchPathsMatch = System.Text.RegularExpressions.Regex.Match(json, @"""prefabSearchPaths""\s*:\s*\[(.*?)\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (searchPathsMatch.Success)
                {
                    string pathsStr = searchPathsMatch.Groups[1].Value;
                    var pathMatches = System.Text.RegularExpressions.Regex.Matches(pathsStr, @"""([^""]+)""");
                    foreach (System.Text.RegularExpressions.Match match in pathMatches)
                    {
                        prefabSearchPaths.Add(match.Groups[1].Value);
                    }
                }
                
                // 解析 prefabAliases
                var aliasesMatch = System.Text.RegularExpressions.Regex.Match(json, @"""prefabAliases""\s*:\s*\{(.*?)\}", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (aliasesMatch.Success)
                {
                    string aliasesStr = aliasesMatch.Groups[1].Value;
                    var aliasMatches = System.Text.RegularExpressions.Regex.Matches(aliasesStr, @"""([^""]+)""\s*:\s*""([^""]+)""");
                    foreach (System.Text.RegularExpressions.Match match in aliasMatches)
                    {
                        prefabAliases[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
                
                // 解析 prefabMatchRules
                var rulesMatch = System.Text.RegularExpressions.Regex.Match(json, @"""prefabMatchRules""\s*:\s*\[(.*?)\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (rulesMatch.Success)
                {
                    string rulesStr = rulesMatch.Groups[1].Value;
                    // 匹配每个规则对象
                    var ruleMatches = System.Text.RegularExpressions.Regex.Matches(rulesStr, @"\{[^{}]*""matchPatterns""\s*:\s*\[(.*?)\][^{}]*\}", System.Text.RegularExpressions.RegexOptions.Singleline);
                    foreach (System.Text.RegularExpressions.Match ruleMatch in ruleMatches)
                    {
                        // 匹配 matchPatterns 数组中的每个模式
                        var patternMatches = System.Text.RegularExpressions.Regex.Matches(ruleMatch.Groups[1].Value, @"\{[^{}]*""nameContains""\s*:\s*\[(.*?)\][^{}]*""prefabName""\s*:\s*""([^""]+)""[^{}]*\}", System.Text.RegularExpressions.RegexOptions.Singleline);
                        foreach (System.Text.RegularExpressions.Match patternMatch in patternMatches)
                        {
                            string nameContainsStr = patternMatch.Groups[1].Value;
                            string prefabName = patternMatch.Groups[2].Value;
                            
                            // 解析 nameContains 数组
                            var nameMatches = System.Text.RegularExpressions.Regex.Matches(nameContainsStr, @"""([^""]+)""");
                            List<string> nameContains = new List<string>();
                            foreach (System.Text.RegularExpressions.Match nameMatch in nameMatches)
                            {
                                nameContains.Add(nameMatch.Groups[1].Value);
                            }
                            
                            if (nameContains.Count > 0)
                            {
                                prefabMatchRules.Add(new PrefabMatchRule
                                {
                                    matchPatterns = new List<MatchPattern>
                                    {
                                        new MatchPattern
                                        {
                                            nameContains = nameContains,
                                            prefabName = prefabName
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"加载预制体配置文件失败：{e.Message}");
            }
        }
        
        // 默认搜索路径
        if (prefabSearchPaths.Count == 0)
        {
            prefabSearchPaths.AddRange(new[]
            {
                "Assets/Game/UIForm/Common",
                "Assets/Game/UIComponent",
                "Assets/Game/Prefab"
            });
        }
    }

    private GameObject LoadPrefab(string prefabName)
    {
        LoadPrefabConfig();

        // 1. 先检查别名
        if (prefabAliases.ContainsKey(prefabName))
        {
            string path = prefabAliases[prefabName];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }
        }

        // 2. 尝试直接路径
        if (prefabName.StartsWith("Assets/"))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabName);
            if (prefab != null)
            {
                return prefab;
            }
        }

        // 3. 在搜索路径中查找
        foreach (string searchPath in prefabSearchPaths)
        {
            string fullPath = $"{searchPath}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            if (prefab != null)
            {
                return prefab;
            }
        }

        // 4. 使用 AssetDatabase.FindAssets 全局搜索
        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.Equals(prefabName, StringComparison.OrdinalIgnoreCase))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    return prefab;
                }
            }
        }

        return null;
    }

    private string GetPrefabRefByRule(string elementName, string comment)
    {
        LoadPrefabConfig();
        
        // 检查匹配规则
        foreach (var rule in prefabMatchRules)
        {
            foreach (var pattern in rule.matchPatterns)
            {
                // 检查名称是否包含关键词
                foreach (var keyword in pattern.nameContains)
                {
                    if (elementName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (comment != null && comment.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return pattern.prefabName;
                    }
                }
            }
        }
        
        return null;
    }

    private string GetPreviousComment(XElement xml)
    {
        // 获取前一个注释节点
        XNode previousNode = xml.PreviousNode;
        while (previousNode != null)
        {
            if (previousNode.NodeType == System.Xml.XmlNodeType.Comment)
            {
                return ((XComment)previousNode).Value;
            }
            previousNode = previousNode.PreviousNode;
        }
        return null;
    }

    private class PrefabMatchRule
    {
        public List<MatchPattern> matchPatterns = new List<MatchPattern>();
    }

    private class MatchPattern
    {
        public List<string> nameContains = new List<string>();
        public string prefabName;
    }

    private TMP_FontAsset GetDefaultTMPFont()
    {
        TMP_FontAsset defaultFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
        if (defaultFont == null)
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
        return defaultFont;
    }

    public GameObject CreateUIElement(XElement xml, Transform parent)
    {
        string elementType = xml.Name.LocalName;
        string name = xml.Attribute("Name")?.Value ?? elementType;
        string refPrefab = xml.Attribute("ref")?.Value;
        
        // 如果没有指定 ref，尝试根据规则自动匹配
        if (string.IsNullOrEmpty(refPrefab))
        {
            // 获取注释（前一个注释节点）
            string comment = GetPreviousComment(xml);
            refPrefab = GetPrefabRefByRule(name, comment);
            if (!string.IsNullOrEmpty(refPrefab))
            {
                Debug.Log($"根据规则自动匹配预制体：{name} -> {refPrefab}");
            }
        }

        // 如果指定了 ref，使用预制体
        if (!string.IsNullOrEmpty(refPrefab))
        {
            GameObject prefab = LoadPrefab(refPrefab);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                instance.name = name;
                
                // 处理位置和尺寸
                HandleSpecialProperties(instance, xml);
                
                // 注册 Undo
                Undo.RegisterCreatedObjectUndo(instance, "Create UI Element from Prefab");
                
                Debug.Log($"已实例化预制体：{refPrefab} -> {name}");
                return instance;
            }
            else
            {
                Debug.LogWarning($"找不到预制体：{refPrefab}，将创建新对象");
            }
        }

        // 命名转换：只有 Button 需要自动添加 m_btn 前缀
        string unityName = ConvertName(name, elementType);

        GameObject go = new GameObject(unityName);
        go.transform.SetParent(parent, false);

        // 添加 RectTransform
        RectTransform rect = go.AddComponent<RectTransform>();

        // 添加对应组件并创建必要的子结构
        Component component = AddComponentByType(go, elementType, xml);

        // 反射设置所有属性
        if (component != null)
        {
            SetPropertiesByReflection(component, xml);
        }

        // RectTransform 也用反射设置
        SetPropertiesByReflection(rect, xml);

        // 处理特殊属性（坐标转换等）
        HandleSpecialProperties(go, xml);

        // 递归创建子元素
        foreach (var child in xml.Elements())
        {
            CreateUIElement(child, go.transform);
        }

        // 注册 Undo
        Undo.RegisterCreatedObjectUndo(go, "Create UI Element");

        return go;
    }


    string ConvertName(string name, string elementType)
    {
        if (name.StartsWith("m_"))
            return name;

        if (elementType == "Button")
        {
            string baseName = name;
            baseName = Regex.Replace(baseName, @"Btn$", "", RegexOptions.IgnoreCase);
            baseName = Regex.Replace(baseName, @"Button$", "", RegexOptions.IgnoreCase);
            return $"m_btn_{baseName}";
        }

        return name;
    }

    Component AddComponentByType(GameObject go, string typeName, XElement xml)
    {
        switch (typeName)
        {
            case "Panel":
                return SetupPanel(go);
            case "Image":
                return SetupImage(go);
            case "Text":
                return SetupTextTMP(go);
            case "Button":
                return SetupButton(go, xml);
            case "InputField":
                return SetupInputField(go, xml);
            case "Slider":
                return SetupSlider(go, xml);
            case "Toggle":
                return SetupToggle(go, xml);
            case "Scrollbar":
                return SetupScrollbar(go);
            case "Dropdown":
                return SetupDropdown(go, xml);
            case "ScrollRect":
                return SetupScrollRect(go);
            case "Grid":
            case "GridLayoutGroup":
                return SetupGridLayoutGroup(go, xml);
            case "HorizontalLayout":
            case "HorizontalLayoutGroup":
                return SetupHorizontalLayoutGroup(go, xml);
            case "VerticalLayout":
            case "VerticalLayoutGroup":
                return SetupVerticalLayoutGroup(go, xml);
            default:
                Type type = Type.GetType($"UnityEngine.UI.{typeName}, UnityEngine.UI");
                if (type != null)
                    return go.AddComponent(type);
                return null;
        }
    }

    Image SetupPanel(GameObject go)
    {
        Image img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.4f);
        return img;
    }

    Image SetupImage(GameObject go)
    {
        return go.AddComponent<Image>();
    }

    TextMeshProUGUI SetupTextTMP(GameObject go)
    {
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        TMP_FontAsset defaultFont = GetDefaultTMPFont();
        if (defaultFont != null)
        {
            text.font = defaultFont;
        }
        return text;
    }

    Button SetupButton(GameObject go, XElement xml)
    {
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        string buttonText = xml.Attribute("Text")?.Value ?? "Button";
        int fontSize = int.Parse(xml.Attribute("FontSize")?.Value ?? "14");
        CreateButtonText(go, buttonText, fontSize);

        return btn;
    }

    TMP_InputField SetupInputField(GameObject go, XElement xml)
    {
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        TMP_InputField inputField = go.AddComponent<TMP_InputField>();

        // Placeholder
        GameObject placeholder = new GameObject("m_text_Placeholder");
        placeholder.transform.SetParent(go.transform, false);
        RectTransform phRect = placeholder.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        phRect.offsetMin = new Vector2(10, 6);
        phRect.offsetMax = new Vector2(-10, -7);

        TextMeshProUGUI phText = placeholder.AddComponent<TextMeshProUGUI>();
        phText.text = "Enter text...";
        phText.fontStyle = FontStyles.Italic;
        phText.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        TMP_FontAsset defaultFont = GetDefaultTMPFont();
        if (defaultFont != null)
        {
            phText.font = defaultFont;
        }

        // Text
        GameObject textObj = new GameObject("m_text_Text");
        textObj.transform.SetParent(go.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 6);
        textRect.offsetMax = new Vector2(-10, -7);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        text.richText = false;
        if (defaultFont != null)
        {
            text.font = defaultFont;
        }

        inputField.textComponent = text;
        inputField.placeholder = phText;

        return inputField;
    }

    Slider SetupSlider(GameObject go, XElement xml)
    {
        Slider slider = go.AddComponent<Slider>();

        // Background
        GameObject bg = new GameObject("m_img_Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.sizeDelta = new Vector2(0, 0);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("m_rect_FillArea");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        // Fill
        GameObject fill = new GameObject("m_img_Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(10, 0);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.6f, 1f, 1f);

        // Handle Slide Area
        GameObject handleSlideArea = new GameObject("m_rect_HandleSlideArea");
        handleSlideArea.transform.SetParent(go.transform, false);
        RectTransform handleSlideRect = handleSlideArea.AddComponent<RectTransform>();
        handleSlideRect.sizeDelta = new Vector2(-20, 0);
        handleSlideRect.anchorMin = new Vector2(0, 0);
        handleSlideRect.anchorMax = new Vector2(1, 1);

        // Handle
        GameObject handle = new GameObject("m_img_Handle");
        handle.transform.SetParent(handleSlideArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    Toggle SetupToggle(GameObject go, XElement xml)
    {
        Toggle toggle = go.AddComponent<Toggle>();

        // Background
        GameObject bg = new GameObject("m_img_Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.sizeDelta = new Vector2(20, 20);
        bgRect.anchoredPosition = new Vector2(10, -10);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.white;

        // Checkmark
        GameObject checkmark = new GameObject("m_img_Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = Vector2.zero;
        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = new Color(0.2f, 0.6f, 1f, 1f);

        // Label
        string labelText = xml.Attribute("Text")?.Value ?? "Toggle";
        GameObject label = new GameObject("m_text_Label");
        label.transform.SetParent(go.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(23, 1);
        labelRect.offsetMax = new Vector2(-5, -2);

        TextMeshProUGUI labelComp = label.AddComponent<TextMeshProUGUI>();
        labelComp.text = labelText;
        labelComp.color = Color.black;
        TMP_FontAsset defaultFont = GetDefaultTMPFont();
        if (defaultFont != null)
        {
            labelComp.font = defaultFont;
        }

        toggle.targetGraphic = bgImg;
        toggle.graphic = checkImg;
        toggle.isOn = false;

        return toggle;
    }

    Scrollbar SetupScrollbar(GameObject go)
    {
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        Scrollbar scrollbar = go.AddComponent<Scrollbar>();

        GameObject slidingArea = new GameObject("m_rect_SlidingArea");
        slidingArea.transform.SetParent(go.transform, false);
        RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
        slidingRect.sizeDelta = new Vector2(-20, -20);
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;

        GameObject handle = new GameObject("m_img_Handle");
        handle.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImg;

        return scrollbar;
    }

    TMP_Dropdown SetupDropdown(GameObject go, XElement xml)
    {
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();

        GameObject label = new GameObject("m_text_Label");
        label.transform.SetParent(go.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 6);
        labelRect.offsetMax = new Vector2(-25, -7);

        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "Option A";
        labelText.color = Color.black;
        TMP_FontAsset defaultFont = GetDefaultTMPFont();
        if (defaultFont != null)
        {
            labelText.font = defaultFont;
        }

        dropdown.captionText = labelText;

        return dropdown;
    }

    ScrollRect SetupScrollRect(GameObject go)
    {
        Image img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.1f);
        ScrollRect scrollRect = go.AddComponent<ScrollRect>();

        GameObject viewport = new GameObject("m_rect_Viewport");
        viewport.transform.SetParent(go.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = Color.white;
        viewport.AddComponent<Mask>();

        GameObject content = new GameObject("m_rect_Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.sizeDelta = new Vector2(0, 300);
        contentRect.pivot = new Vector2(0.5f, 1);

        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;

        return scrollRect;
    }

    GridLayoutGroup SetupGridLayoutGroup(GameObject go, XElement xml)
    {
        GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
        
        // 从XML读取cellSize
        float? cellSizeX = GetFloatAttr(xml, "cellSizeX");
        float? cellSizeY = GetFloatAttr(xml, "cellSizeY");
        grid.cellSize = new Vector2(cellSizeX ?? 100, cellSizeY ?? 100);
        
        // 从XML读取spacing
        float? spacingX = GetFloatAttr(xml, "spacingX");
        float? spacingY = GetFloatAttr(xml, "spacingY");
        grid.spacing = new Vector2(spacingX ?? 10, spacingY ?? 10);
        
        // 从XML读取constraint
        string constraint = xml.Attribute("constraint")?.Value;
        if (!string.IsNullOrEmpty(constraint))
        {
            if (Enum.TryParse<GridLayoutGroup.Constraint>(constraint, true, out var constraintValue))
            {
                grid.constraint = constraintValue;
            }
        }
        else
        {
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
        }
        
        // 从XML读取constraintCount
        float? constraintCount = GetFloatAttr(xml, "constraintCount");
        if (constraintCount.HasValue)
        {
            grid.constraintCount = (int)constraintCount.Value;
        }
        
        // 默认设置
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        
        return grid;
    }

    HorizontalLayoutGroup SetupHorizontalLayoutGroup(GameObject go, XElement xml)
    {
        HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
        
        // 从XML读取spacing
        float? spacing = GetFloatAttr(xml, "spacing");
        if (spacing.HasValue)
        {
            layout.spacing = spacing.Value;
        }
        
        // 从XML读取padding
        float? paddingLeft = GetFloatAttr(xml, "paddingLeft");
        float? paddingRight = GetFloatAttr(xml, "paddingRight");
        float? paddingTop = GetFloatAttr(xml, "paddingTop");
        float? paddingBottom = GetFloatAttr(xml, "paddingBottom");
        if (paddingLeft.HasValue || paddingRight.HasValue || paddingTop.HasValue || paddingBottom.HasValue)
        {
            layout.padding.left = (int)(paddingLeft ?? 0);
            layout.padding.right = (int)(paddingRight ?? 0);
            layout.padding.top = (int)(paddingTop ?? 0);
            layout.padding.bottom = (int)(paddingBottom ?? 0);
        }
        
        // 从XML读取childAlignment
        string childAlignment = xml.Attribute("childAlignment")?.Value;
        if (!string.IsNullOrEmpty(childAlignment))
        {
            if (Enum.TryParse<TextAnchor>(childAlignment, true, out var alignment))
            {
                layout.childAlignment = alignment;
            }
        }
        
        // 从XML读取childControlWidth/Height
        string childControlWidth = xml.Attribute("childControlWidth")?.Value;
        if (!string.IsNullOrEmpty(childControlWidth) && bool.TryParse(childControlWidth, out var controlWidth))
        {
            layout.childControlWidth = controlWidth;
        }
        
        string childControlHeight = xml.Attribute("childControlHeight")?.Value;
        if (!string.IsNullOrEmpty(childControlHeight) && bool.TryParse(childControlHeight, out var controlHeight))
        {
            layout.childControlHeight = controlHeight;
        }
        
        // 从XML读取childForceExpandWidth/Height
        string childForceExpandWidth = xml.Attribute("childForceExpandWidth")?.Value;
        if (!string.IsNullOrEmpty(childForceExpandWidth) && bool.TryParse(childForceExpandWidth, out var expandWidth))
        {
            layout.childForceExpandWidth = expandWidth;
        }
        
        string childForceExpandHeight = xml.Attribute("childForceExpandHeight")?.Value;
        if (!string.IsNullOrEmpty(childForceExpandHeight) && bool.TryParse(childForceExpandHeight, out var expandHeight))
        {
            layout.childForceExpandHeight = expandHeight;
        }
        
        // 默认设置：HorizontalLayout 一般不 ControlSize，一般 forceExpand
        if (string.IsNullOrEmpty(xml.Attribute("childControlWidth")?.Value))
            layout.childControlWidth = false;
        if (string.IsNullOrEmpty(xml.Attribute("childControlHeight")?.Value))
            layout.childControlHeight = false;
        if (string.IsNullOrEmpty(xml.Attribute("childForceExpandWidth")?.Value))
            layout.childForceExpandWidth = true;
        if (string.IsNullOrEmpty(xml.Attribute("childForceExpandHeight")?.Value))
            layout.childForceExpandHeight = false;
        
        return layout;
    }

    VerticalLayoutGroup SetupVerticalLayoutGroup(GameObject go, XElement xml)
    {
        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        
        // 从XML读取spacing
        float? spacing = GetFloatAttr(xml, "spacing");
        if (spacing.HasValue)
        {
            layout.spacing = spacing.Value;
        }
        
        // 从XML读取padding
        float? paddingLeft = GetFloatAttr(xml, "paddingLeft");
        float? paddingRight = GetFloatAttr(xml, "paddingRight");
        float? paddingTop = GetFloatAttr(xml, "paddingTop");
        float? paddingBottom = GetFloatAttr(xml, "paddingBottom");
        if (paddingLeft.HasValue || paddingRight.HasValue || paddingTop.HasValue || paddingBottom.HasValue)
        {
            layout.padding.left = (int)(paddingLeft ?? 0);
            layout.padding.right = (int)(paddingRight ?? 0);
            layout.padding.top = (int)(paddingTop ?? 0);
            layout.padding.bottom = (int)(paddingBottom ?? 0);
        }
        
        // 从XML读取childAlignment
        string childAlignment = xml.Attribute("childAlignment")?.Value;
        if (!string.IsNullOrEmpty(childAlignment))
        {
            if (Enum.TryParse<TextAnchor>(childAlignment, true, out var alignment))
            {
                layout.childAlignment = alignment;
            }
        }
        
        // 从XML读取childControlWidth/Height
        string childControlWidth = xml.Attribute("childControlWidth")?.Value;
        if (!string.IsNullOrEmpty(childControlWidth) && bool.TryParse(childControlWidth, out var controlWidth))
        {
            layout.childControlWidth = controlWidth;
        }
        
        string childControlHeight = xml.Attribute("childControlHeight")?.Value;
        if (!string.IsNullOrEmpty(childControlHeight) && bool.TryParse(childControlHeight, out var controlHeight))
        {
            layout.childControlHeight = controlHeight;
        }
        
        // 从XML读取childForceExpandWidth/Height
        string childForceExpandWidth = xml.Attribute("childForceExpandWidth")?.Value;
        if (!string.IsNullOrEmpty(childForceExpandWidth) && bool.TryParse(childForceExpandWidth, out var expandWidth))
        {
            layout.childForceExpandWidth = expandWidth;
        }
        
        string childForceExpandHeight = xml.Attribute("childForceExpandHeight")?.Value;
        if (!string.IsNullOrEmpty(childForceExpandHeight) && bool.TryParse(childForceExpandHeight, out var expandHeight))
        {
            layout.childForceExpandHeight = expandHeight;
        }
        
        // 默认设置：VerticalLayout 一般不 ControlSize，一般 forceExpand Height
        if (string.IsNullOrEmpty(xml.Attribute("childControlWidth")?.Value))
            layout.childControlWidth = false;
        if (string.IsNullOrEmpty(xml.Attribute("childControlHeight")?.Value))
            layout.childControlHeight = false;
        if (string.IsNullOrEmpty(xml.Attribute("childForceExpandWidth")?.Value))
            layout.childForceExpandWidth = false;
        if (string.IsNullOrEmpty(xml.Attribute("childForceExpandHeight")?.Value))
            layout.childForceExpandHeight = true;
        
        return layout;
    }

    void CreateButtonText(GameObject buttonGo, string text, int fontSize)
    {
        GameObject textObj = new GameObject("m_text_Text");
        textObj.transform.SetParent(buttonGo.transform, false);
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        textComp.fontSize = fontSize;
        TMP_FontAsset defaultFont = GetDefaultTMPFont();
        if (defaultFont != null)
        {
            textComp.font = defaultFont;
        }

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    void SetPropertiesByReflection(Component component, XElement xml)
    {
        Type type = component.GetType();

        foreach (var attr in xml.Attributes())
        {
            string propertyName = attr.Name.LocalName;
            string value = attr.Value;

            if (propertyName == "Name") continue;

            PropertyInfo property = type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null && property.CanWrite)
            {
                try
                {
                    object convertedValue = ConvertValue(value, property.PropertyType);
                    property.SetValue(component, convertedValue);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"无法设置属性 {propertyName}: {e.Message}");
                }
            }
            else
            {
                FieldInfo field = type.GetField(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (field != null)
                {
                    try
                    {
                        object convertedValue = ConvertValue(value, field.FieldType);
                        field.SetValue(component, convertedValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"无法设置字段 {propertyName}: {e.Message}");
                    }
                }
            }
        }
    }

    Sprite LoadSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return null;

        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
            return sprite;

        // AssetDatabase 路径加载
        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
            return sprite;

        // 搜索文件名
        string[] guids = AssetDatabase.FindAssets($"t:Sprite {spritePath}");
        if (guids.Length == 0)
        {
            guids = AssetDatabase.FindAssets($"t:Texture2D {spritePath}");
        }

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (fileName.Equals(spritePath, StringComparison.OrdinalIgnoreCase))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                    return sprite;

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex != null)
                {
                    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                    foreach (var obj in sprites)
                    {
                        if (obj is Sprite)
                            return obj as Sprite;
                    }
                }
            }
        }

        return null;
    }

    object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        if (targetType == typeof(Color))
        {
            if (ColorUtility.TryParseHtmlString(value, out Color color))
                return color;
            return Color.white;
        }

        if (targetType == typeof(Vector2))
        {
            string[] parts = value.Split(',');
            if (parts.Length == 2)
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
        }

        if (targetType == typeof(Vector3))
        {
            string[] parts = value.Split(',');
            if (parts.Length == 3)
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        if (targetType == typeof(Sprite))
        {
            return LoadSprite(value);
        }

        if (targetType == typeof(Font))
        {
            Font font = Resources.Load<Font>(value);
            return font ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        if (targetType == typeof(TMP_FontAsset))
        {
            TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>(value);
            if (fontAsset == null)
            {
                fontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
            }
            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            return fontAsset;
        }

        // TextMeshPro alignment 特殊处理
        if (targetType == typeof(TextAlignmentOptions))
        {
            return ConvertTextAlignment(value);
        }

        if (targetType.IsEnum)
        {
            try
            {
                return Enum.Parse(targetType, value, true);
            }
            catch
            {
                // 如果直接解析失败，尝试查找匹配的枚举值
                var enumValues = Enum.GetValues(targetType);
                foreach (var enumValue in enumValues)
                {
                    if (enumValue.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return enumValue;
                    }
                }
                throw;
            }
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    TextAlignmentOptions ConvertTextAlignment(string alignment)
    {
        // 映射常见的对齐方式到 TextAlignmentOptions
        switch (alignment.ToLower())
        {
            case "topleft":
            case "upperleft":
                return TextAlignmentOptions.TopLeft;
            case "topcenter":
            case "uppercenter":
                return TextAlignmentOptions.Top;
            case "topright":
            case "upperright":
                return TextAlignmentOptions.TopRight;
            case "middleleft":
            case "centerleft":
                return TextAlignmentOptions.MidlineLeft;
            case "middlecenter":
            case "center":
                return TextAlignmentOptions.Midline;
            case "middleright":
            case "centerright":
                return TextAlignmentOptions.MidlineRight;
            case "bottomleft":
            case "lowerleft":
                return TextAlignmentOptions.BottomLeft;
            case "bottomcenter":
            case "lowercenter":
                return TextAlignmentOptions.Bottom;
            case "bottomright":
            case "lowerright":
                return TextAlignmentOptions.BottomRight;
            default:
                // 尝试直接解析
                if (Enum.TryParse<TextAlignmentOptions>(alignment, true, out var result))
                {
                    return result;
                }
                return TextAlignmentOptions.Midline;
        }
    }

    void HandleSpecialProperties(GameObject go, XElement xml)
    {
        RectTransform rect = go.GetComponent<RectTransform>();

        // 先处理 Anchor（因为会影响后续的坐标计算）
        string anchor = xml.Attribute("Anchor")?.Value;
        bool anchorWasSet = false;
        if (!string.IsNullOrEmpty(anchor))
        {
            SetAnchor(rect, anchor);
            anchorWasSet = true;
        }

        // Width, Height - 需要在设置位置之前处理
        float? width = GetFloatAttr(xml, "Width");
        float? height = GetFloatAttr(xml, "Height");

        // 判断是否是拉伸锚点（anchorMin != anchorMax）
        bool isStretchAnchor = rect.anchorMin != rect.anchorMax;

        if (width.HasValue || height.HasValue)
        {
            if (isStretchAnchor)
            {
                // 拉伸锚点使用 offsetMin 和 offsetMax
                Vector2 offsetMin = rect.offsetMin;
                Vector2 offsetMax = rect.offsetMax;

                if (width.HasValue)
                {
                    float currentWidth = rect.rect.width;
                    float delta = width.Value - currentWidth;
                    offsetMin.x -= delta * rect.pivot.x;
                    offsetMax.x += delta * (1 - rect.pivot.x);
                }

                if (height.HasValue)
                {
                    float currentHeight = rect.rect.height;
                    float delta = height.Value - currentHeight;
                    offsetMin.y -= delta * rect.pivot.y;
                    offsetMax.y += delta * (1 - rect.pivot.y);
                }

                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }
            else
            {
                // 点锚点使用 sizeDelta
                Vector2 size = rect.sizeDelta;
                if (width.HasValue) size.x = width.Value;
                if (height.HasValue) size.y = height.Value;
                rect.sizeDelta = size;
            }
        }

        // X, Y - 在设置anchor和size之后处理
        float? x = GetFloatAttr(xml, "X");
        float? y = GetFloatAttr(xml, "Y");
        if (x.HasValue || y.HasValue)
        {
            if (isStretchAnchor)
            {
                // 拉伸锚点：X/Y 表示相对于父节点的偏移
                Rect parentRect = rect.parent != null ? ((RectTransform)rect.parent).rect : new Rect(0, 0, Screen.width, Screen.height);

                Vector2 offsetMin = rect.offsetMin;
                Vector2 offsetMax = rect.offsetMax;

                // 计算当前中心位置
                float currentCenterX = offsetMin.x + (offsetMax.x - offsetMin.x) * 0.5f;
                float currentCenterY = offsetMin.y + (offsetMax.y - offsetMin.y) * 0.5f;

                if (x.HasValue)
                {
                    // 计算目标中心位置相对于锚点的偏移
                    float targetOffsetX = x.Value - parentRect.width * (rect.anchorMin.x + rect.anchorMax.x) * 0.5f;
                    float deltaX = targetOffsetX - currentCenterX;
                    offsetMin.x += deltaX;
                    offsetMax.x += deltaX;
                }

                if (y.HasValue)
                {
                    // Y轴翻转，并计算相对于锚点的偏移
                    float targetOffsetY = -y.Value - parentRect.height * (rect.anchorMin.y + rect.anchorMax.y) * 0.5f;
                    float deltaY = targetOffsetY - currentCenterY;
                    offsetMin.y += deltaY;
                    offsetMax.y += deltaY;
                }

                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }
            else
            {
                // 点锚点：使用 anchoredPosition
                Vector2 pos = rect.anchoredPosition;

                if (x.HasValue)
                {
                    pos.x = x.Value;
                }

                if (y.HasValue)
                {
                    // 根据Anchor位置调整Y坐标计算
                    // 在WPF风格的XML中，Y=-50 表示向下偏移50像素
                    // 但在Unity中，需要根据Anchor的pivot来调整

                    // 获取当前anchor的位置信息
                    Vector2 anchorPos = rect.anchorMin; // 对于点锚点，anchorMin == anchorMax

                    if (anchorPos.y == 1.0f) // Top系列 (TopLeft, TopCenter, TopRight)
                    {
                        // Top系列：Y=-50 表示从顶部向下50，所以是负值
                        pos.y = y.Value;
                    }
                    else if (anchorPos.y == 0.0f) // Bottom系列 (BottomLeft, BottomCenter, BottomRight)
                    {
                        // Bottom系列：Y=50 表示从底部向上50，正值表示向上
                        pos.y = y.Value;
                    }
                    else if (anchorPos.y == 0.5f) // Middle或Center系列
                    {
                        // Middle/Center系列：Y正值向下，负值向上
                        pos.y = y.Value;
                    }
                    else
                    {
                        // 其他情况，保持原逻辑
                        pos.y = y.Value;
                    }
                }

                rect.anchoredPosition = pos;
            }
        }

        // 特殊处理Image组件的Anchor调整
        Image img = go.GetComponent<Image>();
        if (img != null && img.sprite != null)
        {
            // 如果加载的sprite不是默认的(0.5,0.5) pivot，需要调整
            Vector2 spritePivot = img.sprite.pivot;
            Vector2 spriteSize = img.sprite.rect.size;
            Vector2 normalizedPivot = new Vector2(
                spritePivot.x / spriteSize.x,
                spritePivot.y / spriteSize.y
            );

            // 如果sprite的pivot不是(0.5,0.5)，需要调整RectTransform的pivot来保持视觉位置不变
            if (Vector2.Distance(normalizedPivot, new Vector2(0.5f, 0.5f)) > 0.01f)
            {
                // 保存当前anchoredPosition
                Vector2 oldPos = rect.anchoredPosition;

                // 改变pivot
                rect.pivot = normalizedPivot;

                // 调整position以保持视觉位置
                Vector2 pivotDelta = normalizedPivot - new Vector2(0.5f, 0.5f);
                Vector2 sizeDelta = rect.sizeDelta;
                rect.anchoredPosition = oldPos + new Vector2(
                    pivotDelta.x * sizeDelta.x,
                    pivotDelta.y * sizeDelta.y
                );
            }
        }
    }

    void SetAnchor(RectTransform rect, string anchor)
    {
        switch (anchor)
        {
            case "TopLeft":
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                break;
            case "TopCenter":
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                break;
            case "TopRight":
                rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                break;
            case "MiddleLeft":
                rect.anchorMin = rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                break;
            case "Center":
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            case "MiddleRight":
                rect.anchorMin = rect.anchorMax = new Vector2(1, 0.5f);
                rect.pivot = new Vector2(1, 0.5f);
                break;
            case "BottomLeft":
                rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                break;
            case "BottomCenter":
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                break;
            case "BottomRight":
                rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                break;
        }
    }

    float? GetFloatAttr(XElement xml, string name)
    {
        string value = xml.Attribute(name)?.Value;
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            return result;
        return null;
    }

    // 根据Anchor名称获取对应的pivot点
    Vector2 GetAnchorPivot(string anchorName)
    {
        switch (anchorName)
        {
            case "TopLeft":
                return new Vector2(0, 1);
            case "TopCenter":
                return new Vector2(0.5f, 1);
            case "TopRight":
                return new Vector2(1, 1);
            case "MiddleLeft":
                return new Vector2(0, 0.5f);
            case "Center":
            case "MiddleCenter":
                return new Vector2(0.5f, 0.5f);
            case "MiddleRight":
                return new Vector2(1, 0.5f);
            case "BottomLeft":
                return new Vector2(0, 0);
            case "BottomCenter":
                return new Vector2(0.5f, 0);
            case "BottomRight":
                return new Vector2(1, 0);
            default:
                return new Vector2(0.5f, 0.5f);
        }
    }

    // 将WPF风格的坐标转换为Unity坐标
    Vector2 ConvertWPFCoordinatesToUnity(Vector2 xmlPos, Vector2 anchorPos, Vector2 parentSize)
    {
        Vector2 unityPos = xmlPos;

        // 根据anchor位置调整Y坐标
        if (anchorPos.y == 1.0f) // Top系列
        {
            // WPF: Y负值向下，Unity: Y负值向下
            // 对于Top锚点，保持Y值不变
            unityPos.y = xmlPos.y;
        }
        else if (anchorPos.y == 0.0f) // Bottom系列
        {
            // 对于Bottom锚点，Y正值需要调整
            unityPos.y = xmlPos.y;
        }
        else if (anchorPos.y == 0.5f) // Middle/Center系列
        {
            // 对于Middle锚点，Y值就是相对于中心的偏移
            unityPos.y = xmlPos.y;
        }

        return unityPos;
    }
}

