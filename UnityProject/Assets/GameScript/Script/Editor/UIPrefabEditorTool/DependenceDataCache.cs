using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace GameDevKitEditor.UIPrefabEditorTool
{
    [CreateAssetMenu(fileName = "DependenceDataCache", menuName = "GameDevKit/DependenceDataCache", order = 1000)]
    public class DependenceDataCache:SerializedScriptableObject
    {
        
        public Dictionary<string, string> ABDic = new Dictionary<string,string>();
        
        public Dictionary<string, string> SpriteAtlasDict = new Dictionary<string, string>();

        
        public static DependenceDataCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<DependenceDataCache>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:DependenceDataCache")[0]));
                }

                return instance;
            }
        }

        private static DependenceDataCache instance;

        [Button("初始化" )]
        public void InitAll()
        {
            ABDic.Clear();
            SpriteAtlasDict.Clear();
            InitSpriteAtlasData();
            ABDic =InitABeData();
        }
        public List<string> GetDependenceAB(List<string> assetPaths)
        {
            HashSet<string> abSet = new HashSet<string>();
            for (var i = 0; i < assetPaths.Count; i++)
            {
                var path = assetPaths[i];
                if (ABDic.ContainsKey(path.ToLower()))
                {
                    var abName = ABDic[path.ToLower()];
                    if (!abSet.Contains(abName))
                        abSet.Add(abName);
                }
            }
            return abSet.ToList();
        }
        
        /// <summary>
        /// 获取依赖图集
        /// </summary>
        /// <param name="sprites"></param>
        /// <returns>依赖图集的路径</returns>
        public List<string> GetDependenceSpriteAtlas(List<Sprite> sprites)
        {
            HashSet<string> atlasSet = new HashSet<string>();
            
            //查看依赖图集所在图集
            foreach (var sprite in sprites)
            {
                var path = AssetDatabase.GetAssetPath(sprite);
                if (SpriteAtlasDict.ContainsKey(path)&&!atlasSet.Contains(SpriteAtlasDict[path]))
                {
                    var atlasPath = SpriteAtlasDict[path];
                    atlasSet.Add(atlasPath);
                }
            }
            return atlasSet.ToList();
        }
        
         void InitSpriteAtlasData()
        {
            SpriteAtlasDict = new Dictionary<string, string>();
            
            var assetPaths =  AssetDatabase.FindAssets("t:spriteatlas");
            
            for (var i = 0; i < assetPaths.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetPaths[i]);
                var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                //精灵的路径为key，图集的路径为value
                var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(spriteAtlas))
                    .ToDictionary(x => x, _ => path);
                foreach (var dependency in dependencies)
                {
                    SpriteAtlasDict.Add(dependency.Key, dependency.Value);
                }
            }    
        }

        public string mABPath ;
        public string mMainAb;
         Dictionary<string, string> InitABeData()
        {
            //路径为key，AB包名为value
            Dictionary<string, string> abDict = new Dictionary<string, string>();
            GenAssetMap(mABPath, mMainAb);
            bool GenAssetMap(string path, string maniFest)
            {
                path = path.Replace("\\", "/");
                AssetBundle maniFestAb = AssetBundle.LoadFromFile(System.IO.Path.Combine(path, maniFest));
                if (maniFestAb == null)
                    return false;
                AssetBundleManifest manifest = maniFestAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (manifest == null)
                    return false;

                string[] allBundles = manifest.GetAllAssetBundles();
                maniFestAb.Unload(true);
                foreach (string abName in allBundles)
                {
                    string filePath = System.IO.Path.Combine(path, abName);
                    AssetBundle ab = AssetBundle.LoadFromFile(filePath);
                    foreach (string asset in ab.GetAllAssetNames())
                    {
                        abDict.Add(asset.ToLower(), abName);
                    }
                    foreach (string asset in ab.GetAllScenePaths())
                    {
                        abDict.Add(asset.ToLower(), abName);
                    }
                    ab.Unload(true);
                }
                return true;
            }
            
            return abDict;
        }
    }
}