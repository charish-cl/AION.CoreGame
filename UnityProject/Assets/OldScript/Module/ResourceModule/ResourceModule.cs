using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework
{
    [DisallowMultipleComponent]
    public class ResourceModule: Module
    {
        private string RESOURCE_ASSET_NAME="Resource";
        private string BUNDLE_ASSET_NAME="Bundle";
        private string PrefixPath { get; set; }
        private string Platform { get; set; }

        [LabelText("是否是编辑器模式")]
        public bool IsEditor;
        
        private void Start()
        {
            // PrefixPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../AssetBundle")).Replace("\\", "/");
            // PrefixPath += $"/{Platform}";
            PrefixPath = "Assets/Game/Config";
          
        }

        private IResourceManager _resourceManager;

        public IResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    LoadConfig();
                }
                return _resourceManager;
            }
            private set
            {
                _resourceManager = value;
            }
        }


        public async void LoadConfig()
        {
            //编辑器模式下，直接加载资源
            if (IsEditor)
            {
                ResourceManager = new EditorResourceManager();
                ResourceManager.Initialize(null, null, null);
                Debug.Log("编辑器模式资源模块初始化成功！");
                return;
            }
            string manifestBunldeFile=Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/Build")).Replace("\\", "/");

            ResourceManager = ModuleImpSystem.GetModule<IResourceManager>();
            
            string resItemLisPath = Path.Combine(PrefixPath, $"{RESOURCE_ASSET_NAME}.json");
            
            var resItemLis = JsonUtility.FromJson<ResItemLis>(File.ReadAllText(resItemLisPath));

            if (resItemLis == null)
            {
                Debug.LogError($"资源配置文件不存在 {resItemLisPath}");
                return;
            }
            string abPrefix = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build")).Replace("\\", "/");
            ResourceManager.Initialize(manifestBunldeFile, abPrefix,resItemLis);
         
            
            Debug.Log($"资源模块初始化成功！ {resItemLis.mResList.Count} 个资源 ");
        }

        public T LoadAsset<T>(string assetUrl) where T : UnityEngine.Object
        {
            var asset = ResourceManager.LoadAsset(assetUrl);
            if (asset == null)
            {
                Debug.LogError($"资源加载失败 {assetUrl}");
                return null;
            }
            Debug.Log($"资源加载成功 {asset.target.GetType()} {asset.target}");
            return (T)asset.target;
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetUrl) where T : UnityEngine.Object
        {
            var assetObject = ResourceManager.LoadAsset(assetUrl, true);
            var assetObjectAwaiter = await assetObject.awaiter;
            
            var asset = (T)assetObject.target;
            Debug.Log($"资源加载成功 {asset.GetType()} {asset}");

            return asset;
        }
        private string GetFileUrl(string assetUrl)
        {
            return $"{PrefixPath}/{assetUrl}";
        }

        private string GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    throw new System.Exception($"未支持的平台:{Application.platform}");
            }
        }

        public void UnloadAsset(TextAsset textAsset)
        {
            
        }
    }
}