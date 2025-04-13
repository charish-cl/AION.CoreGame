using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace GameDevKitEditor
{
    [TreeWindow("资源工具/预制体冗余资源清理工具")]
    public class ClearResouceWindow : OdinEditorWindow
    {
        [Searchable]
        public List<string> unusedResources = new List<string>();
        [FolderPath] public string parentDirectory = "Assets/Game/Particle";
        [FolderPath] public string childDirectory = "Assets/Game/Particle/Effect";

        [Button]
        public void FindUnusedResources()
        {
            unusedResources.Clear();



            HashSet<string> childDependencies = new HashSet<string>();

            // 遍历子目录中的所有资源并收集依赖
            foreach (string filePath in Directory.GetFiles(childDirectory, "*", SearchOption.AllDirectories))
            {
                string assetPath = filePath.Replace("\\", "/");
                string[] dependencies = AssetDatabase.GetDependencies(assetPath,true);
                foreach (string dependency in dependencies)
                {
                    childDependencies.Add(dependency);
                }
            }

            // 遍历父目录中的所有资源并检查是否被使用
            foreach (string filePath in Directory.GetFiles(parentDirectory, "*", SearchOption.AllDirectories))
            {
              
                string assetPath = filePath.Replace("\\", "/");
                
                //排除子文件夹
                if (assetPath.Contains(childDirectory))
                {
                    continue;
                }
                //排除预制体和meta文件，代码文件,程序集文件,shader文件
                if ( Path.GetExtension(filePath) == ".cs"||
                     Path.GetExtension(filePath) == ".meta"
                    || Path.GetExtension(filePath) == ".dll"
                    || Path.GetExtension(filePath) == ".shader"
                    || Path.GetExtension(filePath) == ".cginc"
                    || Path.GetExtension(filePath) == ".asmdef")
                {
                    continue;
                }

                if (!childDependencies.Contains(assetPath))
                {
                    unusedResources.Add(assetPath);
                }
            }

            Debug.Log($"Found {unusedResources.Count} unused resources.");
        }

        [Button]
        public void DeleteUnusedResources()
        {
            //删除的文件挪到一个临时目录，然后再删除
            string tempDirectory = "Assets/Temp";
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);

            }

            foreach (string filePath in unusedResources)
            {
                //同时把meta文件也移动过去
                
                File.Move(filePath, tempDirectory + "/" + Path.GetFileName(filePath));
                File.Move(filePath+".meta", tempDirectory + "/" + Path.GetFileName(filePath+".meta"));
            }

            // File.Delete(tempDirectory);

            AssetDatabase.Refresh();

            Debug.Log($"Deleted {unusedResources.Count} unused resources.");
        }

        //删除父目录下的空文件夹
        [Button]
        public void DeleteEmptyFolders()
        {
            foreach (string directory in Directory.GetDirectories(parentDirectory, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, true);
                    Debug.Log($"Deleted empty folder: {directory}");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}