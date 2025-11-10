using System;
using System.Diagnostics;
using UnityEngine;

namespace GameDevKitEditor
{
    /// <summary>
    /// Unity编辑器主动执行cmd帮助类。
    /// </summary>
    public static class ShellHelper
    {
        public static void Run(string cmd, string workDirectory)
        {
            RunCommandImp(cmd, workDirectory);
        }

        public static void RunMultiple(params string[] commands)
        {
            // 根据平台选择适当的命令分隔符
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            string splitChar = ";";
#elif UNITY_EDITOR_WIN
            string splitChar = "&&";
#endif
            // 将命令数组连接成一个字符串
            string joinedCommands = string.Join(splitChar + " ", commands);
            // 运行连接后的命令
            RunCommandImp(joinedCommands, "");
        }

        private static void RunCommandImp(string cmd, string workDirectory)
        {
            System.Diagnostics.Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                string app = "cmd.exe";
                string arguments = "/c";
#endif
                ProcessStartInfo start = new ProcessStartInfo(app)
                {
                    Arguments = arguments + " \"" + cmd + "\"",
                    CreateNoWindow = true,
                    ErrorDialog = true,
                    UseShellExecute = false,
                    WorkingDirectory = workDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = System.Text.Encoding.GetEncoding("GBK"), // Windows 常见编码
                    StandardErrorEncoding = System.Text.Encoding.GetEncoding("GBK")
                };

                process.StartInfo = start;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        UnityEngine.Debug.Log(args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        UnityEngine.Debug.LogError(args.Data);
                };

                // 启动进程
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 设置超时时间 (5秒 = 5000 毫秒)
                const int timeoutMs = 1000;
                bool exited = process.WaitForExit(timeoutMs);

                if (!exited)
                {
                    UnityEngine.Debug.LogWarning($"命令执行超时（>{timeoutMs}ms），强制终止：{cmd}");
                    try
                    {
                        process.Kill(); // .NET 5+ 支持杀整个进程树
                    }
                    catch (Exception killEx)
                    {
                        UnityEngine.Debug.LogError($"终止进程失败: {killEx.Message}");
                    }
                }

                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                if (process != null)
                    process.Close();
            }
        }
    }
}