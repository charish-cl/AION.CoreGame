using System.IO;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace AION.CoreFramework
{
    [InitializeOnLoad]
    public class UIFormSwitcher
    {
        // 提取 UIForm 存放路径为常量
        private const string UI_FORM_PATH = "Assets/Game/UIForm/LocalResources";

        static UIFormSwitcher()
        {
            ToolbarExtender.LeftToolbarGUI.Add((1, OnToolbarGUI));
        }

        static readonly string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;
        static GUIStyle _popGuiStyle;

        static string[] _uiNames;
        static string[] _uiPaths;

        static int currentSceneIndex
        {
            get
            {
                return EditorPrefs.GetInt("UIFormIndex", 0);
            }
            set
            {
                EditorPrefs.SetInt("UIFormIndex", value);
            }
        }

        static void OnToolbarGUI()
        {
            GUILayout.Space(250);
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

            // 路径判空操作
            if (!Directory.Exists(UI_FORM_PATH))
            {
                
                EditorGUILayout.BeginHorizontal();
                //绘制提示信息
                GUILayout.Label("未找到UIWindow目录，请检查路径是否正确。", _popGuiStyle);
                
                EditorGUILayout.BeginHorizontal();
                return;
            }

            // 获取所有带有 UIFormLogicExtend 的类
            _uiPaths ??= Directory.GetFiles(UI_FORM_PATH, "*.prefab");
            if (_uiPaths.Length == 0)
            {
                Debug.LogWarning($"在 {UI_FORM_PATH} 路径下未找到任何 .prefab 文件。");
            }
            _uiNames ??= new string[_uiPaths.Length];
            for (int i = 0; i < _uiPaths.Length; i++)
            {
                _uiNames[i] = Path.GetFileNameWithoutExtension(_uiPaths[i]);
            }

            EditorGUILayout.BeginHorizontal();
            // 下拉框选择所有的 UI
            currentSceneIndex = EditorGUILayout.Popup("", currentSceneIndex, _uiNames, _popGuiStyle);
            GUILayout.FlexibleSpace();

            if (currentSceneIndex < _uiPaths.Length)
            {
                if (EditorGUILayout.DropdownButton(new GUIContent("Switch UI"), FocusType.Passive, _buttonGuiStyle))
                {
                    if (!Application.isPlaying)
                    {
                        return;
                    }
                    ShowUI(_uiPaths[currentSceneIndex]);
                }
                GUILayout.FlexibleSpace();
                if (EditorGUILayout.DropdownButton(new GUIContent("打开UI预制体"), FocusType.Passive, _buttonGuiStyle))
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(_uiPaths[currentSceneIndex]));
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowUI(string uiPath)
        {
            // 此处可添加显示 UI 的具体逻辑
        }
    }
}