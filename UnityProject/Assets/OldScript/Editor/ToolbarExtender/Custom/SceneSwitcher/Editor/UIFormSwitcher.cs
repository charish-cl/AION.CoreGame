using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace AION.CoreFramework
{
    [InitializeOnLoad]
    public class UIFormSwitcher
    {
        // 提取 UIForm 存放路径为常量
        private const string UI_FORM_PATH = "Assets/Game/UIForm/";

        const string NAMESPACE = "GameLogic";
        static UIFormSwitcher()
        {
            ToolbarExtender.LeftToolbarGUI.Add((1, OnToolbarGUI));
        }

        static readonly string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;
        static GUIStyle _popGuiStyle;
        
        
        
        static string m_UiName
        {
            get
            {
                return EditorPrefs.GetString("UIFormName", "");
            }
            set
            {
                EditorPrefs.SetString("UIFormName", value);
                //刷新工具栏
                ToolbarCallback.RepaintToolbar();
            }
        }
// 在 EditorWindow 或 Editor 脚本中调用
        private static int selectedIndex;
        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            _buttonGuiStyle ??= new GUIStyle(ButtonStyleName)
            {
                fixedWidth = 100,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            _popGuiStyle ??= new GUIStyle(ButtonStyleName)
            {
                fixedWidth = 200,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            
            // 下拉框选择所有的 UI
            GUILayout.FlexibleSpace();

            GUIStyle toggleStyle = new GUIStyle(EditorStyles.toolbarButton);
            GUIStyle dropdownStyle = new GUIStyle(EditorStyles.foldout);

          
            //绘制一个下拉搜索框
            selectedIndex = SearchableDropdown.ShowDropdown(
                new GUIContent(UINameList[selectedIndex]), // 当前选中项
                UINameList,                                // 所有选项
                toggleStyle,                            // 主按钮样式
                dropdownStyle                          // 下拉箭头样
            );
            m_UiName = UINameList[selectedIndex];
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.DropdownButton(new GUIContent("Switch UI"), FocusType.Passive, _buttonGuiStyle))
            {
                if (!Application.isPlaying)
                {
                    return;
                }
                ShowUI();
            }
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.DropdownButton(new GUIContent("打开UI预制体"), FocusType.Passive, _buttonGuiStyle))
            {
                var guids = AssetDatabase.FindAssets("t:prefab " + m_UiName, new string[] { UI_FORM_PATH });
                if (guids!= null && guids.Length > 0)
                {
                    var go =AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    AssetDatabase.OpenAsset(go);
                    EditorGUIUtility.PingObject(go);
                }
                else
                {
                    Debug.LogWarning("没有找到预制体");
                }
            }
        

            GUILayout.FlexibleSpace();
        }

        private static List<string> m_UiNameList;
        public  static List<string> UINameList
        {
            get
            {
                if (m_UiNameList == null)
                {
                    m_UiNameList = new List<string>();
                    foreach (var file in Directory.GetFiles(UI_FORM_PATH, "*.prefab", SearchOption.AllDirectories))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        m_UiNameList.Add(fileName);
                    }
                }
                return m_UiNameList;
            }
        }
        
        private static Dictionary<string,Type> _uiTypes;

        private static Dictionary<string, Type> UITypes
        {
            get
            {
                //反射继承了UIWindwow的类
                if (_uiTypes == null)
                {
                    _uiTypes = new Dictionary<string, Type>();
                
                    var types = TypeCache.GetTypesDerivedFrom<UIWindow>();
                    foreach (Type type in types)
                    {
                        _uiTypes[type.FullName] = type;
                    }
            
                }
                return _uiTypes;
            }
        }
        private static void ShowUI()
        {
            // 此处可添加显示 UI 的具体逻辑
            //反射调用Game.UI.ShowWindow反省方法
            CallShowWindow(m_UiName);

        }
        static void CallShowWindow(string windowTypeName)
        {
            // 解析目标窗口类型
            Type windowType = Type.GetType(NAMESPACE + "." + windowTypeName); // 例如"YourNamespace.MainWindow"[1](@ref)
    
            // 获取UI成员
            PropertyInfo uiProp = typeof(GameModule).GetProperty("UI");
            object uiInstance = uiProp.GetValue(null);

            // 绑定泛型方法
            MethodInfo genericShow = uiProp.PropertyType
                .GetMethod("ShowWindow")
                ?.MakeGenericMethod(windowType);

            // 执行调用
            if (genericShow != null) 
                genericShow.Invoke(uiInstance, new object[] { null });
        }
    }
}