using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class JsonNewCardExtractor : OdinEditorWindow
{
    [LabelText("源 JSON 文件路径"), Sirenix.OdinInspector.FilePath]
    public string jsonFilePath;

    [LabelText("输出文件夹"), FolderPath]
    public string outputFolder;

    [LabelText("筛选关键字")]
    public string keyword = "【新卡片】";

    [Button("提取包含关键字的 Key-Value")]
    public void ExtractNewCardKeyValues()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON 文件路径无效");
            return;
        }

        var lines = File.ReadAllLines(jsonFilePath);
        var resultLines = new List<string>();

        foreach (var line in lines)
        {
            // 简单判断行内是否包含关键字
            if (line.Contains(keyword))
            {
                // 清除多余空格
                string cleanLine = line.Trim();

                // 这行一般形如: "Key": "Value",
                // 直接保留即可
                if (!string.IsNullOrEmpty(cleanLine))
                    resultLines.Add(cleanLine.TrimEnd(',')); // 去掉原来的逗号，稍后统一补
            }
        }

        if (resultLines.Count == 0)
        {
            Debug.Log("没有找到包含关键字的项。");
            return;
        }

        // 组合成新的标准 JSON 对象
        var newLines = new List<string>();
        newLines.Add("{");
        for (int i = 0; i < resultLines.Count; i++)
        {
            string line = resultLines[i];
            if (i < resultLines.Count - 1)
                newLines.Add($"    {line},");
            else
                newLines.Add($"    {line}");
        }
        newLines.Add("}");

        // 输出保存路径
        if (string.IsNullOrEmpty(outputFolder))
            outputFolder = Path.GetDirectoryName(jsonFilePath);
        string outputPath = Path.Combine(outputFolder, "Filtered_NewCard.json");

        File.WriteAllLines(outputPath, newLines);
        AssetDatabase.Refresh();

        Debug.Log($"提取完成，共 {resultLines.Count} 条，输出文件：{outputPath}");
    }

    [MenuItem("Tools/JsonNewCardExtractor")]
    public static void OpenWindow()
    {
        GetWindow<JsonNewCardExtractor>("提取【新卡片】工具");
    }
}