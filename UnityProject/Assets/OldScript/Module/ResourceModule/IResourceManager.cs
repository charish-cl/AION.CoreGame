using System;
using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    public interface IResourceManager
    {  
        
        public AssetObject LoadAsset(string assetName,bool isAsync=false);
        public BundleObject LoadBundle(string bundleName,bool isAsync=false);
        public void UnloadAsset(string assetName);

        public void UnloadBundle(string bundleName);
        public void UnloadUnusedAssets();
        public void Initialize(string manifestfilePath, string abPrefix, ResItemLis resourceList);
    }
}