using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using GameDevKitEditor;


public class UIPreview 
{
    Texture preview;

    public const string cachePreviewPath = "Assets/CachePreviews";
    public const string uiComponentsFolderPath = "Assets/Game/UIComponent";
    public const string uiFormsFolderPath = "Assets/Game/UIForm";


    public static Texture2D LoadTexture2DForGameObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("GameObject is null.");
            return null;
        }

        // 获取对象的路径
        string assetPath = AssetDatabase.GetAssetPath(obj);

        // 判断是否在指定文件夹下
        if (!assetPath.StartsWith(UIPreview.uiComponentsFolderPath) && !assetPath.StartsWith(UIPreview.uiFormsFolderPath))
        {
            Debug.LogWarning("GameObject is not in the specified folders.");
            return null;
        }

        // 根据对象的 GUID 构建预览图路径
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        string pathname = Path.Combine(UIPreview.cachePreviewPath, guid + ".png");

        // 加载并返回 Texture
        return AssetDatabase.LoadAssetAtPath<Texture2D>(pathname);
    }
    public static bool IsInUIFolders(string path)
    {
        return path.StartsWith(uiComponentsFolderPath) || path.StartsWith(uiFormsFolderPath);
    }
    
    

}