using System.IO;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameDevKitEditor
{
    [TreeWindow("资源工具/AssetBundle测试")]
    public class AssetBundleTestWindow : OdinEditorWindow
    {
        [Button("Build and Copy Selected AssetBundle")]
        public static void BuildAndCopySelectedAssetBundle()
        {
            // 获取选中的对象
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected for AssetBundle.");
                return;
            }

            // 目标StreamingAssets目录
            string streamingAssetsPath = Application.dataPath + "/StreamingAssets";
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
            }

            // 获取当前编辑器平台
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            // 遍历选中的对象
            foreach (Object obj in selectedObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"Invalid asset path for selected object: {obj.name}");
                    continue;
                }

                // 设置AssetBundle名称
                string assetBundleName = obj.name.ToLower(); // 使用对象名称作为AssetBundle名称
                AssetImporter.GetAtPath(assetPath).assetBundleName = assetBundleName;

            
                // 创建AssetBundleBuild结构
                AssetBundleBuild build = new AssetBundleBuild
                {
                    assetBundleName = assetBundleName,
                    assetNames = new[] { assetPath }
                };

                // 打包AssetBundle到StreamingAssets目录
                BuildPipeline.BuildAssetBundles(streamingAssetsPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, buildTarget);

                // 清理打包目录中的AssetBundle名称
                AssetImporter.GetAtPath(assetPath).assetBundleName = null;
            }

            AssetDatabase.Refresh();

            Debug.Log("Selected AssetBundles built and copied to StreamingAssets successfully!");
        }
        [Button]
        public void GetGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log(path);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

    }
    
    
    
}