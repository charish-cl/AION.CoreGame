namespace GameDevKit
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEngine;
    
    public static class DirectoryUtility
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void RenameLastDirectory(string originalDirectoryPath, string newDirectoryName)
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

        /// <summary>
        /// 重命名文件目录
        /// </summary>
        /// <param name="originalDirectoryPath"></param>
        /// <param name="newDirectoryName"></param>
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
        
    }
}