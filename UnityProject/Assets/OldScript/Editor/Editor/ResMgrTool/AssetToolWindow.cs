using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using GameDevKitEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[TreeWindow("资源工具/Sprite合并工具")]
public class AssetToolWindow : OdinEditorWindow
{
   
    [TabGroup("合并资源")]
    [FolderPath]
    [LabelText("选中目录")]
    public string DirectoryPath;
    [TabGroup("合并资源")]

    public List<GameObject> dependentObjects = new List<GameObject>(); // 依赖于资源的物体列表

    [TabGroup("合并资源")]

    [LabelText("需要合并的资源")]
    [Searchable]
    public List<List<Texture2D>> NeedMergeResources = new List<List<Texture2D>>();

    public List<Object> AssetDataBaseGetAllFolderAsset(string directoryPath)
    {
        // 获取目录下的所有资源GUID
        string[] guids = AssetDatabase.FindAssets("", new string[] { directoryPath });

        // 创建列表来存储所有资源
        List<Object> assetsList = new List<Object>();

        // 遍历所有资源GUID，并加载资源添加到列表中
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            if (asset != null)
            {
                assetsList.Add(asset);
            }
        }
        return assetsList;
    }
    [TabGroup("合并资源")]

    [Button("获取当前打开文件夹下的相似资源",ButtonHeight = 50)]
    public void GetSimilarAsset()
    {
        var path = IOUtility.GetCurrentAssetDirectory();
        // SetDisableReadable(path,true);
        if (DirectoryPath!=null)
        {
            path = DirectoryPath;
        }
        var assets = AssetDataBaseGetAllFolderAsset(path)
            .Where(e=>e.GetType()==typeof(Texture2D))
            .Select(e=>e as Texture2D).ToList();
        
        NeedMergeResources?.Clear();
        NeedMergeResources = new List<List<Texture2D>>();
        ImageComparer.imageCache.Clear();
        foreach (var texture2D in assets)
        {
            ImageComparer.AddImage(texture2D);
        }
        Dictionary<string, List<Texture2D>> groupedAssets = new Dictionary<string, List<Texture2D>>();

        groupedAssets = ImageComparer.imageCache;
        // 将分组的资源添加到NeedMergeResources中
        foreach (var group in groupedAssets)
        {
            //只有一个不算是冗余资源
            if (group.Value.Count == 1)
            {
                continue;
            }
            NeedMergeResources.Add(group.Value);
        }
    }
   
    private ReplaceResouseWindow _window;
    [TabGroup("合并资源")]

    [Button("替换NeedMergeResources中的依赖对象",ButtonHeight = 50)]
    public void ReplaceNeedMergeResources()
    {
        //TODO:
        //WARNING:这里仅仅使用于一张Texture对应一张图片情况，如果是一张大图肯定有问题
        foreach (var resources in NeedMergeResources)
        {
            var sprites = resources.Select(e=>
                AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(e))).ToArray();

            dependentObjects = new List<GameObject>();
            //查找所有依赖，替换为第一张sprite
            foreach (var s in sprites)
            {
                ResourcesReplace.StartReplace(s, sprites[0]);
            }
       
            //删除只保留第一个
            for (var i = 1; i < sprites.Length; i++)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprites[i]));
            }
         
            AssetDatabase.Refresh();
        }
    }
    [TabGroup("合并资源")]
    //替换所有选中图片的依赖对象,并合并为一张图
    [Button("替换所有选中图片的依赖对象,并合并为一张图",ButtonHeight = 50)]
    public void ReplaceDependence()
    {
        
        var sprites = Selection.objects.Select(e=>
            AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(e))).ToArray();

        dependentObjects = new List<GameObject>();
        //查找所有依赖，替换为第一张sprite
        // foreach (var s in sprites)
        // {
        //     ResourcesReplace.StartReplace();
        // }
        //
        //删除只保留第一个
        for (var i = 1; i < sprites.Length; i++)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprites[i]));
        }
        AssetDatabase.Refresh();
    }
    
    
  
    /// <summary>
    /// 查找场景中依赖的对象
    /// </summary>
    /// <param name="resource">要查找依赖项的资源</param>
    private void FindSceneDependentObjects(Object resource,Action<GameObject> action)
    {
        if (resource == null)
        {
            Debug.LogError("Please select a resource to find dependent objects.");
            return;
        }
        
        // 获取场景中所有游戏对象
        GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();

        // 遍历场景中所有游戏对象
        foreach (GameObject obj in sceneObjects)
        {
            // 检查游戏对象是否依赖于指定资源
            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty prop = serializedObject.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue == resource)
                    {
                        action.Invoke(obj);
                        // EditorGUIUtility.PingObject(obj);
                        // 添加依赖资源的物体到列表中
                        dependentObjects.Add(obj);
                    }
                }
            }
        }
        
        Debug.Log("Found " + dependentObjects.Count + " objects dependent on resource.");
    }
    
  
}