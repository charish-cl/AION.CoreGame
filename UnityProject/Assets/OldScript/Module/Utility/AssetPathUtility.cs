using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AION.CoreFramework
{
    using System.IO;
    using UnityEngine;

    public static class AssetPathUtility
    {
        /// <summary>
        /// 获取资源相对于指定基路径的路径，传入一个GameObject。
        /// </summary>
        /// <param name="go">GameObject</param>
        /// <param name="basePath">基路径</param>
        /// <returns>相对于基路径的路径</returns>
        public static string GetAssetPathRelativeToBase(GameObject go, string basePath)
        {
            if (go == null)
            {
                Debug.LogError("The provided GameObject is null.");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(basePath))
            {
                Debug.LogError("The provided base path is null or empty.");
                return string.Empty;
            }

            // 获取 GameObject 名称
            string goName = go.name;

            // 确保基路径以斜杠结尾
            if (!basePath.EndsWith("/"))
            {
                basePath += "/";
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // 在目录中搜索与 GameObject 名称相同的文件
            string[] files = Directory.GetFiles(basePath, $"{goName}.prefab", SearchOption.AllDirectories);

            stopwatch.Stop();
            Debug.Log($"Search time: {stopwatch.ElapsedMilliseconds} ms");

            // 检查是否找到匹配项
            if (files.Length == 0)
            {
                Debug.LogError($"No prefab named {goName}.prefab was found in the directory {basePath}.");
                return string.Empty;
            }
            else if (files.Length > 1)
            {
                Debug.LogError($"Multiple prefabs named {goName}.prefab were found in the directory {basePath}.");
                return string.Empty;
            }

            // 获取相对路径
            string relativePath = files[0].Substring(basePath.Length);

            // 去除后缀
            int extensionIndex = relativePath.LastIndexOf(".prefab");
            if (extensionIndex != -1)
            {
                relativePath = relativePath.Substring(0, extensionIndex);
            }

            // 替换路径中的反斜杠
            relativePath = relativePath.Replace('\\', '/');

            // 移除路径末尾的空格
            relativePath = relativePath.Trim();

            // 返回相对路径
            return relativePath;
        }

        public static T LoadAssetDataBase<T>(string path) where T : Object
        {
            T asset = null;
#if UNITY_EDITOR
            // 使用 AssetDatabase 加载资源
             asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset at path: {path}");
            }
#endif
            return asset;
        }
        
        
        
        /// <summary>
        /// 在整个项目中搜索指定的脚本文件名。
        /// </summary>
        /// <param name="scriptFileName">要搜索的脚本文件名（不带扩展名）。</param>
        /// <returns>如果存在返回 true，否则返回 false。</returns>
        public static bool DoesScriptExist(string scriptFileName)
        {

#if UNITY_EDITOR
            // 搜索整个项目的脚本
            string[] scriptGuids = AssetDatabase.FindAssets($"t:script {scriptFileName}");

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == scriptFileName)
                {
                    return true;
                }
            }
#endif
            return false;
        }
    }
}