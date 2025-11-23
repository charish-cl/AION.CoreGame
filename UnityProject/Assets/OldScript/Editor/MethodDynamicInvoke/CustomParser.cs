using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
//动态调用UI测试方法的工具
namespace DodGame
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public static class CustomParser
    {
        // ===== 公共API：从配置文本创建对象 =====
        public static T CreateInstance<T>(Dictionary<string, string> properties) where T : new()
        {
            return (T)CreateInstance(typeof(T), properties);
        }

        public static object CreateInstance(Type targetType, Dictionary<string, string> properties)
        {
            object instance = Activator.CreateInstance(targetType);

            foreach (var kvp in properties)
            {
                SetMemberValue(instance, targetType, kvp.Key, kvp.Value);
            }

            return instance;
        }

        // ===== 从配置文本解析方法参数 =====
        public static object[] ParseMethodParameters(string configText, ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return new object[0];

            if (string.IsNullOrWhiteSpace(configText))
            {
                return parameters.Select(p => GetDefaultValue(p.ParameterType)).ToArray();
            }

            // 解析配置文本为树形结构
            var rootNode = ParseConfigTree(configText);

            // 根据参数信息解析
            object[] args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i < rootNode.Children.Count)
                {
                    var paramNode = rootNode.Children[i];
                    args[i] = ParseNodeValue(paramNode, parameters[i].ParameterType);
                }
                else
                {
                    args[i] = GetDefaultValue(parameters[i].ParameterType);
                }
            }

            return args;
        }

        // ===== 配置树节点 =====
        private class ConfigNode
        {
            public string Name;
            public string Value;
            public List<ConfigNode> Children = new List<ConfigNode>();
            public int ArrayIndex = -1;
            public int IndentLevel;

            public bool IsLeaf => !string.IsNullOrEmpty(Value) || Children.Count == 0;
        }

        // ===== 解析配置文本为树形结构 =====
        private static ConfigNode ParseConfigTree(string text)
        {
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => new { Line = l, Trimmed = l.Trim() })
                .Where(x => !string.IsNullOrWhiteSpace(x.Trimmed) && !x.Trimmed.StartsWith("//"))
                .ToList();

            var root = new ConfigNode { Name = "Root", IndentLevel = -1 };
            var stack = new Stack<ConfigNode>();
            stack.Push(root);

            foreach (var lineData in lines)
            {
                string line = lineData.Line;
                string trimmed = lineData.Trimmed;
                int indent = GetIndentLevel(line);

                // 调整栈到合适的父节点
                while (stack.Count > 0 && stack.Peek().IndentLevel >= indent)
                {
                    stack.Pop();
                }

                if (stack.Count == 0)
                {
                    stack.Push(root);
                }

                ConfigNode node = new ConfigNode { IndentLevel = indent };

                // 解析不同格式的节点
                if (trimmed.Contains("]:"))
                {
                    // 格式: [PropertyName]: value
                    int colonIndex = trimmed.IndexOf("]:");
                    node.Name = trimmed.Substring(1, colonIndex - 1).Trim();
                    node.Value = trimmed.Substring(colonIndex + 2).Trim();
                }
                else if (trimmed.StartsWith("[") && trimmed.Contains("]["))
                {
                    // 格式: [PropertyName][index]
                    var parts = trimmed.Split(new[] { "][" }, StringSplitOptions.None);
                    node.Name = parts[0].Substring(1).Trim();

                    if (parts.Length > 1)
                    {
                        string indexStr = parts[1].TrimEnd(']');
                        if (int.TryParse(indexStr, out int index))
                        {
                            node.ArrayIndex = index;
                        }
                    }
                }
                else if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    // 格式: [ObjectName]
                    node.Name = trimmed.Trim('[', ']').Trim();
                }
                else
                {
                    // 纯值
                    node.Value = trimmed;
                }

                var parent = stack.Peek();
                parent.Children.Add(node);

                // 对象节点入栈
                if (string.IsNullOrEmpty(node.Value))
                {
                    stack.Push(node);
                }
            }

            return root;
        }

        // ===== 获取缩进层级 =====
        private static int GetIndentLevel(string line)
        {
            int count = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ')
                    count++;
                else if (line[i] == '\t')
                    count += 4;
                else
                    break;
            }

            return count / 4;
        }

        // ===== 递归解析节点值 =====
        private static object ParseNodeValue(ConfigNode node, Type targetType)
        {
            if (node == null || node.Value == "null")
                return GetDefaultValue(targetType);

            // 处理数组类型
            if (targetType.IsArray)
            {
                return ParseArray(node, targetType);
            }

            // 处理简单类型
            if (IsSimpleType(targetType))
            {
                // 直接有值的情况: [Name]: value
                if (!string.IsNullOrEmpty(node.Value))
                {
                    return ConvertValue(node.Value, targetType);
                }

                // 没有值，返回默认
                return GetDefaultValue(targetType);
            }

            // 处理复杂对象
            return ParseComplexObject(node, targetType);
        }

        // ===== 解析数组 =====
        private static object ParseArray(ConfigNode node, Type arrayType)
        {
            Type elementType = arrayType.GetElementType();

            // 收集数组元素节点（按索引排序）
            var arrayElements = node.Children
                .Where(c => c.ArrayIndex >= 0)
                .OrderBy(c => c.ArrayIndex)
                .ToList();

            if (arrayElements.Count == 0)
            {
                return Array.CreateInstance(elementType, 0);
            }

            Array array = Array.CreateInstance(elementType, arrayElements.Count);

            for (int i = 0; i < arrayElements.Count; i++)
            {
                object element = ParseNodeValue(arrayElements[i], elementType);
                array.SetValue(element, i);
            }

            return array;
        }

        // ===== 解析复杂对象 =====
        private static object ParseComplexObject(ConfigNode node, Type targetType)
        {
            object instance = Activator.CreateInstance(targetType);

            if (node.Children.Count == 0)
            {
                return instance;
            }

            foreach (var childNode in node.Children)
            {
                if (childNode.ArrayIndex >= 0)
                    continue;

                var member = GetMember(targetType, childNode.Name);
                if (member == null)
                {
                    Debug.LogWarning($"类型 {targetType.Name} 中未找到成员: {childNode.Name}");
                    continue;
                }

                Type memberType = GetMemberType(member);
                object value = ParseNodeValue(childNode, memberType);
                SetMemberValueDirect(instance, member, value);
            }

            return instance;
        }

        // ===== 获取成员 =====
        private static MemberInfo GetMember(Type type, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null) return prop;

            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return field;
        }

        // ===== 获取成员类型 =====
        private static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo prop) return prop.PropertyType;
            if (member is FieldInfo field) return field.FieldType;
            return null;
        }

        // ===== 设置成员值 =====
        private static void SetMemberValue(object instance, Type targetType, string memberName, string value)
        {
            var member = GetMember(targetType, memberName);
            if (member == null) return;

            Type memberType = GetMemberType(member);
            object val = ConvertValue(value, memberType);
            SetMemberValueDirect(instance, member, val);
        }

        // ===== 直接设置成员值 =====
        private static void SetMemberValueDirect(object instance, MemberInfo member, object value)
        {
            if (member is PropertyInfo prop && prop.CanWrite)
            {
                prop.SetValue(instance, value);
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(instance, value);
            }
        }

        // ===== 判断是否为简单类型 =====
        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        // ===== 获取默认值 =====
        public static object GetDefaultValue(Type type)
        {
            if (type == null)
                return null;

            if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                return null;

            return Activator.CreateInstance(type);
        }

        // ===== 转换简单类型值 =====
        public static object ConvertValue(string str, Type targetType)
        {
            if (string.IsNullOrEmpty(str) || str == "null")
                return GetDefaultValue(targetType);

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = Nullable.GetUnderlyingType(targetType);

            try
            {
                str = str.Trim().Trim('"');

                if (targetType == typeof(int) || targetType == typeof(Int32)) return int.Parse(str);
                if (targetType == typeof(long) || targetType == typeof(Int64)) return long.Parse(str);
                if (targetType == typeof(uint) || targetType == typeof(UInt32)) return uint.Parse(str);
                if (targetType == typeof(ulong) || targetType == typeof(UInt64)) return ulong.Parse(str);
                if (targetType == typeof(byte)) return byte.Parse(str);
                if (targetType == typeof(sbyte)) return sbyte.Parse(str);
                if (targetType == typeof(short) || targetType == typeof(Int16)) return short.Parse(str);
                if (targetType == typeof(ushort) || targetType == typeof(UInt16)) return ushort.Parse(str);
                if (targetType == typeof(float)) return float.Parse(str);
                if (targetType == typeof(double)) return double.Parse(str);
                if (targetType == typeof(decimal)) return decimal.Parse(str);
                if (targetType == typeof(bool))
                    return str == "1" || str.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (targetType == typeof(string)) return str;
                if (targetType == typeof(Guid)) return Guid.Parse(str);
                if (targetType.IsEnum) return Enum.Parse(targetType, str, true);

                return Convert.ChangeType(str, targetType);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"转换失败: '{str}' → {targetType.Name}, 错误: {e.Message}");
                return GetDefaultValue(targetType);
            }
        }



// ===== 生成参数配置模板 =====
public static string GenerateParameterTemplate(ParameterInfo[] parameters)
{
    var sb = new StringBuilder();

    for (int i = 0; i < parameters.Length; i++)
    {
        var param = parameters[i];
        
        // 简单类型和复杂类型都显示参数名
        if (IsSimpleType(param.ParameterType))
        {
            // 简单类型格式: [参数名]: 默认值
            sb.AppendLine($"[{param.Name}]: {GetDefaultValueString(param.ParameterType)}");
        }
        else
        {
            // 复杂类型格式: [参数名]
            sb.AppendLine($"[{param.Name}]");
            GenerateTypeTemplate(param.ParameterType, sb, 1);
        }
        
        if (i < parameters.Length - 1)
            sb.AppendLine();
    }

    return sb.ToString();
}

// ===== 递归生成类型模板 =====
private static void GenerateTypeTemplate(Type type, StringBuilder sb, int indentLevel, HashSet<Type> visitedTypes = null, int maxDepth = 3)
{
    if (visitedTypes == null)
        visitedTypes = new HashSet<Type>();

    // 防止无限递归
    if (indentLevel > maxDepth)
    {
        return;
    }

    // 防止循环引用
    if (visitedTypes.Contains(type))
    {
        return;
    }

    visitedTypes.Add(type);

    string indent = GetIndent(indentLevel);

    // 处理数组
    if (type.IsArray)
    {
        Type elementType = type.GetElementType();
        
        if (IsSimpleType(elementType))
        {
            sb.AppendLine($"{indent}[0]: {GetDefaultValueString(elementType)}");
        }
        else
        {
            sb.AppendLine($"{indent}[0]");
            GenerateTypeTemplate(elementType, sb, indentLevel + 1, new HashSet<Type>(visitedTypes), maxDepth);
        }
        return;
    }

    // 处理简单类型 - 不应该到这里
    if (IsSimpleType(type))
    {
        return;
    }

    // 处理复杂对象
    var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
        .Where(m => (m is PropertyInfo || m is FieldInfo) && !m.Name.StartsWith("_"))
        .ToList();

    foreach (var member in members)
    {
        Type memberType = GetMemberType(member);
        if (memberType == null) continue;

        // 简单类型成员
        if (IsSimpleType(memberType))
        {
            sb.AppendLine($"{indent}[{member.Name}]: {GetDefaultValueString(memberType)}");
        }
        // 数组类型
        else if (memberType.IsArray)
        {
            Type elementType = memberType.GetElementType();
            sb.AppendLine($"{indent}[{member.Name}][0]");
            GenerateTypeTemplate(elementType, sb, indentLevel + 1, new HashSet<Type>(visitedTypes), maxDepth);
        }
        // 复杂对象
        else
        {
            sb.AppendLine($"{indent}[{member.Name}]");
            GenerateTypeTemplate(memberType, sb, indentLevel + 1, new HashSet<Type>(visitedTypes), maxDepth);
        }
    }

    visitedTypes.Remove(type);
}

        private static string GetIndent(int level)
        {
            return new string(' ', level * 4);
        }

        private static string GetDefaultValueString(Type type)
        {
            if (type == typeof(string)) return "\"\"";
            if (type == typeof(bool)) return "false";
            if (type.IsEnum) return Enum.GetNames(type).FirstOrDefault() ?? "0";
            if (type.IsValueType) return "0";
            return "null";
        }
    }

    class InvokeUIMethodWindow : OdinEditorWindow
    {
        [MenuItem("Tools/打开调用窗口")]
        static void ShowWindow()
        {
            GetWindow<InvokeUIMethodWindow>().Show();
        }

        private const string BASE_NAMESPACE = "X6Game";
        private static string BASEPARENT_TYPE => BASE_NAMESPACE + ".UIBase";
        private static string BASE_UIMANAGER_TYPE => BASE_NAMESPACE + ".UISys";

        // 缓存反射对象
        private static Assembly _gameLogicAssembly;
        private static Type _uiSysType;
        private static Type _uiMgrType;
        private static object _uiMgrInstance;

        private static Assembly GameLogicAssembly =>
            _gameLogicAssembly ?? (_gameLogicAssembly = Assembly.Load("GameLogic"));

        private static Type UISysType => _uiSysType ?? (_uiSysType = GameLogicAssembly.GetTypes()
            .FirstOrDefault(t => t.FullName == BASE_UIMANAGER_TYPE));

        private static Type UIMgrType => _uiMgrType ?? (_uiMgrType = GameLogicAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == "UIManager"));

        private static object UIMgrInstance
        {
            get
            {
                if (_uiMgrInstance == null)
                {
                    var uiSysInstance = UISysType?.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
                        ?.GetValue(null);
                    _uiMgrInstance = UISysType?.GetProperty("Mgr", BindingFlags.Static | BindingFlags.Public)
                        ?.GetValue(uiSysInstance);
                }

                return _uiMgrInstance;
            }
        }

        [ShowInInspector]
        [LabelText("调用方法时自动打开UI")]
        [InfoBox("开启后，调用方法时会通过 ShowWindowAsync 打开UI窗口并在回调中执行方法",
            InfoMessageType.Warning, VisibleIf = "autoOpenUIBeforeInvoke")]
        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        public bool autoOpenUIBeforeInvoke = true;

        [ShowInInspector, ReadOnly] public object currentUIWindow;

        [ValueDropdown("GetAllUITypes")] [OnValueChanged("OnUITypeChanged")]
        public Type selectedUIType;

        [ShowInInspector, ListDrawerSettings(Expanded = true)]
        private List<MethodInfoWrapper> methodList = new List<MethodInfoWrapper>();

        private IEnumerable<ValueDropdownItem<Type>> GetAllUITypes()
        {
            try
            {
                Type baseUIType = GameLogicAssembly.GetTypes().FirstOrDefault(t => t.FullName == BASEPARENT_TYPE);
                if (baseUIType == null) return new List<ValueDropdownItem<Type>>();

                return GameLogicAssembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && baseUIType.IsAssignableFrom(t))
                    .Select(t => new ValueDropdownItem<Type>(t.Name, t))
                    .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError($"获取UI类型失败: {e.Message}");
                return new List<ValueDropdownItem<Type>>();
            }
        }

        private void OnUITypeChanged()
        {
            RefreshMethodList();
        }

        [Button("打开UI窗口", ButtonSizes.Large)]
        public void OpenUIWindowButton()
        {
            if (selectedUIType == null)
            {
                Debug.LogWarning("请先选择UI类型");
                return;
            }

            currentUIWindow = OpenUIWindow(selectedUIType, null);
            RefreshMethodList();
        }

        public object OpenUIWindow(Type uiWindowType, Action<object> callback)
        {
            try
            {
                var showWindowAsync = UIMgrType.GetMethod("ShowWindowAsync").MakeGenericMethod(uiWindowType);

                Delegate typedCallback = null;
                if (callback != null)
                {
                    Type actionType = typeof(Action<>).MakeGenericType(uiWindowType);
                    typedCallback = CreateDelegate(actionType, callback, uiWindowType);
                }

                var parameters = showWindowAsync.GetParameters();
                object[] args = new object[parameters.Length];
                args[0] = typedCallback;
                for (int i = 1; i < args.Length; i++)
                {
                    args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                }

                showWindowAsync.Invoke(UIMgrInstance, args);

                if (callback == null)
                {
                    return UIMgrType.GetMethod("GetWindow").MakeGenericMethod(uiWindowType)
                        .Invoke(UIMgrInstance, null);
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"打开UI窗口失败: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private Delegate CreateDelegate(Type delegateType, Action<object> callback, Type uiType)
        {
            var wrapperType = typeof(CallbackWrapper<>).MakeGenericType(uiType);
            var wrapper = Activator.CreateInstance(wrapperType);
            wrapperType.GetField("Callback").SetValue(wrapper, callback);
            var methodInfo = wrapperType.GetMethod("Invoke");
            return Delegate.CreateDelegate(delegateType, wrapper, methodInfo);
        }

        private class CallbackWrapper<T>
        {
            public Action<object> Callback;
            public void Invoke(T ui) => Callback?.Invoke(ui);
        }

        [Button("刷新方法列表", ButtonSizes.Medium)]
        private void RefreshMethodList()
        {
            methodList.Clear();

            if (selectedUIType == null)
            {
                Debug.LogWarning("请先选择UI类型");
                return;
            }

            var methods = selectedUIType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .ToList();

            foreach (var method in methods)
            {
                methodList.Add(new MethodInfoWrapper(method, this));
            }

            Debug.Log($"已加载 {methodList.Count} 个方法");
        }

// ===== 新增：Odin 序列化回调 =====
        protected override void OnEnable()
        {
            base.OnEnable();

            // 反序列化后重新设置 Window 引用
            if (methodList != null)
            {
                foreach (var wrapper in methodList)
                {
                    wrapper?.SetWindow(this);
                }
            }
        }


        [Serializable]
        public class MethodInfoWrapper
        {
            [ShowInInspector, ReadOnly, HideLabel] public string MethodSignature { get; private set; }

            // ===== 保存方法元数据而不是 MethodInfo 本身 =====
            [SerializeField, HideInInspector] private string methodName;

            [SerializeField, HideInInspector] private string[] parameterTypeNames;

            [SerializeField, HideInInspector] private string returnTypeName;

            // ===== 缓存 MethodInfo（运行时重建） =====
            [NonSerialized] private MethodInfo _cachedMethod;

            [NonSerialized] private InvokeUIMethodWindow _window;

            // ===== 属性：延迟获取 MethodInfo =====
            private MethodInfo Method
            {
                get
                {
                    if (_cachedMethod == null && Window != null && Window.selectedUIType != null)
                    {
                        _cachedMethod = ReconstructMethodInfo();
                    }

                    return _cachedMethod;
                }
            }

            private InvokeUIMethodWindow Window
            {
                get => _window;
                set => _window = value;
            }

            [SerializeField, HideInInspector] private bool hasParameters;

            public bool HasParameters => hasParameters;

            [ShowInInspector, HideLabel]
            [MultiLineProperty(15)]
            [InfoBox("配置格式(支持嵌套，未填写的字段将使用默认值):\n[参数名]: 值\n[对象名]\n    [属性名]: 值",
                InfoMessageType.Info)]
            [ShowIf("HasParameters")]
            public string parameterConfig = "";

            public MethodInfoWrapper(MethodInfo method, InvokeUIMethodWindow window)
            {
                Window = window;
                _cachedMethod = method;

                // ===== 保存方法元数据 =====
                methodName = method.Name;
                returnTypeName = method.ReturnType.FullName;

                var parameters = method.GetParameters();
                hasParameters = parameters.Length > 0;

                parameterTypeNames = parameters.Select(p => p.ParameterType.FullName).ToArray();

                var paramStr = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                MethodSignature = $"{method.ReturnType.Name} {method.Name}({paramStr})";

                if (hasParameters)
                {
                    parameterConfig = CustomParser.GenerateParameterTemplate(parameters);
                }
            }

            // ===== 重建 MethodInfo =====
            private MethodInfo ReconstructMethodInfo()
            {
                if (Window?.selectedUIType == null)
                {
                    Debug.LogError("无法重建MethodInfo：UI类型为空");
                    return null;
                }

                try
                {
                    // 根据方法名和参数类型查找方法
                    Type[] paramTypes = new Type[parameterTypeNames.Length];

                    for (int i = 0; i < parameterTypeNames.Length; i++)
                    {
                        paramTypes[i] = Type.GetType(parameterTypeNames[i]);
                        if (paramTypes[i] == null)
                        {
                            // 尝试从 GameLogic 程序集查找
                            var assembly = System.Reflection.Assembly.Load("GameLogic");
                            paramTypes[i] = assembly.GetType(parameterTypeNames[i]);
                        }

                        if (paramTypes[i] == null)
                        {
                            Debug.LogError($"无法找到参数类型: {parameterTypeNames[i]}");
                            return null;
                        }
                    }

                    MethodInfo method = Window.selectedUIType.GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        paramTypes,
                        null
                    );

                    if (method == null)
                    {
                        Debug.LogError($"无法找到方法: {methodName}");
                    }

                    return method;
                }
                catch (Exception e)
                {
                    Debug.LogError($"重建MethodInfo失败: {e.Message}\n{e.StackTrace}");
                    return null;
                }
            }

            // ===== 公开方法：设置Window引用（反序列化后需要调用） =====
            public void SetWindow(InvokeUIMethodWindow window)
            {
                Window = window;
                _cachedMethod = null; // 清除缓存，下次使用时重建
            }

            [Button("调用方法", ButtonSizes.Large)]
            [GUIColor(0.4f, 0.8f, 1f)]
            public void InvokeMethod()
            {
                if (Method == null)
                {
                    Debug.Log("MethodInfo 为空，自动刷新方法列表");
                    _cachedMethod = ReconstructMethodInfo();
                }

                if (Method == null)
                {
                    Debug.LogError("无法找到方法，请检查方法名和参数类型是否正确");
                    return;
                }
                
                
                if (Window.autoOpenUIBeforeInvoke)
                {
                    InvokeWithOpenUI();
                }
                else
                {
                    InvokeDirectly();
                }
            }

            private void InvokeDirectly()
            {
                if (Window.currentUIWindow == null)
                {
                    Debug.LogError("UI窗口未打开，请先打开UI窗口或启用'调用方法时自动打开UI'");
                    return;
                }

                ExecuteMethod(Window.currentUIWindow);
            }

            private void InvokeWithOpenUI()
            {
                if (Window.selectedUIType == null)
                {
                    Debug.LogError("UI类型为空");
                    return;
                }

                object[] args = null;
                try
                {
                    args = CustomParser.ParseMethodParameters(parameterConfig, Method.GetParameters());
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析参数失败: {e.Message}\n{e.StackTrace}");
                    return;
                }

                Window.OpenUIWindow(Window.selectedUIType, (ui) =>
                {
                    try
                    {
                        var result = Method.Invoke(ui, args);

                        Debug.Log($"<color=green>✓ 在UI回调中成功调用: {Method.Name}</color>");
                        if (Method.ReturnType != typeof(void) && result != null)
                        {
                            Debug.Log($"<color=cyan>返回值: {result}</color>");
                        }

                        Window.currentUIWindow = ui;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"回调中调用失败: {e.Message}\n{e.StackTrace}");
                    }
                });
            }

            private void ExecuteMethod(object target)
            {
                try
                {
                    object[] args = CustomParser.ParseMethodParameters(parameterConfig, Method.GetParameters());
                    var result = Method.Invoke(target, args);

                    Debug.Log($"<color=green>✓ 成功调用方法: {Method.Name}</color>");
                    if (Method.ReturnType != typeof(void) && result != null)
                    {
                        Debug.Log($"<color=cyan>返回值: {result}</color>");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"调用方法失败: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}