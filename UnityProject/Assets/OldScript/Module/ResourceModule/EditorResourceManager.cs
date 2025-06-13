using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AION.CoreFramework
{
    /// <summary>
    /// Editor模式下不需要处理依赖的资源，直接加载
    /// </summary>
    public class EditorResourceManager:ModuleImp, IResourceManager
    {
        internal override void Shutdown()
        {
            AssetDict.Clear();
        }
        public Dictionary<string, AssetObject> AssetDict { get; set; }
        
       
        public AssetObject LoadAsset(string assetName, bool isAsync = false)
        {
            if (AssetDict.TryGetValue(assetName, out var cachedAsset))
            {
                return cachedAsset;
            }

#if UNITY_EDITOR
            Object asset = null;
    
            // 模式1：直接路径加载
            if (assetName.StartsWith("Assets/") && System.IO.File.Exists(assetName))
            {
                asset = AssetDatabase.LoadAssetAtPath<Object>(assetName);
            }
            // 模式2：可寻址资源搜索
            else
            {
                // 智能搜索策略
                var guids = AssetDatabase.FindAssets($"t:Prefab {assetName}"); // 限定预制体类型
                if (guids.Length > 0)
                {
                    // 优先精确匹配，其次包含匹配
                    var orderedAssets = guids
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .OrderBy(path => 
                        {
                            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                            return (fileName == assetName) ? 0 : 1; // 精确匹配优先
                        })
                        .ThenBy(path => path.Length); // 路径短的优先

                    var targetPath = orderedAssets.First();
                    asset = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
                }
            }

            // 资源验证和警告
            if (asset == null)
            {
                Debug.LogWarning($"Resource not found: {assetName}");
                return new AssetObject(assetName, null); // 返回空对象避免重复查找
            }

            // 记录实际加载路径
            var actualPath = AssetDatabase.GetAssetPath(asset);
            var assetObject = new AssetObject(actualPath, asset);
    
            // 双缓存策略（支持原名和实际路径两种访问方式）
            AssetDict[assetName] = assetObject;
            if (assetName != actualPath)
            {
                AssetDict[actualPath] = assetObject;
            }
    
            return assetObject;
#else
    // 正式构建时使用Addressables系统
    // 此处需接入Addressables或其他资源管理系统
    throw new System.NotImplementedException("Runtime asset loading not implemented");
#endif
        }
        public BundleObject LoadBundle(string bundleName, bool isAsync = false)
        {
           return null;
        }

        public void UnloadAsset(string assetName)
        {
        }

        public void UnloadBundle(string bundleName)
        {
        }

        public void UnloadUnusedAssets()
        {
        }

        public void Initialize(string manifestfilePath, string abPrefix, ResItemLis resourceList)
        {
            AssetDict = new Dictionary<string, AssetObject>();
        }
    }
}