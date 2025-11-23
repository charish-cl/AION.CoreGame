using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
//这个导入资源的工具也是一把好手呀，资源管理相关你看看   
 class ImportImageTipWindow : OdinEditorWindow
    {
        [Unity.Collections.ReadOnly]
        [LabelText("项目资源路径")]
        public string uiRawImagePath = "Assets/UIRaw/";
        [Unity.Collections.ReadOnly]
        [LabelText("美术图片路径")]
        public string uiImagePath = "E:\\design\\";

        [MenuItem("Tools/Import Image Tip &i", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<ImportImageTipWindow>();
            window.titleContent = new GUIContent("Import Image Tip");
            window.Show();
        }

        
        [BoxGroup("图片")] [LabelText("图片路径(svn的路径)")] [Multiline(lines:10)]
        public string ImagePaths;
        public string GetReplacePath(string svnPath = "")
        {
            if (string.IsNullOrEmpty(svnPath))
            {
               return "";
            }
            return  svnPath.Replace("svn://192.168.1.9/x6game/design", uiImagePath);
        }
        [BoxGroup("图片")]
        [Button("打开文件夹", ButtonSizes.Large)]
        public void OpenFolder()
        {
            var copyPathList = Regex.Split(ImagePaths, @"\s+").ToList();
            foreach (var s in copyPathList)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }
                var path = GetReplacePath(s);
                if (Directory.Exists(path))
                {
                    Application.OpenURL(path);
                }
                else
                {
                    SVNTool.UpdatePaths(new List<string>()
                    {
                        path,
                    });
                    Debug.LogWarning($"找不到{path}");
                }
            }
        }
        [BoxGroup("图片")]
        [Button("批量导入图片", ButtonSizes.Large)]
        public void ImportImgCopyUnityPackage()
        {
            //空格或者换行
            var copyPathList = Regex.Split(ImagePaths, @"\s+").ToList();

            List<ImageInfo> imageInfos = new List<ImageInfo>();

            foreach (var s in copyPathList)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }
                //目录
                if (Directory.Exists(s))
                {
                    //copy目录下所有资源
                    var files = Directory.GetFiles(s, "*", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        imageInfos.Add(new ImageInfo()
                        {
                            SourcePath = GetReplacePath(file),
                        });
                    }
                }
                //文件
                else
                {
                    imageInfos.Add(new ImageInfo()
                    {
                        SourcePath = GetReplacePath(s),
                    });
                }
            }

            Init(imageInfos);
        }


        //绿色背景
        [GUIColor("#00FF00")]  [TableList] [LabelText("新增图片列表")]
        public List<ImageInfo> addImageInfoList;


        //蓝色背景
        [GUIColor("#007ACC")]  [TableList]  [LabelText("修改的图片列表")]
        public List<ImageInfo> ModifiedImageInfoList;


        public Dictionary<string, string> folderMap = new Dictionary<string, string>();

        [PropertyOrder(100)]
        [Button("执行", ButtonSizes.Large)]
        public void Excecute()
        {
            if (addImageInfoList == null || addImageInfoList.Count == 0)
            {
                Debug.Log("没有新增图片");
            }

            if (ModifiedImageInfoList == null || ModifiedImageInfoList.Count == 0)
            {
                Debug.Log("没有修改图片");
            }

            foreach (var imageInfo in addImageInfoList)
            {
                if (string.IsNullOrEmpty(imageInfo.TargetFolder))
                {
                    if (folderMap.TryGetValue(imageInfo.SourceFolder, out var value))
                    {
                        imageInfo.SetTargetFolder(value);
                    }
                    else
                    {
                        Debug.LogWarning($"找不到{imageInfo.Name}的目录");
                        continue;
                    }
                }

                //看看目标路径是否存在
                if (!Directory.Exists(imageInfo.TargetFolder))
                {
                    Debug.Log($"自动创建目录{imageInfo.TargetFolder}");
                    Directory.CreateDirectory(imageInfo.TargetFolder);
                }

                MoveOrOverriderImage(imageInfo.SourcePath, imageInfo.TargePath);
            }

            foreach (var imageInfo in ModifiedImageInfoList)
            {
                MoveOrOverriderImage(imageInfo.SourcePath, imageInfo.TargePath);
            }
            AssetDatabase.Refresh();
        }

        public void Init(List<ImageInfo> imageInfoList)
        {
            addImageInfoList = new List<ImageInfo>();
            ModifiedImageInfoList = new List<ImageInfo>();
            if (folderMap == null)
            {
                folderMap = new Dictionary<string, string>();
            }
            for (var i = 0; i < imageInfoList.Count; i++)
            {
                var imageInfo = imageInfoList[i];
                //不是新增
                if (TryGetImagePath(imageInfo.Name, out var path))
                {
                    imageInfo.SetTargetFolder(Path.GetDirectoryName(path));
                    folderMap[imageInfo.SourceFolder] = imageInfo.TargetFolder;
                    ModifiedImageInfoList.Add(imageInfo);
                }
                else
                {
                    addImageInfoList.Add(imageInfo);
                }
            }

            Dictionary<string, string> mostFolderDict = new Dictionary<string, string>();
            for (var i = 0; i < addImageInfoList.Count; i++)
            {
                var imageInfo = addImageInfoList[i];
                if (mostFolderDict.TryGetValue(imageInfo.SourceFolder, out var value))
                {
                    folderMap[imageInfo.SourceFolder] = value;
                    imageInfo.SetTargetFolder(value);
                    continue;
                }

                var folder = GetImageMostFolder(imageInfo.SourceFolder);
                if (string.IsNullOrEmpty(folder))
                {
                    folderMap.TryAdd(imageInfo.SourceFolder, "");
                    Debug.LogWarning($"找不到{imageInfo.Name}的目录");
                    continue;
                }

                mostFolderDict[imageInfo.SourceFolder] = folder;
                folderMap[imageInfo.SourceFolder] = folder;
                imageInfo.SetTargetFolder(folder);
            }
        }
        [BoxGroup("日志")] [LabelText("svn的路径")] [Multiline(lines:3)]
        public string LogPaths;
        
        [BoxGroup("日志")]
        [Button("打开多行日志", ButtonSizes.Large)]
        public void OpenMultiLog()
        {
            //空格或者换行
            var copyPathList = Regex.Split(LogPaths, @"\s+").ToList();
            List<string> logPaths = copyPathList.Where(x => !string.IsNullOrEmpty(x)).ToList();
            
            SVNTool.Log(logPaths);
        }

        // [ButtonGroup("SVN")]
        // [Button("更新切图资源", ButtonSizes.Large)]
        // [GUIColor(0.4f, 0.8f, 1f)]
        // public void UpdateArtSVN()
        // {
        //     SVNTool.UpdatePaths(new List<string>()
        //     {
        //         "E:/design/UI/UI切图输出资源",
        //     });
        // }

        [ButtonGroup("SVN")]
        [Button("更新指定资源", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void UpdateTargetFolder()
        {
            SVNTool.UpdatePaths(folderMap.Keys.ToList());
        }
        
        [ButtonGroup("SVN")]
        [Button("打开日志", ButtonSizes.Large)]
        public void OpenLog()
        {
            SVNTool.Log(new List<string>() { "E:/design/UI/UI切图输出资源" });
        }
        
        [ButtonGroup("SVN")]
        [Button("提交图片资源", ButtonSizes.Large)]
        public void CommitArtSVN()
        {
            SVNTool.CommitPaths(new List<string>() { uiRawImagePath });
        }

        public bool TryGetImagePath(string fileName, out string path)
        {
            var guid = AssetDatabase.FindAssets(fileName, new[] { uiRawImagePath }).FirstOrDefault();
            if (string.IsNullOrEmpty(guid))
            {
                path = "";
                return false;
            }

            path = AssetDatabase.GUIDToAssetPath(guid);
            return !string.IsNullOrEmpty(path);
        }

        public void MoveOrOverriderImage(string file, string newPath)
        {
            if (!File.Exists(newPath))
            {
                File.Copy(file, newPath);
                Debug.Log($"<color=green>复制资源 {newPath}</color>");
            }
            else
            {
                Debug.Log($"<color=blue>覆盖资源 {newPath} </color>");
                File.Copy(file, newPath, true);
            }
        }

        //获取目录图片大多数所在的目录
        public string GetImageMostFolder(string folderName)
        {
            Dictionary<string, int> folderSet = new Dictionary<string, int>();
            var files = Directory.EnumerateFiles(folderName, "*.png", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                var guids = AssetDatabase.FindAssets(name, new[] { uiRawImagePath });
                if (guids.Length == 0)
                {
                    continue;
                }
                else
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var folder = Path.GetDirectoryName(path);
                    if (folder != null)
                    {
                        if (folderSet.ContainsKey(folder))
                        {
                            folderSet[folder] += 1;
                        }
                        else
                        {
                            folderSet[folder] = 1;
                        }
                    }
                }
            }

            if (folderSet.Count == 0)
            {
                return "";
            }

            return folderSet.OrderByDescending(x => x.Value).First().Key;
        }
        [ButtonGroup("资源控制")]
        [Button("更新选中下的图片", ButtonSizes.Large)]
        public void UpdateSelectFolderImage()
        {
            var obj =Selection.objects[0];
    
            var file = Directory.EnumerateFiles(uiImagePath, "*.png", SearchOption.AllDirectories)
                .First(e => Path.GetFileNameWithoutExtension(e) == obj.name);
          
            string newPath = AssetDatabase.GetAssetPath(obj);
            MoveOrOverriderImage(file, newPath);
            
            AssetDatabase.Refresh();
        }

        [ButtonGroup("资源控制")]
        [Button("打开选中图片所在文件夹", ButtonSizes.Large)]
        public void OpenSelectImageParentFolder()
        {
            var go = Selection.activeObject;
            string name = go.name;
            var files = Directory
                .EnumerateFiles(uiImagePath, "*.png", SearchOption.AllDirectories).First(e => Path.GetFileNameWithoutExtension(e) ==name);
            if (string.IsNullOrEmpty(files))
            {
                EditorUtility.DisplayDialog("错误", "没有找到图片", "确定");
                return;
            }
            Application.OpenURL(Path.GetDirectoryName(files));
        }
        
        [ButtonGroup("资源控制")]
        [Button("更新选中图片所在文件夹下的图片", ButtonSizes.Large)]
        public void UpdateSelectImageParentFolder(bool isDelete = false)
        {
            var   selectGameObject = Selection.activeObject;
            if (selectGameObject == null)
            {
                Debug.LogWarning("请先选择一个GameObject");
                return;
            }
            var path = AssetDatabase.GetAssetPath(selectGameObject);
            var imageFolder = Path.GetDirectoryName(path);
            var imagePath = GetUIImagePathInArtSVN(path);
            var parentFolder = Path.GetDirectoryName(imagePath);
            
            CopyFolderImage(parentFolder, imageFolder , isDelete);
            
            AssetDatabase.Refresh();
        }

        private string effectFolder = "E:/";
        
        [ButtonGroup("特效资源控制")]
        [Button("更新特效资源", ButtonSizes.Large)]
        public void UpdateEffect()
        {
            //空格或者换行
            var copyPathList = Regex.Split(ImagePaths, @"\s+").ToList();
            for (var i = 0; i < copyPathList.Count; i++)
            {
                if (string.IsNullOrEmpty(copyPathList[i]))
                {
                    continue;
                }
                copyPathList[i] = copyPathList[i].Replace("svn://192.168.1.9/x6game/", effectFolder);
                
                Debug.Log($"更新特效资源{copyPathList[i]}");
                
                if (!Directory.Exists(copyPathList[i]))
                {
                    //创建目录
                    Debug.Log($"创建目录{copyPathList[i]}");
                    Directory.CreateDirectory(copyPathList[i]);
                }
            }
         
            SVNTool.UpdatePaths(copyPathList);
            // var window = EditorWindow.GetWindow<ImportPackageCombineWindow>();
            // window.ImportPackage(copyPathList[0]);
        }
        
        public string GetUIImagePathInArtSVN(string imagePath)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var files = Directory.EnumerateFiles(uiImagePath, "*.png", SearchOption.AllDirectories)
                .FirstOrDefault(e => imageName==Path.GetFileNameWithoutExtension(e));
            if (string.IsNullOrEmpty(files))
            {
                Debug.LogWarning($"没有找到图片{Path.GetFileNameWithoutExtension(imagePath)}");
                return "";
            }
            return files;
        }
        public void CopyFolderImage(string folderName, string newFolderName,bool isDelete = false)
        {
            var  imageFiles = Directory.EnumerateFiles(folderName, "*.png", SearchOption.TopDirectoryOnly);

            HashSet<string> moveIamges = new HashSet<string>();
            foreach (var imageFile in imageFiles)
            {
                moveIamges.Add( Path.GetFileNameWithoutExtension(imageFile));
                MoveOrOverriderImage(imageFile, Path.Combine( newFolderName, Path.GetFileName(imageFile)));
            }

            if (isDelete)
            {
                //这里要把不存在的删除掉
                var   oldImageFiles = Directory.EnumerateFiles(newFolderName, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var oldImageFile in oldImageFiles)
                {
                    var oldImageName = Path.GetFileNameWithoutExtension(oldImageFile);
                    if (!moveIamges.Contains(oldImageName))
                    {
                        Debug.Log($"<color=red>删除资源 {oldImageFile} </color>");
                        AssetDatabase.DeleteAsset(oldImageFile);
                    }
                }
                AssetDatabase.Refresh();
            }
        
        }
        const string PSDPATH = "E:\\design\\UI\\UI设计资源\\源文件";
        
        [ValueDropdown("GetPSDPaths")]
        public string psdPath;

        public IEnumerable<string> GetPSDPaths()
        {
            var files = Directory.EnumerateFiles(PSDPATH, "*.psd", SearchOption.AllDirectories);
            return files;
        }

        [Button("导入PSD文件", ButtonSizes.Large)]
        public void  ImportPSD()
        {
            if (string.IsNullOrEmpty(psdPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a PSD file first.", "OK");
                return;
            }

            // var window = GetWindow<GenUIEditor>();
            // window.ParsePSD(psdPath);
            // AssetDatabase.Refresh();    
        }

        
        //打开路径
        [Button("打开Psd路径", ButtonSizes.Large)]
        public void OpenPath()
        {
            Application.OpenURL(PSDPATH);
        }

        #region 版本控制

        [ButtonGroup("SVN")]
        [Button("更新切图资源", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void UpdateArtSVN()
        {
            SVNTool.UpdatePaths(new List<string>()
            {
                "E:/design/UI/UI切图输出资源",
            });
        }
        
        [ButtonGroup("SVN")]
        [Button("更新PSd资源", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void UpdatePSDSVN()
        {
            SVNTool.UpdatePaths(new List<string>()
            {       
                "E:/design/UI/UI设计资源/源文件",
            });
        }
        
        // [ButtonGroup("SVN")]
        // [Button("更新特效资源", ButtonSizes.Large)]
        // [GUIColor(0.4f, 0.8f, 1f)]
        // public void UpdateUISVN()
        // {
        //     SVNTool.UpdatePaths(new List<string>()
        //     {
        //         "E:/design/UI/UI资源",
        //     });
        // }   
        //

        #endregion
    }

    public class ImageInfo
    {
        [LabelText("图片源路径")] public string SourcePath;

        public string SourceFolder
        {
            get
            { 
                if (!IsPathSyntaxValid(SourcePath))
                {
                    Debug.LogError($"路径{SourcePath}不合法");
                    return "";
                }   
                string path = Path.GetDirectoryName(SourcePath);

                return path;
            }
        }
        public static bool IsPathSyntaxValid(string path)
        {
            // 检查是否包含无效字符
            char[] invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
                return false;
    
            try
            {
                // 尝试获取完整路径，这会检查一些格式问题
                string fullPath = Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                // 如果路径格式异常，会抛出异常
                return false;
            }
        }
        public string Name => Path.GetFileNameWithoutExtension(SourcePath);

        public string NameWithSuffix => Path.GetFileName(SourcePath);
        [Unity.Collections.ReadOnly] [LabelText("将要导入的位置")] public string TargetFolder;

        [Unity.Collections.ReadOnly] [LabelText("将要导入的完整路径")] public string TargePath;

        [Button("打开文件夹")]
        public void OpenFolder()
        {
            Application.OpenURL(SourceFolder);
        }
        public void SetTargetFolder(string folder)
        {
            TargetFolder = folder;
            TargePath = Path.Combine(folder, NameWithSuffix);
        }
    }