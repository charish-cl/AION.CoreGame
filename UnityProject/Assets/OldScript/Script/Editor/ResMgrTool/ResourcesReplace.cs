using System.Threading.Tasks;

namespace GameDevKitEditor
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.Linq;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;

    /// <summary>
    /// 解决项目中 一样的资源（名字或者路径不同）存在两份的问题  （多人做UI出现的问题， 或者美术没有管理好资源）
    /// 如果是要替换资源的话， 那就直接替换好了
    /// 
    /// 以上可以这么操作的基础是，你的Unity项目内的.prefab .Unity 都可以直接用文本开看到数据，而不是乱码（二进制）。这一步很关键，怎么设置呢？
    /// 打开项目Unity编辑器：Edit —-> Project Settings —-> Editor 这样就会调到你的Inspector面板的Editor Settings 
    /// 设置 Asset Serialization 的Mode类型为：Force Text(默认是Mixed); 这样你就能看到你的prefab文件引用了哪些贴图，字体，prefab 等资源了
    /// </summary>
    public class ResourcesReplace
    {
        public static Dictionary<string, string> fileContents = new Dictionary<string, string>();


        private static List<string> _withoutExtensions;

        private static List<string> WithoutExtensions
        {
            get
            {
                if (_withoutExtensions == null)
                {
                    _withoutExtensions = new List<string>();
                    _withoutExtensions.Add(".unity");

                    _withoutExtensions.Add(".prefab");

                    _withoutExtensions.Add(".mat");

                    _withoutExtensions.Add(".asset");
                }

                return _withoutExtensions;
            }
        }

        private static string[] files;

        public static string[] Files
        {
            get
            {
                if (files == null)
                {
                    files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                        .Where(s => WithoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                }

                return files;
            }

            set => files = value;
        }


        public static void StartReplace(Object old, Object newObj)
        {
            var _oldGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(old));
            var _newGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newObj));

            Debug.Log($"{old.name} oldGUID = {_oldGuid}  {newObj.name} _newGuid = {_newGuid}");
           
            Find(_oldGuid, _newGuid);
        }

        private static void ReadFileContents()
        {
            foreach (var file in Files)
            {
                fileContents[file] = File.ReadAllText(file);
            }
        }

        public static void Reset()
        {
            Files = null;
            fileContents.Clear();
        }
        /// <summary>
        /// 查找  并   替换 
        /// </summary>
        /// <param name="_oldGuid"></param>
        /// <param name="_newGuid"></param>
        private static void Find(string _oldGuid, string _newGuid)
        {
            if (fileContents == null || fileContents.Count == 0)
            {
                ReadFileContents();
            }

            List<string> keysToUpdate = new List<string>();

            Parallel.ForEach(fileContents, kvp =>
            {
                if (Regex.IsMatch(kvp.Value, _oldGuid))
                {
                    Debug.Log("替换了资源的路径：" + kvp.Key + GetRelativeAssetsPath(kvp.Key));
                    keysToUpdate.Add(kvp.Key); // 记录需要更改的 key
                }
                else
                {
                    Debug.Log("查看了的路径：" + kvp.Key);
                }
            });

            foreach (var key in keysToUpdate)
            {
                var newContent = fileContents[key].Replace(_oldGuid, _newGuid);
                fileContents[key] = newContent; // 统一替换
            }

            foreach (var key in keysToUpdate.Distinct()) // 使用 Distinct 方法确保相同的文件路径只写入一次
            {
                File.WriteAllText(key, fileContents[key]);
            }


            // 在这里进行文件写入等操作
            Debug.Log("替换结束");
            
            AssetDatabase.Refresh();
            
        }


        private static string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "")
                .Replace('\\', '/');
        }

        public static List<Object> GetReferenceResult(Object sourceObj)
        {
            List<Object> results = new List<Object>();

            if (Files == null)
            {
                return results;
            }

            if (fileContents == null || fileContents.Count == 0)
            {
                ReadFileContents();
            }

            var _oldGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceObj));

            List<string> path = new List<string>();
            Parallel.ForEach(fileContents, e =>
            {
                if (Regex.IsMatch(e.Value, _oldGuid))
                {
                    path.Add(GetRelativeAssetsPath(e.Key));
                }
            });


            foreach (var s in path)
            {
                results.Add(AssetDatabase.LoadAssetAtPath<Object>(s));
            }

            return results;
        }
    }
}