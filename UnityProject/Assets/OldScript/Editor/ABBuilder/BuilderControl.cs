using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AION.CoreFramework
{
    public class BuilderControl
    {
        
        public string BuildSettingPath = "Assets/Game/Config/BuilderSetting.asset";
        
        public string BuildOutputABPath = Application.dataPath + "/../Build";

        public string OutPutBuildData = "Assets/Game/Config";
        
        /// <summary>
        /// 防止重复打包
        /// </summary>
        public HashSet<string> buildAssetPaths = new HashSet<string>();



        public Dictionary<string,BuildDataDefine> bundleDic = new Dictionary<string,BuildDataDefine>();


        public Dictionary<string, ResItem> resItemDic = new Dictionary<string, ResItem>();
        
        
        
        public void Build()
        {
            //初始化BuildSetting
            LoadBuildSetting();

            //处理依赖关系
            LoadDepence();
            
            //保存配置文件
            SaveBuildData();
            
            //打ab
            BuildAssetBundle();
        }

        /// <summary>
        /// 保存资源依赖以及ab包里的资源保存到两份文件里
        /// </summary>
        private void SaveBuildData()
        {
           
            //bundleDic把存到json文件里
            SaveFile(OutPutBuildData + "/Bundle.json", JsonUtility.ToJson(new ABItemLis()
            {
                mABList = bundleDic.Values.ToList()
            }));
            //resItemDic存到json文件里
            SaveFile(OutPutBuildData + "/Resource.json", JsonUtility.ToJson(new ResItemLis()
            {
                mResList = resItemDic.Values.ToList()
            }));
            
            AssetDatabase.Refresh();
        }

        public void SaveFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        private void LoadDepence()
        {
            foreach (KeyValuePair<string,BuildDataDefine> keyValuePair in bundleDic)
            {
                var abItem = keyValuePair.Value;

                foreach (var s in abItem.AssetList)
                {
                    if (resItemDic.ContainsKey(s))
                    {
                        throw new Exception("Duplicate resource name!");
                    }
                    resItemDic.Add(s,new ResItem()
                    {
                        ResName = s.Replace("\\", "/"),
                        ABName = abItem.ABName,
                        DependList = GetDependencies(s).ToList()
                    });
                }
            }
        }

        private void BuildAssetBundle()
        {
            List<AssetBundleBuild> assetBundleBuilds=new List<AssetBundleBuild>();
            
            foreach (KeyValuePair<string,BuildDataDefine> keyValuePair in bundleDic)
            {
                assetBundleBuilds.Add(new AssetBundleBuild()
                {
                    assetBundleName = keyValuePair.Key,
                    assetNames = keyValuePair.Value.AssetList.ToArray()
                });
            }

            if (!Directory.Exists(BuildOutputABPath))
            {
                Debug.Log("创建Build输出目录");
                Directory.CreateDirectory(BuildOutputABPath);
            }
            BuildPipeline.BuildAssetBundles(BuildOutputABPath, assetBundleBuilds.ToArray(),
                BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }

        private void LoadBuildSetting()
        {
            var setting = AssetDatabase.LoadAssetAtPath<BuildSetting>(BuildSettingPath);
            if (setting == null)
            {
                throw new System.Exception("BuildSetting is null,please create it first!");
            }
            List<BuildItem> buildItems = new List<BuildItem>();
            buildItems.AddRange(setting.BuildItems);
            //按照路径排序,防止路径长的打包在前面
            buildItems.Sort((a, b) => a.bundlePath.Length.CompareTo(b.bundlePath.Length));
            
            for (var i = 0; i < buildItems.Count; i++)
            {
                if (string.IsNullOrEmpty(buildItems[i].bundlePath))
                {
                    throw new Exception("bundlePath is null or empty!");
                }
                LoadAsset( buildItems[i].bundleType,buildItems[i].bundlePath, buildItems[i].rules);
            }
        }

        public string GetABPath(string bundleName)
        {
            //大写转小写
            bundleName = bundleName.ToLower();
            bundleName = bundleName.Replace("\\", "/");
          
            return bundleName;
        }
        /// <summary>
        /// 根据路径和规则加载资源
        /// </summary>
        /// <param name="bundleType"></param>
        /// <param name="bundlePath"></param>
        /// <param name="rules"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LoadAsset(BundleType bundleType, string bundlePath, string rules)
        {
            List<string> filePaths = new List<string>();
         
            switch (bundleType)
            {
                case BundleType.File:
                    filePaths = GetSubFile(bundlePath,rules).ToList();
                    
                    //一个文件一个ab
                    foreach (var filePath in filePaths)
                    {
                        bundleDic.Add(filePath, new BuildDataDefine()
                        {
                            ABName = GetABPath(filePath),
                            //把自己添加进去
                            AssetList = new List<string> {filePath}
                        });
                    }
                    break;
                
                case BundleType.Directory:
                    var folders = GetDirectory(bundlePath);
                    
                    foreach (var folder in folders)
                    {
                        var file = GetSubFile(folder, rules);
                        if (file.Length==0)
                        {
                            continue;
                        }
                        bundleDic.Add(folder, new BuildDataDefine()
                        {
                            ABName = GetABPath(folder),
                            AssetList = file.ToList()
                        });
                    }
                    break;
                case BundleType.All:
                    filePaths = GetSubFile(bundlePath,rules).ToList();
                    bundleDic.Add(bundlePath, new BuildDataDefine()
                    {
                        ABName = GetABPath(bundlePath),
                        //把自己添加进去
                        AssetList = filePaths
                    });
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bundleType), bundleType, null);
            }
            
        }

        public string[] GetSubFile(string folder,string rules)
        {
            if (string.IsNullOrEmpty(rules))
            {
                rules="*";
            }
            string[] files = Directory.GetFiles(folder, rules, SearchOption.AllDirectories);
            
            //排除meta，cs文件
            files = files.Where(e => !e.EndsWith(".meta") && !e.EndsWith(".cs")).ToArray();

            //排除已经打到包里的资源
            files = files.Where(e => !buildAssetPaths.Contains(e)).ToArray();

            foreach (var file in files)
            {
                //加入到打包列表
                buildAssetPaths.Add(file);
            }
           
            //加入到打包列表
            return files;
        }
        public string[] GetDirectory(string folder)
        {
            string[] folders = Directory.GetFiles(folder, "*",SearchOption.TopDirectoryOnly);

            if (folders.Length == 0)
            {
                throw new System.Exception(folder+"子文件夹为空");
            }
            return folders;
        }
        public  string[] GetDependencies(string assetPath)
        {
            
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            //排除默认的依赖项，以Package开头的默认资源，那些会被打包到首包里
            dependencies = dependencies.Where(e => !e.StartsWith("Packages/")).ToArray();
            
            return dependencies;
        }
        
    }
}