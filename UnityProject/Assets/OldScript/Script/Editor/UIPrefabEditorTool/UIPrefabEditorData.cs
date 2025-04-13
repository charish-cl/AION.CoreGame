using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace GameDevKitEditor.UIPrefabEditorTool
{
    [CreateAssetMenu(fileName = "UIPrefabEditorData", menuName = "GameDevKit/UIPrefabEditorData", order = 0)]
    public class UIPrefabEditorData : SerializedScriptableObject
    {
        private const string Left = "Split/Left";
        private const string Right = "Split/Right";
            
      
        [LabelText("UI预制体")] 
        public Object UIPrefab;
        
        [HorizontalGroup("Split")]
        [VerticalGroup(Left)]
        [LabelText("依赖的精灵")]
        public List<UIFolderData> SpriteDependencies = new List<UIFolderData>();
        

        [VerticalGroup(Left)]
        [ReadOnly] [LabelText("依赖的预制体")]
        public List<Object> DependentPrefabs = new List<Object>();
        
        [VerticalGroup(Left)]
        [LabelText("依赖的图集")]
        public List<SpriteAtlas> DependentAtalas = new List<SpriteAtlas>();
        
        [VerticalGroup(Left)]
        [ReadOnly] [LabelText("依赖的AB包")]
        public List<string> DependentAB = new List<string>();


        

        [VerticalGroup(Right)]
        [LabelText("所有依赖信息")]
        [TextArea(minLines: 40, maxLines: 40)]
        public string AllInfo;
        
        [VerticalGroup(Right)]
        [LabelText("预估下载速度")]
        float DowloadSpeed => 1024 * 1024 *2;
        
        [VerticalGroup(Right)]
        [Button("更新依赖信息",ButtonSizes.Large)]
        public void UpdateDependentInfo()
        {
            //计算图片大小
            int totalSize = 0;
            int totalCount = 0;
            StringBuilder builder = new StringBuilder();
            foreach (UIFolderData folderData in SpriteDependencies)
            {
                foreach (Sprite sprite in folderData.Sprites)
                {
                    totalSize += sprite.texture.width * sprite.texture.height;
                }
                totalCount += folderData.Sprites.Count;
            }
            builder.AppendLine( $"依赖的精灵：{totalCount}张，大小：{totalSize / 1024.0f / 1024.0f:F2}MB");

            builder.AppendLine( $"依赖的图集：{DependentAtalas.Count}个");
            //计算依赖的图集
            for (var i = 0; i < DependentAtalas.Count; i++)
            {
                var atlas = DependentAtalas[i];
                var length = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(atlas);
                var s = GetFileSize(length);
                builder.AppendLine( $"图集{i+1}：{atlas.name}，{s}");
              
            }
            builder.AppendLine( $"依赖的ab包：{DependentAB.Count}个");
            //计算总大小
            long totalABSize = 0;
            //计算依赖的ab包
            foreach (var s in DependentAB)
            {
                var prefix = DependenceDataCache.Instance.mABPath;
               var size = System.IO.File.ReadAllBytes(Path.Combine(prefix, s)).Length;
                totalABSize += size;
                var fileInfo=GetFileSize(size);
                builder.AppendLine(s);
                builder.AppendLine(fileInfo);
            }
            builder.AppendLine( $"总大小：{GetFileSize(totalABSize)}");
            builder.AppendLine( $"预估下载速度：{DowloadSpeed / 1024.0f / 1024.0f:F2}MB/s");
            //计算下载ab包速度
           builder.AppendLine( $"预估下载时间：{totalABSize / DowloadSpeed:F2}秒");
            
            AllInfo = builder.ToString();
          
         
         
        }
        
        //获取选中文件大小
        public string GetFileSize(long fileSizeInBytes)
        {
            //小于1KB的直接显示字节数，大于1KB小于1MB的显示KB数，大于1MB的显示MB数
            string result = "";
            if (fileSizeInBytes < 1024)
            {
               result = ($"文件大小：{fileSizeInBytes} B");
            }
            else if (fileSizeInBytes < 1024 * 1024)
            {
                float fileSizeInKB = fileSizeInBytes / 1024f;
                result = ($"文件大小：{fileSizeInKB} KB");
            }
            else
            {
                float fileSizeInMB = fileSizeInBytes / (1024f * 1024f);
                result = ($"文件大小：{fileSizeInMB} MB");
            }
            return result;;
        }
       
       
        [VerticalGroup(Right)]
        [Button("获取依赖的精灵", ButtonSizes.Large)]
        public void GetSpriteDependencies()
        {
            SpriteDependencies.Clear();
            DependentAtalas.Clear();
            DependentPrefabs.Clear();
            DependentAB.Clear();
            
            Dictionary<string, UIFolderData> folderDict = new Dictionary<string, UIFolderData>();
            string[] paths = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(UIPrefab),false);
            foreach (string path in paths)
            {                
                if (path.Contains(".png") || path.Contains(".jpg"))
                {
                    string folderPath = System.IO.Path.GetDirectoryName(path);

                    if (!folderDict.ContainsKey(folderPath))
                    {
                        folderDict.Add(folderPath, new UIFolderData() { FolderPath = folderPath });
                    }
                  
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    folderDict[folderPath].Sprites.Add(sprite);
                    
                }
                else if (path.Contains(".prefab"))
                {
                    DependentPrefabs.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            }
            SpriteDependencies = new List<UIFolderData>(folderDict.Values);

            List<string> atlasSet = DependenceDataCache.Instance.GetDependenceSpriteAtlas(SpriteDependencies.SelectMany(x => x.Sprites).ToList());
            
            //查看依赖图集所在图集
            DependentAtalas = atlasSet.Select(x => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(x)).ToList();
            
            //获取依赖的AB包
            DependentAB = DependenceDataCache.Instance.GetDependenceAB(paths.ToList());
            
            UpdateDependentInfo();
        }

        [Serializable]
        public class UIFolderData
        {
            [FolderPath] [LabelText("精灵文件夹路径")] public string FolderPath;

            [LabelText("精灵文件名")] public List<Sprite> Sprites = new List<Sprite>();

            [Button("移动到新文件夹")]
            public void MoveToNewFolder([FolderPath] string newFolderPath)
            {
                string oldFolderPath = FolderPath;
                FolderPath = newFolderPath;

                foreach (Sprite s in Sprites)
                {
                    var path = AssetDatabase.GetAssetPath(s);
                    string fileName = System.IO.Path.GetFileName(path);
                    string newFile = System.IO.Path.Combine(FolderPath, fileName);
                    System.IO.File.Move(path, newFile);
                }
                AssetDatabase.Refresh();
            }
            
            [Button("复制到新文件夹")]
            public void CopyToNewFolder([FolderPath] string newFolderPath)
            {
                string oldFolderPath = FolderPath;
                FolderPath = newFolderPath;

                foreach (Sprite s in Sprites)
                {
                    var path = AssetDatabase.GetAssetPath(s);
                    string fileName = System.IO.Path.GetFileName(path);
                    string newFile = System.IO.Path.Combine(FolderPath, fileName);
                    System.IO.File.Copy(path, newFile, true);
                }
                
                AssetDatabase.Refresh();
            }

            public void ReplaceSprite()
            {
                
            }
        }
    }
}