using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    public class SelectionExtend
    {
        /// <summary>
        /// 获取当前选中的目录
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentAssetDirectory()
        {
            var GUIDs = Selection.assetGUIDs;
            foreach (var guid in GUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return path;
                //输出结果为：Assets/测试文件.png
            }

            throw new Exception("选择文件夹为空");
            return null;
        }

        /// <summary>
        /// 获取选中目录所有文件
        /// </summary>
        /// <returns></returns>
        public static List<Object> GetSelectFolderAllAsset()
        {
            return AssetDataBaseGetAllFolderAsset(GetCurrentAssetDirectory());
        }

        public static List<Object> AssetDataBaseGetAllFolderAsset(string directoryPath, List<string> excludeDirectories = null)
        {
            // 获取目录下的所有资源GUID
            string[] guids;

            if ((File.GetAttributes(directoryPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                guids = AssetDatabase.FindAssets("", new string[] { directoryPath });
            }
            else
            {
                guids = new[] { AssetDatabase.AssetPathToGUID(directoryPath) };
            }

            if (guids == null || guids.Length == 0)
            {
                throw new Exception("选择的目录下文件为空 " + directoryPath);
            }

            // 如果 excludeDirectories 为空，初始化为空列表
            excludeDirectories = excludeDirectories ?? new List<string>();

            // 创建列表来存储所有资源
            var assetsList = guids
                // 将 GUID 转换为资源路径
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                // 排除文件夹
                .Where(assetPath => !AssetDatabase.IsValidFolder(assetPath))
                // 排除在排除目录中的资源
                .Where(assetPath => !excludeDirectories.Any(excludeDir => assetPath.StartsWith(excludeDir)))
                // 加载资源
                .Select(assetPath => AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)))
                // 过滤掉空的资源
                .Where(asset => asset != null)
                // 转换为列表
                .ToList();

            return assetsList;
        }

        /// <summary>
        /// 获取当前打开的预制体
        /// </summary>
        /// <returns></returns>
        public static GameObject GetCurrentOpenPrefabRoot()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            return prefabStage.prefabContentsRoot;
        }

        /// <summary>
        /// 打开预制体
        /// </summary>
        public static GameObject OpenPrefab(Object prefab)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (!string.IsNullOrEmpty(prefabPath))
            {
                PrefabStageUtility.OpenPrefab(prefabPath);

                return GetCurrentOpenPrefabRoot();
            }
            else
            {
                Debug.LogError("Selected object is not a valid prefab.");
            }

            return null;
        }
        
        /// <summary>
        /// 获取相对于父物体的路径
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static string GetTransformPath(Transform parent, Transform child)
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
        public static (string comment,string name) GenerateConstantName(string tabName)
        {
            // Remove special characters and spaces from tab name and convert to PascalCase
            var a = tabName.Split('_');
        
            return (a.First(),a.Last());
        }
        // 获取最上层父物体的方法
        public static Transform GetRoot(Transform child)
        {
            // 如果对象没有父物体，那么它就是最上层父物体
            if (child.parent == null)
            {
                return child;
            }

            // 递归查找父物体
            return GetRoot(child.parent);
        }
    }
}