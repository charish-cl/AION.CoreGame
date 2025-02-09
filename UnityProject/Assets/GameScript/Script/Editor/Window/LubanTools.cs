using System.Diagnostics;
using System.Text;
using GameDevKitEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace GameScripts.Editor
{
    [TreeWindow("Luban")]
    public class LubanTools:OdinEditorWindow
    {
        public static Process CreateCmdProcess(string cmd, string args, string workingDir = "")
        {
            var pStartInfo = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = args,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            
            if (!string.IsNullOrEmpty(workingDir))
                pStartInfo.WorkingDirectory = workingDir;

            return Process.Start(pStartInfo);
        }
    
        [Button("Luban 生成代码",ButtonHeight = 50)]
        public static void BuildLubanExcel()
        {
#if UNITY_EDITOR_WIN
            string batchFilePath = Path.Combine(Application.dataPath, "../Luban/gen.bat");
            batchFilePath = Path.GetFullPath(batchFilePath);
            var process = CreateCmdProcess("cmd.exe", "/c " + batchFilePath,Path.GetDirectoryName(batchFilePath));
#elif UNITY_EDITOR_OSX
            var process = CreateCmdProcess("sh", 
                "./Luban/gen.sh");
#endif
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                UnityEngine.Debug.Log("Output: " + eventArgs.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                UnityEngine.Debug.LogError("Error: " + eventArgs.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.WaitForExit();
            process.Close();
            UnityEngine.Debug.Log("Batch file execution completed.");
        }
        
        
        [Button("Luban 仅生成配置",ButtonHeight = 50)]
        public static void BuildLubanExcelData()
        {
#if UNITY_EDITOR_WIN
            string batchFilePath = Path.Combine(Application.dataPath, "../Luban/genData.bat");
            batchFilePath = Path.GetFullPath(batchFilePath);
            var process = CreateCmdProcess("cmd.exe", "/c " + batchFilePath,Path.GetDirectoryName(batchFilePath));
#elif UNITY_EDITOR_OSX
            var process = CreateCmdProcess("sh", 
                "./Luban/genData.sh");
#endif
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                UnityEngine.Debug.Log("Output: " + eventArgs.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                UnityEngine.Debug.LogError("Error: " + eventArgs.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.WaitForExit();
            process.Close();
            UnityEngine.Debug.Log("Batch file execution completed.");
        }
        
        [Button("打开表格目录",ButtonHeight = 50)]
        public static void OpenConfigFolder()
        {
            // OpenFolder.Execute(Application.dataPath + @"/../Luban/Datas/");
        }
    }
}