using System.Diagnostics;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Development;
using AION.CoreFramework;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    [TreeWindow("资源工具/替换资源工具")]
    public class ReplaceResouseWindow : OdinEditorWindow
    {
        [TabGroup("合并资源")] [TabGroup("一对一替换")] [LabelText("替换源对象（旧的需要被替换的）")]
        public Object findObj;

        [TabGroup("一对一替换")] [LabelText("目标对象（新对象）")]
        public Object newObj;

        [TabGroup("合并资源")] [LabelText("是否指定目录")]
        public bool IsSpecifyDirectory = false;

        [ShowIf(nameof(IsSpecifyDirectory))] [LabelText("指定目录")] [FolderPath]
        public List<string> SpecifyDirectorys = new List<string>();

        [ShowIf(nameof(IsSpecifyDirectory))] [LabelText("指定文件")] [Sirenix.OdinInspector.FilePath]
        public List<string> SpecifyFiles = new List<string>();


        public static ReplaceResouseWindow Instance;
        [MenuItem("Tools/打开资源替换界面")]
        public static void Open()
        {
            Instance=GetWindow<ReplaceResouseWindow>();
            Instance.Show();
        }

        #region 一对一替换
        /// <summary>
        /// 替换资源
        /// </summary>
        /// <param name="sourceObj">源Object</param>
        /// <param name="targetObj">目标Object</param>
        [TabGroup("一对一替换")]
        [Button("替换资源", ButtonHeight = 30)]
        public void ReplaceResource()
        {
            ResourcesReplace.StartReplace(findObj, newObj);
            AssetDatabase.Refresh();
        }

        #endregion
        #region 获取引用

        [TabGroup("获取引用")] [LabelText("查询结果")] public List<Object> result = new List<Object>();

        [TabGroup("获取引用")]
        [Button("获取引用", ButtonHeight = 30)]
        public bool CheckIsUnUse(Object sourceObj)
        {
            findObj = sourceObj;
            result = ResourcesReplace.GetReferenceResult(sourceObj);
            return result.Count == 0;
        }

        [TabGroup("获取引用")]
        [Button("Ping对象", ButtonHeight = 30)]
        public void PingObj()
        {
            ResourceDependenceHelper.FindDependentObjectsFromSelectObj(findObj);
        }

        #endregion


        #region 获取所有资源的所有引用
        
        [TabGroup("获取所有资源的所有引用","正向")]
        [TableList(ScrollViewHeight = 1000)]
        public List<ReferenceData> allObjReferenceSprite = new List<ReferenceData>();

        [TabGroup("获取所有资源的所有引用","反向")]
        [TableList(ScrollViewHeight = 1000)]
        public List<ReferenceData> AllSpriteRefrenceObj = new List<ReferenceData>();
        [TabGroup("获取所有资源的所有引用")]
        [Button("获取所有资源的所有引用", ButtonHeight = 30)]
        public void GetAllReference()
        {
            var assets = SelectionExtend.GetSelectFolderAllAsset();

            AllSpriteRefrenceObj.Clear();
            allObjReferenceSprite.Clear();
            
            Dictionary<Object, List<Object>> RefrencePrefab = new Dictionary<Object, List<Object>>();
            
            foreach (var asset in assets)
            {
                var referenceResult = ResourcesReplace.GetReferenceResult(asset);
                
                
                if (referenceResult != null && referenceResult.Count!=0)
                {
                    AllSpriteRefrenceObj.Add(new ReferenceData(asset,referenceResult));
                    foreach (var o in referenceResult)
                    {
                        if (!RefrencePrefab.ContainsKey(o))
                        {
                            RefrencePrefab.Add(o,new List<Object>());
                        }
                        RefrencePrefab[o].Add(asset);
                    }
                }
                //无用的资源
                else
                {
                    ClearResourse.Add(asset);
                }
            }

            allObjReferenceSprite=RefrencePrefab.Select(e => new ReferenceData(e.Key, e.Value)).ToList();
        }
        /// <summary>
        /// 替换图片设置Slice
        /// </summary>
        /// <param name="obj"></param>
        public static void Convert9Slide(Object obj)
        {
            if (Instance == null)
            {
                Instance=GetWindow<ReplaceResouseWindow>();
            }
            var referenceData = Instance.AllSpriteRefrenceObj.FirstOrDefault(e => e.rawAsset == obj);
            foreach (var editorDependence in referenceData.DependenceData)
            {
                //打开预制体窗口
               SelectionExtend.OpenPrefab(editorDependence.obj);
               
               editorDependence.Convert9SlidcerFromPrefab();
            }
            AssetDatabase.Refresh();
        }
        public class ReferenceData
        {
            public Object rawAsset;

            
            [ListDrawerSettings(ShowFoldout = false,DefaultExpandedState = true)]
            // [OnInspectorGUI("DrawPing")]
            [TableList]
            public List<EditorDependence> DependenceData;
            
            public class EditorDependence
            {
               // [OnInspectorGUI("DrawObj")]
                public Object obj;


                [HideInInspector]
                public Object Raw;
                
                public bool IsTextureRawAsset=>Raw is Texture2D;

                
                public EditorDependence(Object raw,Object obj)
                {
                    this.Raw = raw;
                    this.obj = obj;
                }

                [Button("Ping",ButtonHeight = 20)]
                public void Ping()
                {
                    ResourceDependenceHelper.FindPrefabDependentObjects(GetHandleSprite());
                }
                [Button("转化成9图",ButtonHeight = 20)]
                public void Convert9()
                {
                    var handle = GetHandleSprite();
                    ReplaceResouseWindow.Convert9Slide(handle);
                }

                private Object GetHandleSprite()
                {
                    var handle = obj;
                    if (IsTextureRawAsset)
                    {
                        handle = Raw;
                    }

                    return handle;
                }

                public void Convert9SlidcerFromPrefab()
                {
                    var handle = GetHandleSprite();
                    Convert9SlicerEditor.Convert9Slicer(handle);
                    ResourceDependenceHelper.FindPrefabDependentObjects(handle, (e) =>
                    {
                        //这里如果其它预制体也引用这张图片了，单独打开预制体进行替换
                        if (e.TryGetComponent<Image>(out var image ))
                        {
                            image.type = Image.Type.Sliced;
                        }
                        EditorUtility.SetDirty(e);
                    });
                    
                }
            }
            public ReferenceData(Object rawAsset, List<Object> dependenceData)
            {
                this.rawAsset = rawAsset;
                DependenceData = dependenceData.Select(e => new EditorDependence(rawAsset,e)).ToList();
            }
        }

       

        #endregion


        public List<Object> AssetDataBaseGetAllFolderAsset(string directoryPath)
        {
            // 获取目录下的所有资源GUID
            string[] guids = AssetDatabase.FindAssets("", new string[] { directoryPath });

            // 创建列表来存储所有资源
            List<Object> assetsList = new List<Object>();

            // 遍历所有资源GUID，并加载资源添加到列表中
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                if (asset != null)
                {
                    assetsList.Add(asset);
                }
            }

            return assetsList;
        }

        #region 合并资源

        [TabGroup("合并资源")] [Searchable] public List<List<Texture2D>> NeedMergeResources = new List<List<Texture2D>>();

        [TabGroup("合并资源")] [LabelText("选中目录")] public string DirectoryPath;


        [TabGroup("合并资源")]
        [Button("获取findobj的相似资源", ButtonHeight = 50)]
        public void GetSlectSimilarAsset()
        {
            var path = SelectionExtend.GetCurrentAssetDirectory();


            if (DirectoryPath != null)
            {
                path = DirectoryPath;
            }

            var assets = AssetDataBaseGetAllFolderAsset(path)
                .Where(e => e.GetType() == typeof(Texture2D))
                .Select(e => e as Texture2D).ToList();

            NeedMergeResources?.Clear();
            NeedMergeResources = new List<List<Texture2D>>();
            ImageComparer.imageCache.Clear();
            foreach (var texture2D in assets)
            {
                ImageComparer.AddImage(texture2D);
            }

            Dictionary<string, List<Texture2D>> groupedAssets = new Dictionary<string, List<Texture2D>>();
            var selectSprite = findObj as Texture2D;
            groupedAssets = ImageComparer.imageCache;
            // 将分组的资源添加到NeedMergeResources中
            foreach (var group in groupedAssets)
            {
                //只有一个不算是冗余资源
                if (group.Value.Count == 1)
                {
                    continue;
                }

                if (!ImageComparer.CompareImagesWithCache(selectSprite, group.Value[0]))
                {
                    continue;
                }

                NeedMergeResources.Add(group.Value);
            }
        }

        [TabGroup("合并资源")]
        [Button("获取当前打开文件夹下的相似资源", ButtonHeight = 50)]
        public void GetSimilarAsset()
        {
            var path = SelectionExtend.GetCurrentAssetDirectory();

            SetDisableReadable(path, true);

            var assets = AssetDataBaseGetAllFolderAsset(path)
                .Where(e => e.GetType() == typeof(Texture2D))
                .Select(e => e as Texture2D).ToList();

            NeedMergeResources?.Clear();
            NeedMergeResources = new List<List<Texture2D>>();
            ImageComparer.imageCache.Clear();
            foreach (var texture2D in assets)
            {
                ImageComparer.AddImage(texture2D);
            }

            Dictionary<string, List<Texture2D>> groupedAssets = new Dictionary<string, List<Texture2D>>();

            groupedAssets = ImageComparer.imageCache;
            // 将分组的资源添加到NeedMergeResources中
            foreach (var group in groupedAssets)
            {
                //只有一个不算是冗余资源
                if (group.Value.Count == 1)
                {
                    continue;
                }

                NeedMergeResources.Add(group.Value);
            }
        }
        [TabGroup("合并资源")]
        [Button("获取当前打开文件夹下的同名资源",ButtonHeight = 50)]
        public void GetSimilarNameAsset()
        {
            var path = SelectionExtend.GetCurrentAssetDirectory();
            // SetDisableReadable(path,true);
        
            var assets = AssetDataBaseGetAllFolderAsset(path)
                .Where(e=>e.GetType()==typeof(Texture2D))
                .Select(e=>e as Texture2D).ToList();
        
            NeedMergeResources?.Clear();
            NeedMergeResources = new List<List<Texture2D>>();
        
 
            // 将分组的资源添加到NeedMergeResources中
      
            // 对资源名称进行处理，去除空格
            var processedAssets = assets
                .Select(asset => new
                {
                    Asset = asset,
                    ProcessedName = asset.name.Split(" ").First()
                })
                .ToList();

            // 分组相似的图片
            var groupedAssets = processedAssets
                .GroupBy(x => new string(x.ProcessedName.Where(char.IsLetterOrDigit).ToArray()))
                .Where(g => g.Count() > 1)
                .Select(g => g.Select(x => x.Asset).ToList())
                .ToList();

            // 将分组的资源添加到NeedMergeResources中
            NeedMergeResources.AddRange(groupedAssets);
        
     
        }
        [TabGroup("合并资源")]
        [Button("替换NeedMergeResources Project中的依赖对象", ButtonHeight = 50)]
        public void ReplaceNeedMergeResourcesInProject()
        {
            ResourcesReplace.Files = null;
            if (IsSpecifyDirectory)
            {
                List<string> files = new List<string>();
                foreach (var directory in SpecifyDirectorys)
                {
                    string[] directoryFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                    files.AddRange(directoryFiles);
                }

                files.AddRange(SpecifyFiles);
                ResourcesReplace.Files = files.ToArray();
            }


            foreach (var resources in NeedMergeResources)
            {
                var sprites = resources.Select(e =>
                    AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(e))).ToArray();
                //查找所有依赖，替换为第一张sprite
                for (var index = 1; index < sprites.Length; index++)
                {
                    var s = sprites[index];
                    ResourcesReplace.StartReplace(s, sprites[0]);
                }

                //删除只保留第一个
                for (var i = sprites.Length - 1; i > 0; i--)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprites[i]));
                }
            }

            AssetDatabase.Refresh();
        }

        [TabGroup("合并资源")]
        [Button("替换NeedMergeResources场景中的依赖对象", ButtonHeight = 50)]
        public void ReplaceNeedMergeResourcesInScene()
        {
            foreach (var resources in NeedMergeResources)
            {
                var sprites = resources.Select(e =>
                    AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(e))).ToArray();

                //查找所有依赖，替换为第一张sprite
                foreach (var s in sprites)
                {
                    ResourceDependenceHelper.FindSceneDependentObjects(s, e =>
                    {
                        e.GetComponent<Image>().sprite = sprites[0];
                        e.name = sprites[0].name;
                    });
                }

                //删除只保留第一个
                for (var i = 1; i < sprites.Length; i++)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprites[i]));
                }

                AssetDatabase.Refresh();
            }
        }

        #endregion


        #region 清除资源

        [TabGroup("清除资源")] public List<Object> ClearResourse = new List<Object>(); // 依赖于资源的物体列表

        [TabGroup("清除资源")]
        [Button("获取资源")]
        public void GetUnUseResources()
        {
            var path = SelectionExtend.GetCurrentAssetDirectory();
            ClearResourse.Clear();
            var assets = AssetDataBaseGetAllFolderAsset(path)
                .Where(e => e.GetType() == typeof(Texture2D))
                .Select(e => e as Texture2D).ToList();
            foreach (var o in assets)
            { 
                //TODO：这里可以排除有用的资源
                ClearResourse.Add(o);
            }
        }

        [TabGroup("清除资源")]
        [Button("清除无用的")]
        public void ClearUnUseResources()
        {
            List<string> paths = new List<string>();

            ResourcesReplace.Reset();
            
            foreach (var o in ClearResourse)
            {
                paths.Add(AssetDatabase.GetAssetPath(o));
            }

            //允许您一次删除多个资产或文件夹，并在版本控制下获得性能优势。
            AssetDatabase.DeleteAssets(paths.ToArray(), new List<string>());

            AssetDatabase.Refresh();
        }

        #endregion
        
        public static  void SetDisableReadable(string folderPath ="", bool isReadable = false)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = IOUtility.GetCurrentAssetDirectory();
            }
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            
            for (int i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];

                var path = AssetDatabase.GUIDToAssetPath(guid);
                //如果图片本来就是可读的，就不用设置了

                
                var texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture2D.isReadable!=isReadable)
                {
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        importer.isReadable = isReadable;
                        importer.SaveAndReimport();
                    }
                }
         
       
            }
        }

    }
}