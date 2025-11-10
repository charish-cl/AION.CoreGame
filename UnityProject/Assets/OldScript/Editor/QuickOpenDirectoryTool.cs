using UnityEngine;
using UnityEditor;

public class QuickOpenDirectoryTool
{
    [MenuItem("提效工具/打开表格目录 %_F1")] // Ctrl+F1
    private static void OpenExcelDirectory()
    {
        OpenDirectory("QuickOpen_ExcelPath", "表格");
    }

    [MenuItem("提效工具/打开美术目录 %_F2")] // Ctrl+F2
    private static void OpenArtDirectory()
    {
        OpenDirectory("QuickOpen_ArtPath", "美术");
    }

    [MenuItem("提效工具/打开服务器目录 %_F3")] // Ctrl+F3
    private static void OpenServerDirectory()
    {
        OpenDirectory("QuickOpen_ServerPath", "服务器");
    }

    private static void OpenDirectory(string key, string dirName)
    {
        string path = EditorPrefs.GetString(key, "");
        
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.OpenFolderPanel($"选择{dirName}目录", "", "");
            if (!string.IsNullOrEmpty(path))
                EditorPrefs.SetString(key, path);
            else
                return;
        }
        EditorUtility.RevealInFinder(path+"/");
    }
}