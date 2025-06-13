using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AION.CoreFramework
{
    public class BundleObject:ObjectBase
    {
        public AssetBundle bundle;
        
        public List<BundleObject> Dependencies;
        
        //异步加载ab的request
        AssetBundleCreateRequest request;
        
        public bool IsDone;
        
        public BundleObject(string bundleName,List<BundleObject> dependencies)
        {
            this.Dependencies = dependencies;
            this.Name = bundleName;
            this.IsDone = false;
        }


        public string BundlePath;
        private void LoadBundleAsync()
        {
            request = AssetBundle.LoadFromFileAsync(BundlePath);
        }
        public void LoadBundle()
        {
            bundle = AssetBundle.LoadFromFile(BundlePath);
        }
        
        
        public Object LoadAsset<T>(string AssetName) where T : Object
        {
            var asset = bundle.LoadAsset<T>(AssetName);
            return asset;
        }
        public AssetBundleRequest LoadAssetAsync(string assetName, Type type)
        {
            return bundle.LoadAssetAsync(assetName, type);
        }

        public bool Update()
        {
            if (IsDone)
            {
                return true;
            }

            if (request == null)
            {
                LoadBundleAsync();
                return false;
            }
            
            if (!request.isDone)
            {
                return false;
            }
            if (Dependencies!= null && Dependencies.Count > 0)
            {
                for (int i = 0; i < Dependencies.Count; i++)
                {
                    if (!Dependencies[i].IsDone)
                    {
                        return false;
                    }
                }
            }
            
            bundle = request.assetBundle;
            
            request = null;
            IsDone = true;
            return true;
        }
        
        public override void OnRelease()
        {
            bundle.Unload(true);
        }

     
    }
}