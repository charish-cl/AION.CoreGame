using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace GameDevKitEditor
{
    public class SARedundanyChecker:OdinEditorWindow
    {
        [MenuItem("GameDevKit/图集冗余检测")]
        private static void OpenWindow()
        {
            GetWindow<SARedundanyChecker>().Show();
        }

        public string mABPath = "C:\\Users\\Administrator\\Desktop\\New\\chessgoWX\\AssetBundle\\Working\\WebGL";

        
        public string mManifestName  => mABPath.Replace("\\", "/").Split("/").Last();
            
        
        [LabelText("冗余图片")]
        public List<Sprite> mSprites = new List<Sprite>();
        [Button]
        public void Check()
        {
            GetAssetGenMap(mABPath, mManifestName);
            
            var assetPaths =  AssetDatabase.FindAssets("t:spriteatlas");
            mSprites.Clear();
            for (var i = 0; i < assetPaths.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetPaths[i]);
                var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                // Sprite[] sprites = new Sprite[spriteAtlas.spriteCount] ;
                // spriteAtlas.GetSprites(sprites);
                var dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(spriteAtlas)).ToDictionary(x => x.ToLower(), x => x.ToLower());
                //看这些图片是否在一个ab中
                var abNames = new List<string>();
                foreach (var assetPath in dependencies.Values)
                {
                    if (!mAssetGenMap.ContainsKey(assetPath))
                    {
                        continue;
                    }
                    var abName = mAssetGenMap[assetPath];
                    if (!abNames.Contains(abName))
                    {
                        
                        abNames.Add(abName);
                        
                        //如果有冗余，则提示
                        if (abNames.Count > 1)
                        {
                            mSprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath.ToUpper()));
                            Debug.LogWarningFormat($"存在冗余图集{spriteAtlas.name}： 图片{assetPath} 存在于多个AB中：{string.Join(",", abNames.ToArray())}"); 
                        }
                    }
                }

               
            }
        }
        
        Dictionary<string, string> mAssetGenMap = null;
        
        //适配项目打包（有加密） 或 原生打包
        AssetBundle CreateABAdapter(string path)
        {
            return AssetBundle.LoadFromFile(path);
        }
        bool GetAssetGenMap(string path, string maniFest)
        {
            mAssetGenMap = new Dictionary<string, string>();
            path = path.Replace("\\", "/");
            AssetBundle maniFestAb = CreateABAdapter(System.IO.Path.Combine(path, maniFest));
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
                AssetBundle ab = CreateABAdapter(filePath);
                foreach (string asset in ab.GetAllAssetNames())
                {
                    mAssetGenMap.Add(asset.ToLower(), abName);
                }
                foreach (string asset in ab.GetAllScenePaths())
                {
                    mAssetGenMap.Add(asset.ToLower(), abName);
                }
                ab.Unload(true);
            }

            if (mAssetGenMap.Count == 0)
                return false;

            return true;
        }
    }
}