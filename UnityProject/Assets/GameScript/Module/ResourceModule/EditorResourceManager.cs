using System;
using System.Collections.Generic;
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
            if (AssetDict.ContainsKey(assetName))
            {
                return AssetDict[assetName];
            }
            
            #if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
            #endif
            var assetObject = new AssetObject(assetName, asset);
            AssetDict.Add(assetName, assetObject);
            return assetObject;
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