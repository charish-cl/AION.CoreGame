using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using GameLogic;

namespace GameLogic.Editor
{
    /// <summary>
    /// Actor调试Inspector，用于在编辑器中美观地显示GameActor的组件信息
    /// </summary>
    [CustomEditor(typeof(ActorDebugComponent))]
    [CanEditMultipleObjects]
    public class ActorDebugInspect : UnityEditor.Editor
    {
        // 存储每个组件的展开状态
        private Dictionary<string, bool> m_componentFoldouts = new Dictionary<string, bool>();
        
        // 存储每个组件属性的展开状态
        private Dictionary<string, Dictionary<string, bool>> m_propertyFoldouts = new Dictionary<string, Dictionary<string, bool>>();
        
        // 样式
        private GUIStyle m_headerStyle;
        private GUIStyle m_componentHeaderStyle;
        private GUIStyle m_propertyLabelStyle;
        private GUIStyle m_boxStyle;
        
        private bool m_initialized = false;
        
        private void InitializeStyles()
        {
            if (m_initialized) return;
            
            // 标题样式
            m_headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.6f, 1f) }
            };
            
            // 组件标题样式
            m_componentHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };
            
            // 属性标签样式
            m_propertyLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            
            // 盒子样式
            m_boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            m_initialized = true;
        }
        
        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            ActorDebugComponent debugComp = (ActorDebugComponent)target;
            
            // 绘制默认Inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);
            
            // 获取GameActor
            GameActor actor = debugComp.GetActor();
            
            if (actor == null)
            {
                EditorGUILayout.HelpBox("未找到关联的GameActor。请确保Actor已添加到SceneMgr中。", MessageType.Warning);
                return;
            }
            
            // 绘制Actor基本信息
            DrawActorInfo(actor);
            
            EditorGUILayout.Space(10);
            
            // 绘制配置信息
            DrawConfigs(actor);
            
            EditorGUILayout.Space(10);
            
            // 绘制组件列表
            DrawComponents(actor);
        }
        
        /// <summary>
        /// 绘制Actor基本信息
        /// </summary>
        private void DrawActorInfo(GameActor actor)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            
            EditorGUILayout.LabelField("Actor Information", m_headerStyle);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tag:", GUILayout.Width(80));
            EditorGUILayout.LabelField(actor.Tag.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"({actor.Position.x:F2}, {actor.Position.y:F2})", EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("IsDestroyed:", GUILayout.Width(80));
            EditorGUILayout.LabelField(actor.IsDestroyed ? "Yes" : "No", 
                actor.IsDestroyed ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Components:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{actor.cmps.Count}", EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制配置信息
        /// </summary>
        private void DrawConfigs(GameActor actor)
        {
            // 使用反射获取m_configs字典
            var configsField = typeof(GameActor).GetField("m_configs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (configsField == null)
            {
                return;
            }
            
            var configs = configsField.GetValue(actor) as Dictionary<System.Type, object>;
            
            if (configs == null || configs.Count == 0)
            {
                return;
            }
            
            EditorGUILayout.BeginVertical(m_boxStyle);
            
            EditorGUILayout.LabelField("Configurations", m_headerStyle);
            EditorGUILayout.Space(5);
            
            string configsKey = "Actor_Configs";
            if (!m_propertyFoldouts.ContainsKey(configsKey))
            {
                m_propertyFoldouts[configsKey] = new Dictionary<string, bool>();
            }
            
            bool isExpanded = false;
            if (m_propertyFoldouts[configsKey].TryGetValue("Expanded", out bool expanded))
            {
                isExpanded = expanded;
            }
            
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"Configs ({configs.Count})", EditorStyles.foldoutHeader);
            m_propertyFoldouts[configsKey]["Expanded"] = isExpanded;
            
            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                
                foreach (var kvp in configs)
                {
                    System.Type configType = kvp.Key;
                    object config = kvp.Value;
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField($"Type: {configType.Name}", EditorStyles.boldLabel);
                    
                    if (config != null)
                    {
                        // 尝试获取配置的基本信息
                        DrawConfigInfo(config, configType);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("<null>", EditorStyles.miniLabel);
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制配置的详细信息
        /// </summary>
        private void DrawConfigInfo(object config, System.Type configType)
        {
            // 使用反射获取配置的公共属性
            var properties = configType.GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
            
            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;
                
                try
                {
                    object value = prop.GetValue(config);
                    string valueStr = FormatConfigValue(value, prop.PropertyType);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{prop.Name}:", m_propertyLabelStyle, GUILayout.Width(120));
                    EditorGUILayout.LabelField(valueStr, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
                catch (System.Exception ex)
                {
                    EditorGUILayout.LabelField($"{prop.Name}: <Error: {ex.Message}>", EditorStyles.miniLabel);
                }
            }
        }
        
        /// <summary>
        /// 格式化配置值
        /// </summary>
        private string FormatConfigValue(object value, System.Type type)
        {
            if (value == null)
            {
                return "<null>";
            }
            
            if (type == typeof(string))
            {
                return (string)value;
            }
            
            if (type == typeof(int) || type == typeof(float) || type == typeof(double))
            {
                return value.ToString();
            }
            
            if (type == typeof(bool))
            {
                return value.ToString();
            }
            
            if (type.IsEnum)
            {
                return value.ToString();
            }
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var list = value as System.Collections.ICollection;
                if (list != null)
                {
                    return $"List<{type.GetGenericArguments()[0].Name}> ({list.Count} items)";
                }
            }
            
            // 如果是引用类型（如ModelConfig），显示类型名称和ID（如果有）
            if (type.IsClass)
            {
                // 尝试获取Id属性
                var idProp = type.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (idProp != null && idProp.CanRead)
                {
                    try
                    {
                        var id = idProp.GetValue(value);
                        return $"{type.Name} (Id: {id})";
                    }
                    catch { }
                }
                
                return $"{type.Name}";
            }
            
            return value.ToString();
        }
        
        /// <summary>
        /// 绘制组件列表
        /// </summary>
        private void DrawComponents(GameActor actor)
        {
            EditorGUILayout.LabelField("Components", m_headerStyle);
            EditorGUILayout.Space(5);
            
            if (actor.cmps == null || actor.cmps.Count == 0)
            {
                EditorGUILayout.HelpBox("该Actor没有组件。", MessageType.Info);
                return;
            }
            
            int index = 0;
            foreach (var cmp in actor.cmps)
            {
                if (cmp == null) continue;
                
                string componentKey = $"{cmp.GetType().Name}_{index}";
                
                // 初始化组件展开状态
                if (!m_componentFoldouts.ContainsKey(componentKey))
                {
                    m_componentFoldouts[componentKey] = false;
                }
                
                // 绘制组件
                DrawComponent(cmp, componentKey, index);
                
                EditorGUILayout.Space(3);
                index++;
            }
        }
        
        /// <summary>
        /// 绘制单个组件
        /// </summary>
        private void DrawComponent(GameActorCmp cmp, string componentKey, int index)
        {
            EditorGUILayout.BeginVertical(m_boxStyle);
            
            // 组件标题（可展开）
            string componentName = $"[{index}] {cmp.GetType().Name}";
            bool isExpanded = m_componentFoldouts[componentKey];
            
            EditorGUILayout.BeginHorizontal();
            
            // 启用/禁用状态指示
            Color originalColor = GUI.color;
            GUI.color = cmp.Enable ? Color.green : Color.red;
            GUILayout.Label(cmp.Enable ? "●" : "○", GUILayout.Width(15));
            GUI.color = originalColor;
            
            // 展开/折叠按钮
            isExpanded = EditorGUILayout.Foldout(isExpanded, componentName, m_componentHeaderStyle);
            m_componentFoldouts[componentKey] = isExpanded;
            
            EditorGUILayout.EndHorizontal();
            
            // 如果展开，绘制组件属性
            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawComponentProperties(cmp, componentKey);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 绘制组件属性
        /// </summary>
        private void DrawComponentProperties(GameActorCmp cmp, string componentKey)
        {
            Type componentType = cmp.GetType();
            
            // 初始化属性展开状态字典
            if (!m_propertyFoldouts.ContainsKey(componentKey))
            {
                m_propertyFoldouts[componentKey] = new Dictionary<string, bool>();
            }
            
            var propertyFoldout = m_propertyFoldouts[componentKey];
            
            EditorGUILayout.Space(3);
            
            // 绘制基础属性
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Basic Properties", EditorStyles.miniLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enable:", GUILayout.Width(80));
            EditorGUILayout.LabelField(cmp.Enable.ToString(), m_propertyLabelStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 使用反射获取所有字段和属性
            FieldInfo[] fields = componentType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            PropertyInfo[] properties = componentType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            
            // 绘制字段
            if (fields.Length > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Fields", EditorStyles.miniLabel);
                
                foreach (var field in fields)
                {
                    // 跳过一些内部字段
                    if (field.Name == "Actor" || field.Name.StartsWith("m_") && field.IsPrivate)
                    {
                        // 可以选择显示私有字段，这里先跳过
                        continue;
                    }
                    
                    DrawFieldOrProperty(field.Name, field.GetValue(cmp), field.FieldType, componentKey);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // 绘制属性
            if (properties.Length > 0)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Properties", EditorStyles.miniLabel);
                
                foreach (var prop in properties)
                {
                    // 跳过一些属性
                    if (prop.Name == "Actor" || !prop.CanRead)
                        continue;
                    
                    try
                    {
                        object value = prop.GetValue(cmp);
                        DrawFieldOrProperty(prop.Name, value, prop.PropertyType, componentKey);
                    }
                    catch (System.Exception ex)
                    {
                        EditorGUILayout.LabelField($"{prop.Name}: <Error: {ex.Message}>", EditorStyles.miniLabel);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // 特殊处理：如果是NumericComponent，显示数值信息
            if (cmp is NumericComponent numericCmp)
            {
                EditorGUILayout.Space(3);
                DrawNumericComponentInfo(numericCmp, componentKey);
            }
            
            // 特殊处理：如果是BuffCmp，显示Buff信息
            if (cmp is BuffCmp buffCmp)
            {
                EditorGUILayout.Space(3);
                DrawBuffComponentInfo(buffCmp, componentKey);
            }
        }
        
        /// <summary>
        /// 绘制字段或属性值
        /// </summary>
        private void DrawFieldOrProperty(string name, object value, System.Type type, string componentKey)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"{name}:", m_propertyLabelStyle, GUILayout.Width(120));
            
            if (value == null)
            {
                EditorGUILayout.LabelField("<null>", EditorStyles.miniLabel);
            }
            else if (type == typeof(bool))
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.label);
            }
            else if (type == typeof(int) || type == typeof(float) || type == typeof(double))
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.label);
            }
            else if (type == typeof(string))
            {
                EditorGUILayout.LabelField((string)value, EditorStyles.label);
            }
            else if (type == typeof(Vector2) || type == typeof(Vector3))
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.label);
            }
            else if (type.IsEnum)
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.label);
            }
            else if (type.IsClass)
            {
                // 对于复杂类型，显示类型名称
                EditorGUILayout.LabelField($"<{type.Name}>", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(value.ToString(), EditorStyles.label);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 绘制NumericComponent的特殊信息
        /// </summary>
        private void DrawNumericComponentInfo(NumericComponent numericCmp, string componentKey)
        {
            string numericKey = $"{componentKey}_Numeric";
            
            if (!m_propertyFoldouts.ContainsKey(numericKey))
            {
                m_propertyFoldouts[numericKey] = new Dictionary<string, bool>();
            }
            
            bool isExpanded = false;
            if (m_propertyFoldouts[numericKey].TryGetValue("Expanded", out bool expanded))
            {
                isExpanded = expanded;
            }
            
            isExpanded = EditorGUILayout.Foldout(isExpanded, "Numeric Values", EditorStyles.foldoutHeader);
            m_propertyFoldouts[numericKey]["Expanded"] = isExpanded;
            
            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                
                // 使用反射获取NumericDic字典
                var numericDicField = typeof(NumericComponent).GetField("NumericDic", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (numericDicField != null)
                {
                    var numericDic = numericDicField.GetValue(numericCmp) as System.Collections.Generic.Dictionary<int, int>;
                    
                    if (numericDic != null && numericDic.Count > 0)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        // 显示常用数值
                        EditorGUILayout.LabelField("Common Values:", EditorStyles.miniLabel);
                        
                        // 尝试获取常用数值类型
                        System.Array numericTypes = System.Enum.GetValues(typeof(NumericType));
                        foreach (NumericType nt in numericTypes)
                        {
                            int key = (int)nt;
                            if (numericDic.ContainsKey(key))
                            {
                                int value = numericDic[key];
                                float floatValue = numericCmp.GetAsFloat(nt);
                                
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{nt}:", m_propertyLabelStyle, GUILayout.Width(150));
                                EditorGUILayout.LabelField($"Int: {value}", EditorStyles.miniLabel, GUILayout.Width(100));
                                EditorGUILayout.LabelField($"Float: {floatValue:F4}", EditorStyles.miniLabel);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        
                        EditorGUILayout.EndVertical();
                        
                        EditorGUILayout.Space(3);
                        
                        // 显示所有数值
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField($"All Values ({numericDic.Count}):", EditorStyles.miniLabel);
                        
                        foreach (var kvp in numericDic.OrderBy(x => x.Key))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Key {kvp.Key}:",m_propertyLabelStyle,GUILayout.Width(100));
                            EditorGUILayout.LabelField($"{kvp.Value}", EditorStyles.miniLabel);
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No numeric values", EditorStyles.miniLabel);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 绘制BuffCmp的特殊信息
        /// </summary>
        private void DrawBuffComponentInfo(BuffCmp buffCmp, string componentKey)
        {
            string buffKey = $"{componentKey}_Buffs";
            
            if (!m_propertyFoldouts.ContainsKey(buffKey))
            {
                m_propertyFoldouts[buffKey] = new Dictionary<string, bool>();
            }
            
            bool isExpanded = false;
            if (m_propertyFoldouts[buffKey].TryGetValue("Expanded", out bool expanded))
            {
                isExpanded = expanded;
            }
            
            isExpanded = EditorGUILayout.Foldout(isExpanded, "Buffs", EditorStyles.foldoutHeader);
            m_propertyFoldouts[buffKey]["Expanded"] = isExpanded;
            
            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                
                // 使用反射获取buffs列表（如果是private）
                var buffsField = typeof(BuffCmp).GetField("buffs", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (buffsField != null)
                {
                    var buffs = buffsField.GetValue(buffCmp) as System.Collections.Generic.List<BaseBuff>;
                    
                    if (buffs != null && buffs.Count > 0)
                    {
                        EditorGUILayout.LabelField($"Active Buffs: {buffs.Count}", EditorStyles.miniLabel);
                        
                        foreach (var buff in buffs)
                        {
                            if (buff != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"- {buff.Id} (ID: {buff.BuffId})", EditorStyles.miniLabel);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No active buffs", EditorStyles.miniLabel);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
}

