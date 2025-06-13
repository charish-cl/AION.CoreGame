using System;
using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    [UpdateModule]
    //TODO:避免加载重复资源问题
    public class ResourceManager:ModuleImp,IResourceManager
    {
        
        #region 初始化相关
        
        //需要初始化的资源
        Dictionary<string, List<string>> ResourceDependencyDic = new Dictionary<string, List<string>>();
        //资源对应的bundle
        Dictionary<string, string> AssetBundleDic = new Dictionary<string, string>();
        // bundle依赖管理信息
        AssetBundleManifest m_AssetBundleManifest;
        #endregion
        
        //已经加载的资源
        public Dictionary<string, AssetObject> m_AssetDict;
        public Dictionary<string, BundleObject> m_BundleDict;
        
        
        
        //资源池
        public ObjectPool<AssetObject> m_AssetPool;
        public ObjectPool<BundleObject> m_BundlePool;


        
        /// <summary>
        /// 需要卸载的资源
        /// </summary>
        public List<BundleObject> m_UnloadBundleList;
        public List<AssetObject> m_UnloadAssetList;
        
        
        
        public List<BundleObject> m_LoadingBundle;
        public List<AssetObject> m_LoadingAsset;

        public void Init( ) 
        {
            m_LoadingAsset = new List<AssetObject>();
            m_LoadingBundle = new List<BundleObject>();
            m_AssetDict = new Dictionary<string, AssetObject>();
            m_BundleDict = new Dictionary<string, BundleObject>();
            
            m_UnloadBundleList = new List<BundleObject>();
            m_UnloadAssetList = new List<AssetObject>();
            
            var poolModule = GameModule.Get<ObjectPoolModule>();
            m_AssetPool = poolModule.CreateObjectPool<AssetObject>();
            m_BundlePool = poolModule.CreateObjectPool<BundleObject>();
        }

        public string Prefix ;
        public void Initialize(string manifestfilePath,string abPrefix,ResItemLis resourceList)
        {
            Init();
            Prefix =abPrefix;
            
            AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(manifestfilePath);
            UnityEngine.Object[] objs = manifestAssetBundle.LoadAllAssets();

            if (objs.Length == 0)
            {
                throw new Exception($"{nameof(ResourceManager)}.{nameof(Initialize)}() AssetBundleManifest load fail.");
            }

            m_AssetBundleManifest = objs[0] as AssetBundleManifest;
            
            //填充初始化数据
            foreach (var item in resourceList.mResList)
            {
                //填充资源依赖信息
                if (item.DependList.Count > 0)
                {
                    ResourceDependencyDic.Add(item.ResName, item.DependList);
                }
                //所属的bundle
                if (!AssetBundleDic.ContainsKey(item.ResName))
                {
                    AssetBundleDic.Add(item.ResName, item.ABName);
                }
            }
            
        }
        
        //同步和异步只是实际加载的方式不同，加载流程是一样的
        public AssetObject LoadAsset(string assetName,bool isAsync = false)
        {
            //处理bundle ---> 加载依赖 ---> 加载资源
            
            //看看池子里面有没有这个资源
            if (m_AssetDict.ContainsKey(assetName))
            {
                return m_AssetPool.Spawn(assetName);
            }
            //先加载依赖
            BundleObject bundleObject=null;
            if (AssetBundleDic.ContainsKey(assetName))
            {
                var bundleName = AssetBundleDic[assetName];
                bundleObject = LoadBundle(bundleName,isAsync);
                Debug.Log($"Load bundle {bundleName} for {assetName}");
            }
            else
            {
                throw new Exception($"Can not find bundle for {assetName}");
            }
            
            List<AssetObject> dependencies = null;
            if (ResourceDependencyDic.ContainsKey(assetName))
            {
                dependencies = new List<AssetObject>();
                
                var resourceDependencies = ResourceDependencyDic[assetName];
                
                foreach (var deppendenc in resourceDependencies)
                {
                    var depAsset = LoadAsset(deppendenc,isAsync);
                    dependencies.Add(depAsset);
                }
            }
            //加载资源
            var assetObj = new AssetObject(assetName,bundleObject,dependencies);
            m_AssetDict.Add(assetName, assetObj);
            
            if (isAsync)
            {
                m_LoadingAsset.Add(assetObj);
                assetObj.awaiter = new LoadAssetAwaiter();
            }
            else
            {
                assetObj.LoadAsset();
            }
            m_AssetPool.Register(assetObj,true);
            return assetObj;
        }
        
        public BundleObject LoadBundle(string bundleName,bool isAsync = false)
        {
            //看看池子里面有没有这个资源
            if (m_BundleDict.TryGetValue(bundleName,out var bundleObj))
            {
                return m_BundlePool.Spawn(bundleName); 
            }
           
            List<BundleObject> bundleDependencies =null;
            var dependencies = m_AssetBundleManifest.GetDirectDependencies(bundleName);

            if (dependencies.Length > 0)
            {
                bundleDependencies = new List<BundleObject>();
                foreach (var dependency in dependencies)
                {
                    var depBundle = LoadBundle(dependency);
                    bundleDependencies.Add(depBundle);
                }
            }
            var bundleObject = new BundleObject(bundleName, bundleDependencies);
            m_BundleDict.Add(bundleName, bundleObject);
            
            //加载bundle,加上前缀
            string bundlePath = $"{Prefix}/{bundleName}";
            bundleObject.BundlePath = bundlePath;
            if (isAsync)
            {
                m_LoadingBundle.Add(bundleObject);
            }
            else
            {
                bundleObject.LoadBundle();
            }
            
            m_BundlePool.Register(bundleObject,true);
            return bundleObject;
        }
        

        public void UnloadAsset(string assetName)
        {
            m_UnloadAssetList.Add(m_AssetDict[assetName]);
        }
        
        public void UnloadBundle(string bundleName)
        {
            m_UnloadBundleList.Add(m_BundleDict[bundleName]);
        }
        
        public void UnloadUnusedAssets()
        {
          
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);
            if (m_LoadingBundle.Count > 0)
            {
                for (int i = m_LoadingBundle.Count - 1; i >= 0; i--)
                {
                    var bundle = m_LoadingBundle[i];
                    if (bundle.Update())
                    {
                        m_LoadingBundle.RemoveAt(i);
                    }
                }
            }
            
            if (m_LoadingAsset.Count > 0)
            {
                for (int i = m_LoadingAsset.Count - 1; i >= 0; i--)
                {
                    var asset = m_LoadingAsset[i];
                    if (asset.Update())
                    {
                        m_LoadingAsset.RemoveAt(i);
                    }
                }
            }
            
            
            if (m_UnloadAssetList.Count > 0)
            {
                foreach (var asset in m_UnloadAssetList)
                {
                    m_AssetPool.UnSpawn(asset);
                    
                    //依赖也要卸载
                    foreach (var dep in asset.Dependencies)
                    {
                        m_AssetPool.UnSpawn(dep);
                    }
                }
            }

            if (m_UnloadBundleList.Count>0)
            {

                foreach (var bundle in m_UnloadBundleList)
                {
                    m_BundlePool.UnSpawn(bundle);
                    //依赖也要卸载

                    foreach (var asset in bundle.Dependencies)
                    {
                        m_BundlePool.UnSpawn(asset);
                    }
                }
            }
        }

        internal override void Shutdown()
        {
            
        }
    }
}