using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    public static class IOUtility
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public  static void RenameLastDirectory(string originalDirectoryPath, string newDirectoryName)
        {
            try
            {
                if (Directory.Exists(originalDirectoryPath))
                {
                    string parentDirectory = Directory.GetParent(originalDirectoryPath).FullName;
                    string originalDirectoryName = new DirectoryInfo(originalDirectoryPath).Name;
                    string newDirectoryPath = Path.Combine(parentDirectory, newDirectoryName);

                    Directory.Move(originalDirectoryPath, newDirectoryPath);
                    Debug.Log("目录已重命名。");
                }
                else
                {
                    Debug.Log("原目录不存在，无法重命名。");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("发生错误: " + ex.Message);
            }
        }
        
        public static void RenameDirectory(string originalDirectoryPath, string newDirectoryName)
        {
            try
            {
                // 检查原目录是否存在
                if (Directory.Exists(originalDirectoryPath))
                {
                    // 获取原目录的父目录路径
                    string parentDirectory = Directory.GetParent(originalDirectoryPath).FullName;
                    // 构建新目录的完整路径
                    string newDirectoryPath = Path.Combine(parentDirectory, newDirectoryName);

                    // 重命名目录
                    Directory.Move(originalDirectoryPath, newDirectoryPath);
                    
                    Debug.Log("目录已重命名。");
                }
                else
                {
                    Debug.Log("原目录不存在，无法重命名。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("发生错误: " + ex.Message);
            }
        }
        public static void SaveFileSafe(string path, string fileName, string text)
        {
            string filePath = Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new UTF8Encoding(false);
                using (StreamWriter writer = new StreamWriter(stream, utf8Encoding))
                {
                    writer.Write(text);
                }
            }
        }

        public static void SaveFileSafe(string filePath, string text)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new UTF8Encoding(false);
                using (StreamWriter writer = new StreamWriter(stream, utf8Encoding))
                {
                    writer.Write(text);
                }
            }
        }
        
        public static T LoadOrCreateScriptableObject<T>(string path) where T : ScriptableObject
        {
            T obj = null;

            // 尝试从路径加载对象
            if (File.Exists(path))
            {
                obj = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            // 如果没有找到对象，则创建一个新的
            if (obj == null)
            {
                obj = ScriptableObject.CreateInstance<T>();
             
                // 保存对象为 Asset
                AssetDatabase.CreateAsset(obj, path);
                AssetDatabase.SaveAssets();
            }

            return obj;
        }
        
        /// <summary>
        /// 获取当前选中的目录

        /// </summary>
        /// <returns></returns>
        public  static string  GetCurrentAssetDirectory()
        {
            var GUIDs = Selection.assetGUIDs;
            foreach (var guid in GUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return path;
                //输出结果为：Assets/测试文件.png
            }
            return null;
        }

        public static string GetAssetPath(string destPath)
        {
            if (string.IsNullOrEmpty(destPath))
            {
                throw new ArgumentException("The provided path is null or empty.", nameof(destPath));
            }

            // 将路径中的反斜杠替换为正斜杠
            destPath = destPath.Replace("\\", "/");

            // 检查路径是否包含 "Assets" 目录，并返回相对路径
            int index = destPath.IndexOf("Assets/");
            if (index == -1)
            {
                throw new InvalidOperationException("The provided path is not within the Assets directory.");
            }

            // 截取相对于 "Assets" 的路径
            return destPath.Substring(index);
        }
    }
}


