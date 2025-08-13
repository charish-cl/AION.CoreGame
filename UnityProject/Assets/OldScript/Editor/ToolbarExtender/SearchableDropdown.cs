using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SearchableDropdown
{
    private static bool isPanelOpen;
    private static SearchableDropdownWindow dropdownWindow;
    private static Vector2 panelPosition;
    private static string searchText = "";
    private static List<string> filteredOptions;
    private static int selectedIndex;

    public static int ShowDropdown(
        GUIContent selectedContent,
        List<string> options,
        GUIStyle toggleStyle,
        GUIStyle dropdownStyle,
        float panelWidth = 250,
        float panelHeight = 300)
    {
        Rect mainRect = GUILayoutUtility.GetRect(selectedContent, toggleStyle);
        Rect dropdownRect = new Rect(
            mainRect.xMax - dropdownStyle.fixedWidth,
            mainRect.y,
            dropdownStyle.fixedWidth,
            mainRect.height
        );

        // 绘制主按钮（显示当前选中项）
        if (GUI.Button(mainRect, selectedContent, toggleStyle))
        {
            ShowDropdownPanel(mainRect, options, panelWidth, panelHeight);
        }

        // 绘制下拉箭头
        if (GUI.Button(dropdownRect, "", dropdownStyle))
        {
            ShowDropdownPanel(mainRect, options, panelWidth, panelHeight);
        }

        // 返回当前选中索引
        return selectedIndex;
    }

    private static void ShowDropdownPanel(
        Rect position,
        List<string> options,
        float width,
        float height)
    {
        panelPosition = GUIUtility.GUIToScreenPoint(position.position);
        searchText = "";
        filteredOptions = new List<string>(options);
        isPanelOpen = true;

        // 创建下拉窗口
        dropdownWindow = ScriptableObject.CreateInstance<SearchableDropdownWindow>();
        dropdownWindow.Initialize(options, width, height);
        dropdownWindow.ShowAsDropDown(new Rect(panelPosition, Vector2.zero), new Vector2(width, height));
    }

    private class SearchableDropdownWindow : EditorWindow
    {
        private List<string> options;
        private Vector2 scrollPosition;
        private float width;
        private float height;

        public void Initialize(List<string> options, float width, float height)
        {
            this.options = options;
            this.width = width;
            this.height = height;
        }

        private void OnGUI()
        {
            // 搜索框
            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField("", searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                // 实时过滤选项（不区分大小写）
                filteredOptions = options
                    .Where(opt => opt.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // 选项列表（滚动视图）
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(height - 40));
            for (int i = 0; i < filteredOptions.Count; i++)
            {
                Rect optionRect = GUILayoutUtility.GetRect(
                    new GUIContent(filteredOptions[i]),
                    EditorStyles.label,
                    GUILayout.ExpandWidth(true)
                );

                // 鼠标悬停效果
                if (optionRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(optionRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                    EditorGUIUtility.AddCursorRect(optionRect, MouseCursor.Link);
                }

                // 点击选择选项
                if (GUI.Button(optionRect, filteredOptions[i], EditorStyles.label))
                {
                    selectedIndex = options.IndexOf(filteredOptions[i]);
                    isPanelOpen = false;
                    this.Close();
                }
            }
            EditorGUILayout.EndScrollView();

            // 点击外部区域关闭面板
            if (Event.current.type == EventType.MouseDown && !position.Contains(Event.current.mousePosition))
            {
                isPanelOpen = false;
                this.Close();
            }
        }
    }
}