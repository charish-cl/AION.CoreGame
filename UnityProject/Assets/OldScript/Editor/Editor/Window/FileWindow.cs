using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKitEditor
{
    [TreeWindow("文件工具/常用文件夹")]
    public class FileWindow:OdinEditorWindow
    {
        [Button("打开Packages文件夹",ButtonHeight = 50)]
        public void OpenMain()
        {
            Application.OpenURL(Application.dataPath + "/../Packages");
        }
        [Button("打开Assets文件夹 Application.dataPath", ButtonHeight = 50)]
        public void OpenAssetsFolder()
        {
            string path = Application.dataPath;
            Debug.Log("Assets文件夹路径: " + path);
            Application.OpenURL(path);
        }

        [Button("打开Persistent Data文件夹", ButtonHeight = 50)]
        public void OpenPersistentDataFolder()
        {
            string path = Application.persistentDataPath;
            Debug.Log("Persistent Data文件夹路径: " + path);
            Application.OpenURL(path);
        }

        [Button("打开StreamingAssets文件夹", ButtonHeight = 50)]
        public void OpenStreamingAssetsFolder()
        {
            string path = Application.streamingAssetsPath;
            Debug.Log("StreamingAssets文件夹路径: " + path);
            Application.OpenURL(path);
        }

        [Button("打开Temporary Cache文件夹", ButtonHeight = 50)]
        public void OpenTemporaryCacheFolder()
        {
            string path = Application.temporaryCachePath;
            Debug.Log("Temporary Cache文件夹路径: " + path);
            Application.OpenURL(path);
        }


        [Button]
        public void OpenMapFolder()
        {
            
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/Game/DynamicResource/Map/map_1"));
        }
        
        public class Folder
        {   [TableColumnWidth(250)]
            [LabelText("描述")]
            public string Dec;
            [TableColumnWidth(250)]
            [FolderPath]
            [LabelText("路径")]
            public string Path;

            [Button("打开")]
            public void Open()
            {
                Application.OpenURL(Path);
            }
        }
    }
}