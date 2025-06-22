using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


[Flags]
public enum HLogLevel
{
    Info = 1,
    Debug = 2 | Info,
    Warning = 4 | Info,
    Error = 8,
    All = Debug | Info | Warning | Error
}

//基本的输出方法，有log等级，有输出日志到控制台和文本文件的功能
public static class HLog
{
    public static Dictionary<HLogLevel, StringBuilder> LogDict = new Dictionary<HLogLevel, StringBuilder>()
    {
        {HLogLevel.Debug, new StringBuilder()},
        {HLogLevel.Info, new StringBuilder()},
        {HLogLevel.Warning, new StringBuilder()},
        {HLogLevel.Error, new StringBuilder()},
        {HLogLevel.All, new StringBuilder()}
    };

    static HLogLevel _hLogLevel = HLogLevel.All;

    public static void Debug(string message)
    {
        Log(HLogLevel.Debug, message);
    }

    public static void Info(string message)
    {
        Log(HLogLevel.Info, message);
    }

    public static void Warning(string message)
    {
        Log(HLogLevel.Warning, message);
    }

    public static void Error(string message)
    {
        Log(HLogLevel.Error, message);
    }

    public static void Log(HLogLevel level, string message)
    {
        if (!LogDict.ContainsKey(level))
        {
            LogDict.Add(level, new StringBuilder());
        }
        message = string.Format("{0}|{1}|", (object) level, (object) DateTime.Now.ToString("yy-MM-dd HH:mm:ss")) + message;

        foreach (var (key, value) in LogDict)
        {
            if (key.HasFlag(level))
            {
                value.AppendLine(message);
            }
        }

        if (_hLogLevel.HasFlag(level))
        {
            switch (level)
            {
                case HLogLevel.Debug:
                case HLogLevel.Info:
                    UnityEngine.Debug.Log(message);
                    break;
                case HLogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case HLogLevel.Error:
                    UnityEngine.Debug.LogError(message);
                    break;
            }
        }
    }

    //清除缓存
    public static void Clear()
    {
        foreach (var item in LogDict)
        {
            if (item.Value != null)
            {
                item.Value.Clear();
            }
        }
    }

    public static string buildLogPath = Application.dataPath + "/../Build/log";

    //输出日志到文件
    public static void SaveLog(string logName, HLogLevel level)
    {
        if (string.IsNullOrEmpty(logName))
        {
            return;
        }

        var logPath = Path.Combine(buildLogPath, logName);
        if (!Directory.Exists(buildLogPath))
        {
            Directory.CreateDirectory(buildLogPath);
        }

        using (StreamWriter writer = new StreamWriter(logPath, false, Encoding.UTF8))
        {
            //只输出指定等级的日志  Info是始终输出的，其余的看情况
            if (LogDict.TryGetValue(level, out var value))
            {
                writer.Write(value.ToString());
            }
        }
    }

    public static void OpenLog(string logName)
    {
        if (string.IsNullOrEmpty(logName))
        {
            return;
        }

        var logPath = Path.Combine(buildLogPath, logName);
        if (File.Exists(logPath))
        {
            System.Diagnostics.Process.Start(logPath);
            UnityEngine.Debug.Log("打开日志文件：" + logPath);
        }
        else
        {
            UnityEngine.Debug.LogWarning("日志文件不存在：" + logPath);
        }
    }

    public static void SetLevel(HLogLevel level)
    {
        _hLogLevel = level;
    }
}