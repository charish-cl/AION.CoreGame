using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    public class AssetObject:ObjectBase
    {
        
        public List<AssetObject> Dependencies;
        
        public BundleObject Bundle;
        
        public LoadAssetAwaiter awaiter;
        
        public AssetBundleRequest request;

        public bool IsDone { get; set; }
        
        public AssetObject(string name, BundleObject bundle,List<AssetObject> dependencies)
        {
            Name = name;
            Bundle = bundle;
            Dependencies = dependencies;
            IsDone = false;
            
        }
        
        public object target;

        public AssetObject(string name,object target)
        {
            this.target = target;
            Name = name;
        }

        private void LoadAssetAsync()
        {
            if (Bundle == null)
            {
                throw new System.Exception("AssetObject LoadAssetAsync failed, Bundle is null");
            }
      
            request = Bundle.LoadAssetAsync(Name, typeof(Object));
        }

        public Object LoadAsset()
        {
            target =  Bundle.LoadAsset<Object>(Name);
            
            awaiter.SetResult(this);
            
            IsDone = true;
            
            return (Object)target;
        }

        public bool Update()
        {
            if (IsDone)
            {
                return true;
            }
            if (!Bundle.IsDone)
            {
                return false;
            }

            if (request == null)
            {
                LoadAssetAsync();
                return false;
            }
            if (!request.isDone)
            {
              return false;
            }
            //Debug.Log("AssetObject Update");
            if (Dependencies!= null && Dependencies.Count > 0)
            {
                foreach (var dependency in Dependencies)
                {
                    if (!dependency.IsDone)
                    {
                        return false;
                    }
                }
            }
            
            LoadAsset();
            return true;
        }
        public override void OnRelease()
        {
            
        }
    }
}