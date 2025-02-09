using System.Collections.Generic;
using System.IO;
using GameDevKitEditor;
using UnityEngine;
using UnityEditor;

public class ProjectController
{
    static Dictionary<string, Texture> previewIcons = new Dictionary<string, Texture>();

    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod()
    {
        return;
        Debug.Log("[InitializeOnLoadMethod] 绑定OnProjectWindowItemGUI");
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }



    [MenuItem("Assets/添加UIIcon", priority = 2)]
    public static void Refresh()
    {
        // previewIcons.Clear();
        var obj = Selection.activeObject;
        var assetPath = AssetDatabase.GetAssetPath(obj);

        
        previewIcons[assetPath] = PrefabPreview.SaveImgToCachePreviews(obj);
    }
    static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
    
        // if (Selection.activeObject == null )
        // {
        //     return;
        // }
        //
        // if (!UIPreview.IsInUIFolders(AssetDatabase.GetAssetPath(Selection.activeObject)))
        // {
        //     return;
        // }
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        
        
        // 检查是否为目录
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }
        
        if (UIPreview.IsInUIFolders(assetPath))
        {
            DrawUIPreview(selectionRect, assetPath);
        }
    }

    static void DrawUIPreview(Rect rect, string assetPath)
    {
       
        if (!previewIcons.ContainsKey(assetPath))
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (obj != null)
            {
                Texture texture =UIPreview.LoadTexture2DForGameObject(obj);
                previewIcons.Add(assetPath,texture);
           
            }
        }
        
        if (previewIcons.ContainsKey(assetPath)&& previewIcons[assetPath] != null)
        {
            // 绘制预览图像
            Rect previewRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            GUI.DrawTexture(previewRect, previewIcons[assetPath]);
            // 绘制标题
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.red;

            Rect labelRect = new Rect(rect.x, rect.y + rect.height, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, Path.GetFileNameWithoutExtension(assetPath), labelStyle);
        }
        else
        {
            EditorGUI.LabelField(rect, "No Preview Available");
        }
    }
}