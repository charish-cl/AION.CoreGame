﻿
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
namespace GameDevKitEditor
{ [TreeWindow("文件工具/文件显示容量工具")]
public class FileCapacity : OdinEditorWindow
{
    private const string REMOVE_STR = "Assets";
    private const string FILESIZE = "FileSize";

    private static readonly int mRemoveCount = REMOVE_STR.Length;
    private static readonly Color mColor = new Color(0.635f, 0.635f, 0.635f, 1);
    private static Dictionary<string, string> DirSizeDictionary = new Dictionary<string, string>();
    private static List<string> DirList = new List<string>();
    private static bool isShowSize = true;

    [Button("打开")]
    private  void OpenPlaySize()
    {
        EditorPrefs.SetBool(FILESIZE, true);
        isShowSize = true;
        GetPropjectDirs();
    }

    [Button("关闭")]
    private static void ClosePlaySize()
    {
        EditorPrefs.SetBool(FILESIZE, false);
        isShowSize = false;
        Init();
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoadMethod()
    {
        // Debug.Log("[InitializeOnLoadMethod] 文件显示容量工具");

        Init();
        EditorApplication.projectChanged += GetPropjectDirs;
        EditorApplication.projectWindowItemOnGUI += OnGUI;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        GetPropjectDirs();
    }

    private static void GetPropjectDirs()
    {
        Init();
        if (isShowSize == false) return;
        GetAllDirecotries(Application.dataPath);
        foreach (string path in DirList)
        {
            string newPath = path.Replace("\\", "/");
            DirSizeDictionary.Add(newPath, GetFormatSizeString((int)GetDirectoriesSize(path)));
        }
    }
    private static void Init()
    {
        isShowSize = EditorPrefs.GetBool(FILESIZE);
        DirSizeDictionary.Clear();
        DirList.Clear();
    }

    private static void OnGUI(string guid, Rect selectionRect)
    {
        if (isShowSize == false) return;
        var dataPath = Application.dataPath;
        var startIndex = dataPath.LastIndexOf(REMOVE_STR);
        var dir = dataPath.Remove(startIndex, mRemoveCount);
        var path = dir + AssetDatabase.GUIDToAssetPath(guid);
        string text = null;
        if (DirSizeDictionary.ContainsKey(path))
        {
            text = DirSizeDictionary[path];
        }
        else if (File.Exists(path))
        {
            var fileInfo = new FileInfo(path);
            var fileSize = fileInfo.Length;
            text = GetFormatSizeString((int)fileSize);
        }
        else
        {
            return;
        }



        var label = EditorStyles.label;
        var content = new GUIContent(text);
        var width = label.CalcSize(content).x + 10;

        var pos = selectionRect;
        pos.x = pos.xMax - width;
        pos.width = width;
        pos.yMin++;
        GUI.Label(pos, text);
        if (Bg == null)
        {
            return;   
        }
        GUI.DrawTexture(pos, Bg);
        
    }

    public static Texture2D _bg;

    public static Texture2D Bg
    {
        get
        {
            if (_bg is null)
            {
                _bg = new Texture2D(256, 256);

                for (int y = 0; y < _bg.height; y++)
                {
                    for (int x = 0; x < _bg.width; x++)
                    {
                        _bg.SetPixel(x, y, new Color(0, 1, 0, 0.1f));
                    }
                }

                _bg.Apply();
            }

            return _bg;
        }
    }
    private static string GetFormatSizeString(int size)
    {
        return GetFormatSizeString(size, 1024);
    }

    private static string GetFormatSizeString(int size, int p)
    {
        return GetFormatSizeString(size, p, "#,##0.##");
    }

    private static string GetFormatSizeString(int size, int p, string specifier)
    {
        var suffix = new[] { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
        int index = 0;

        while (size >= p)
        {
            size /= p;
            index++;
        }

        return string.Format(
            "{0}{1}B",
            size.ToString(specifier),
            index < suffix.Length ? suffix[index] : "-"
        );
    }

    private static void GetAllDirecotries(string dirPath)
    {
        if (Directory.Exists(dirPath) == false)
        {
            return;
        }
        DirList.Add(dirPath);
        DirectoryInfo[] dirArray = new DirectoryInfo(dirPath).GetDirectories();
        foreach (DirectoryInfo item in dirArray)
        {
            GetAllDirecotries(item.FullName);
        }
    }

    private static long GetDirectoriesSize(string dirPath)
    {
        if (Directory.Exists(dirPath) == false)
        {
            return 0;
        }

        long size = 0;
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        foreach (FileInfo info in dir.GetFiles())
        {
            size += info.Length;
        }

        DirectoryInfo[] dirBotton = dir.GetDirectories();
        foreach (DirectoryInfo info in dirBotton)
        {
            size += GetDirectoriesSize(info.FullName);
        }
        return size;
    }
}

}
#endif