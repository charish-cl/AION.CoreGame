using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AION.CoreFramework;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

/// <summary>
/// 组件自动绑定工具
/// </summary>
public class ComponentAutoBindTool : SerializedMonoBehaviour
{
    [Serializable]
    public class BindData
    {
        public BindData()
        {
        }

        public BindData(Object bindCom, string TypeName)
        {
            BindCom = bindCom;
            this.TypeName = TypeName;
        }

        public Object BindCom;

        [ValueDropdown("GetTypeName")] public string TypeName;

        public List<string> GetTypeName()
        {
            return dicWidget;
        }
    }

    [Title("UI控件")] [TableList] [ValueDropdown("TreeViewAdd", ExpandAllMenuItems = true, IsUniqueList = false, DrawDropdownForListElements = true)]
    public List<BindData> BindDatas = new List<BindData>();

    private IEnumerable TreeViewAdd()
    {
        return transform.GetComponentsInChildren<Transform>(true)
            .Select(x =>
                new ValueDropdownItem(GetTransformPath(transform, x.transform), new BindData(x.gameObject, "Button")));
    }


    [Title("UI子元素")] //暂定用物体Name作为key
    [ValueDropdown("AddUIWidget", ExpandAllMenuItems = true, IsUniqueList = false, DrawDropdownForListElements = true)]
    [ListDrawerSettings(CustomAddFunction = "AddUIWidget")]
    public List<GameObject> SubItems = new List<GameObject>();

    public IEnumerable AddUIWidget()
    {
        return GetAllSubUIWidget()
            .Select(x =>
                new ValueDropdownItem(GetTransformPath(transform, x.transform), x.gameObject));
    }

    [Button("获取所有子Item", ButtonHeight = 40)]
    public void GetChildrenSubUIWidget()
    {
        SubItems = GetAllSubUIWidget();
    }
    
    public List<GameObject> GetAllSubUIWidget()
    {
        //获取名字以Item结尾的物体
      return  transform.GetComponentsInChildren<ComponentAutoBindTool>(true)
            .Where(e => e.uiType == UIType.UIWidget).Select(e => e.gameObject).ToList();
    }
    

    [ValueDropdown("TreeViewAdd")] [LabelText("添加数据")] [OnValueChanged("SimpleAdd")]
    public BindData AddData;

    public void SimpleAdd()
    {
        if (AddData == null || AddData.BindCom == null)
        {
            return;
        }

        BindDatas.Add(AddData);
        AddData = null;
    }

    public int GetIndex(Object bindCom)
    {
        var bindData = BindDatas.Find(e => e.BindCom == bindCom);

        if (bindData != null)
        {
            if (dicIndex == null)
            {
                //索引字典 把dicWidget类型索引对应

                dicIndex = new Dictionary<string, int>();

                for (var i = 0; i < dicWidget.Count; i++)
                {
                    dicIndex.Add(dicWidget[i], i);
                }
            }

            return dicIndex[bindData.TypeName];
        }

        return 0;
    }


    public void UpdateData(Object selectobj, string TypeName)
    {
        var bindData = BindDatas.Find(e => e.BindCom == selectobj);
        if (bindData != null)
        {
            bindData.BindCom = selectobj;
            bindData.TypeName = TypeName;
        }
        else
        {
            Debug.Log("添加");
            BindDatas.Add(new BindData(selectobj, TypeName));
        }
    }
    
    public void DeletNullReference()
    {
        BindDatas.RemoveAll(e => e.BindCom == null);
        SetDirty();
    }

    
    /// <summary>
    /// 获取子物体的绑定数据
    /// </summary>
    [ButtonGroup("Tools2")]
    [Button("获取子物体的绑定数据", ButtonHeight = 40)]
    public void GetBindDataFromChild()
    {
        var bindDatas = transform.GetComponentsInChildren<ComponentAutoBindTool>(true)
            .Where(e =>
                e.uiType != UIType.UIWidget
                && e.uiType != UIType.WindowItemOrWindow
                && e.transform.parent.GetComponentInParent<ComponentAutoBindTool>() == this
            )
            .Select(e => e.BindDatas);


        //获取所有UIWidget

        foreach (var data in bindDatas)
        {
            data.ForEach(e =>
            {
                if (BindDatas.Exists(x => x.BindCom == e.BindCom))
                {
                    return;
                }

                BindDatas.Add(e);
            });
        }

        SetDirty();
    }

    private const string GeneratePartialCodePath = "Assets/GameLogic/UI/Generate/";
    private const string GenerateFormCodePath = "Assets/GameLogic/UI/";

    private const string Namespace = "UI";
   

    // UI元素名与类型的映射字典
    public static List<string> dicWidget = new List<string>()
    {
        "None",
        "TextMeshProUGUI", "GameObject", "Image", "Transform", "RectTransform", "Text", "Button",
        "RawImage", "ScrollRect", "InputField", "TMP_InputField", "Slider", "ToggleGroup", "Toggle", "TabModule"
    };

    public static Dictionary<string, int> dicIndex;
    string className;
    
    public enum UIType
    {
        [LabelText("UIWindow")] UIForm,
        [LabelText("UIWidget")] UIWidget,
        [LabelText("WindowItem或Window")] WindowItemOrWindow,
    }

    [LabelText("是否是UIWidget")] public UIType uiType = UIType.UIForm;

    private void OnEnable()
    {
        var selectedTransform = Selection.activeTransform;
        if (selectedTransform == null)
        {
            return;
        }
        //如果名字里含有Item，则认为是UIWidget
        if (selectedTransform.name.Contains("Item"))
        {
            uiType = UIType.UIWidget;
        }
        else
        {
            uiType = UIType.UIForm;
        }
    }
#if UNITY_EDITOR
    [ButtonGroup("Tools")]
    [Button("生成代码", ButtonHeight = 40)]
    public string GenerateUIBindings()
    {
        var selectedTransform = Selection.activeTransform;
        if (selectedTransform == null)
        {
            Debug.LogError("Please select a valid UI element.");
            return string.Empty;
        }
        DeletNullReference();
        
        
        StringBuilder builder_register = new StringBuilder();
        StringBuilder builder_Func = new StringBuilder();
        StringBuilder builder_propery = new StringBuilder();

        //先都添加一个空行
        builder_register.AppendLine();
        builder_Func.AppendLine();
        builder_propery.AppendLine();

        foreach (BindData bindData in BindDatas)
        {
            string name = bindData.BindCom.name;
            string type = bindData.TypeName;
            Transform tr = (bindData.BindCom as GameObject).GetComponent<Transform>();
            string path = GetTransformPath(selectedTransform, tr);
            string propName = GetPropNameFrom(name);

            builder_propery.AppendLine($"        public {type} {propName} {{ get;  set; }}");
            switch (type)
            {
                case "GameObject":
                    //前面加一些空格
                    builder_register.AppendLine($"            {propName} = transform.Find(\"{path}\").gameObject;");
                    break;
                case "Button":
                    builder_register.AppendLine(
                        $"            {propName} = transform.Find(\"{path}\").GetComponent<{type}>();");
                    builder_register.AppendLine(
                        $"            {propName}.onClick.AddListener(() => {GetBtnFuncName(propName)}());");
                    builder_Func.AppendLine(GetBtnFuncCode(propName));
                    break;
                case "TabModule":
                    builder_register.AppendLine(
                        $"            {propName} = transform.Find(\"{path}\").GetComponent<UISetActiveBindTool>().ConvertToTab();");
                    break;
                default:
                    builder_register.AppendLine(
                        $"            {propName} = transform.Find(\"{path}\").GetComponent<{type}>();");
                    break;
            }
        }

        //子UIForm绑定逻辑
        foreach (var subItem in SubItems)
        {
            string name = subItem.name;
            if (subItem.GetComponent<ComponentAutoBindTool>() == null)
            {
                Debug.LogError($"{name} 没有挂载 ComponentAutoBindTool组件，无法绑定UIWidget");
            }

            string type = subItem.GetComponent<ComponentAutoBindTool>().GetType().Name;

            Transform tr = subItem.transform;
            string path = GetTransformPath(selectedTransform, tr);
            string propName = GetPropNameFrom(name);

            builder_propery.AppendLine($"        public {type} {propName} {{ get;  set; }}");
            builder_register.AppendLine(
                $"            {propName} = transform.Find(\"{path}\").GetComponent<{type}>();");
        }

        string uiBindCode = "";
        string uiFormCode = "";
        
        className = selectedTransform.name;

        if (uiType == UIType.UIForm)
        {
            //绑定的类
            uiBindCode =CodeTemplate.uiFormBindClass
                .Replace("#命名空间#", Namespace)
                .Replace("#类名#", className)
                .Replace("#绑定变量#", builder_propery.ToString())
                .Replace("#绑定代码#", builder_register.ToString());
        }
        else if (uiType == UIType.WindowItemOrWindow)
        {
            if (string.IsNullOrEmpty(className))
            {
                if (EditorUtility.DisplayDialog("提升", "className为空！", "是"))
                {
                    return null;
                }
            }

            //绑定的类
            string UIWidgetCode = CodeTemplate.uiWindowClass
                .Replace("#类名#", className)
                .Replace("#绑定变量#", builder_propery.ToString())
                .Replace("#绑定代码#", builder_register.ToString())
                .Replace("#绑定方法#", builder_Func.ToString());

            //复制
            Debug.Log(UIWidgetCode);
            GUIUtility.systemCopyBuffer = UIWidgetCode;
            return UIWidgetCode;
        }
        else if (uiType == UIType.UIWidget)
        {
            // 定义 UIComponent 目录的基本路径
            uiBindCode = CodeTemplate.UIWidgetBindClass
                .Replace("#命名空间#", Namespace)
                .Replace("#类名#", className)
                .Replace("#绑定变量#", builder_propery.ToString())
                .Replace("#绑定代码#", builder_register.ToString());
        }


        string bindClass = (uiType == UIType.UIWidget) ? CodeTemplate.UIWidgetClass : CodeTemplate.uiFormClass;
        //主类
        uiFormCode = bindClass
            .Replace("#命名空间#", Namespace)
            .Replace("#类名#", className)
            .Replace("#绑定方法#", builder_Func.ToString());


        string generatePartialCodePath = GeneratePartialCodePath;
        string generateFormCodePath = GenerateFormCodePath;

        if (uiType == UIType.UIWidget)
        {
            generatePartialCodePath += "UIWidget/";
            generateFormCodePath += "UIWidget/";
        }

        if (!Directory.Exists(generatePartialCodePath))
        {
            Directory.CreateDirectory(generatePartialCodePath);
        }

        if (!Directory.Exists(generateFormCodePath))
        {
            Directory.CreateDirectory(generateFormCodePath);
        }


        string UIFormCodeFilePath = generateFormCodePath + className + ".cs";
        string BindCodeFilePath = $"{generatePartialCodePath}/{className}.Bind.cs";

        if (!string.IsNullOrEmpty(UIFormCodeFilePath))
        {
            //UIFormCodeFilePath只生成一次,搜索整个脚本目录
            if (!AssetPathUtility.DoesScriptExist(className))
            {
                File.WriteAllText(UIFormCodeFilePath, uiFormCode);
            }

            File.WriteAllText(BindCodeFilePath, uiBindCode);
            AssetDatabase.Refresh();
        }

        return uiFormCode;
    }
#endif
    public string GetPropNameFrom(string widgetName)
    {
        string[] names = widgetName.Split('_');
        return names[names.Length - 1].Substring(0, 1) + names[names.Length - 1].Substring(1);
    }


    private string GetTransformPath(Transform parent, Transform child)
    {
        if (parent == null || child == null)
        {
            return string.Empty;
        }

        string path = child.name;
        Transform current = child.parent;
        while (current != null && current != parent)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return current == parent ? path : string.Empty;
    }

    private string GetBtnFuncName(string btnName)
    {
        return "OnClick_" + btnName;
    }

    private string GetBtnFuncCode(string btnName)
    {
        //用一个stringbuilder来拼接代码
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"        private void {GetBtnFuncName(btnName)}()");
        builder.AppendLine("        {");
        builder.AppendLine("        ");
        builder.AppendLine("        }");
        return builder.ToString();
    }

    //获取前缀
    private string GetPrefix(string name)
    {
        string[] names = name.Split('_');
        return names[0];
    }

    public bool HashComponent(Object selectobj)
    {
        return BindDatas.Exists(e => e.BindCom == selectobj);
    }

    public void RemoveData(Object selectobj)
    {
        BindDatas.RemoveAll(e => e.BindCom == selectobj);
        SetDirty();
    }

    void SetDirty()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
#endif
    }
}