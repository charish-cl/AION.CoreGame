using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace Editor
{
    public class CommandTool
    {
        public static void InstallApk(string apkpath, bool IsTestOnRealDevice)
        {
            Debug.Log($" CommandTool InstallApk: {apkpath}");
            //调用adb命令安装apk
#if UNITY_EDITOR_WIN
		string adbPath =
 "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.17f1c1\\Editor\\Data\\PlaybackEngines\\AndroidPlayer\\SDK\\platform-tools\\adb.exe";
        string installCommand = "adb install -r " + Path.GetFullPath(apkpath);
        string connectCommand = "adb connect 127.0.0.1:21503";
        //先连接再安装,如果是真机测试,则不需要连接
        if (!IsTestOnRealDevice)
        {
            Process.Start("cmd.exe", $"/c {connectCommand}");
        }
        Process.Start("cmd.exe", $"/c {installCommand}");
#elif UNITY_EDITOR_OSX
            string installCommand = "install -r " + Path.GetFullPath(apkpath);
            string cmd =
                "/Volumes/PS3000/Applications/Unity/Hub/Editor/2022.3.17f1c1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb";
            Debug.Log("cmd " + cmd + installCommand);
            
            RunCommand(cmd, $" {installCommand}");
            // RunCommand(cmd, "shell am start -n com.nightq.MonopolyRush/com.google.firebase.MessagingUnityPlayerActivity");
#endif
        }

        /**
         * windows 会使用cmd执行
         * mac 直接执行
         */
        public static bool RunCommand(string command, string param, string workingDirectory="")
        {
            UnityEngine.Debug.Log($" {command} {param} ");
            Process myProcess = new Process();
            var pStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false, //必须为false
                RedirectStandardError = true, //必须为true
                RedirectStandardInput = false,
                RedirectStandardOutput = true, //必须为true
            };
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                pStartInfo.WorkingDirectory = workingDirectory;
            }
            
            
#if UNITY_EDITOR_WIN
            pStartInfo.FileName = "cmd.exe";
            pStartInfo.Arguments = $"/k {command} {param}";
            pStartInfo.StandardOutputEncoding = Encoding.GetEncoding("GB2312"); // 设置标准输出编码为GB2312
            pStartInfo.StandardErrorEncoding = Encoding.GetEncoding("GB2312"); // 设置标准错误编码为GB2312
#elif UNITY_EDITOR_OSX
            if (command.StartsWith("ossutil"))
            {
                command = "/usr/local/bin/ossutil";
            }

            pStartInfo.FileName = command;
            pStartInfo.Arguments = param;
#endif
     
            myProcess.StartInfo = pStartInfo;
            myProcess.Start();
     
          
            // Read the standard error of net.exe and write it on to console.
            if (myProcess.StandardOutput.Peek() != -1)
            {
                UnityEngine.Debug.Log(myProcess.StandardOutput.ReadToEnd());
            } 
            if (myProcess.StandardError.Peek() != -1)
            {
                Debug.LogError(myProcess.StandardError.ReadToEnd());
            }
            UnityEngine.Debug.Log($" {command} {param} end ");
            myProcess.WaitForExit();
            return true;
        }

    }
}