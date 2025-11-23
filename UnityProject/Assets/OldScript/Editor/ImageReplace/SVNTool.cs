using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

    class SVNTool
    {
        public static string CodePath => "Assets/../GameProject/GameLogic/src";
        
        [MenuItem("Assets/SVN工具/选中更新", false, 5)]
        private static void SvnToolUpdate()
        {
            List<string> path = GetSelectionAssetPaths();
            UpdatePaths(path);
        }

        [MenuItem("Assets/SVN工具/选中提交", false, 5)]
        private static void SvnToolCommit()
        {
            List<string> path = GetSelectionAssetPaths();
            CommitPaths(path);
        }

        [MenuItem("Assets/SVN工具/选中恢复", false, 5)]
        private static void SvnToolRevert()
        {
            List<string> path = GetSelectionAssetPaths();
            RevertPaths(path);
        }

        [MenuItem("Assets/SVN工具/显示日志", false, 5)]
        private static void SvnToolLog()
        {
            List<string> path = GetSelectionAssetPaths();
            if (path.Count == 0)
            {
                return;
            }
            string arg = "/command:log /closeonend:0 /path:\"";
            arg += path[0];
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN工具/提交代码 GameLogic Resources_UI Dll", false, 501)]
        private static void SvnToolCommit_Script()
        {
            //// 往上两级，包括数据配置文件      
            SvnCommandRun("/command:commit /closeonend:0 /path:\"Assets/../GameProject/GameLogic/src*Assets/LogicDll*Assets/Resources/UI\"");
        }

        [MenuItem("Assets/SVN工具/提交 Effect Resources", false, 501)]
        private static void SvnToolCommit_Effect()
        {
            //// 往上两级，包括数据配置文件      
            SvnCommandRun("/command:commit /closeonend:0 /path:\"Assets/Effect*Assets/Resources\"");
        }

        [MenuItem("Assets/SVN工具/提交 Atlas UIRaw Resources", false, 501)]
        private static void SvnToolCommit_Atlas()
        {
            //// 往上两级，包括数据配置文件      
            SvnCommandRun("/command:commit /closeonend:0 /path:\"Assets/Atlas*Assets/UIRaw*Assets/Resources\"");
        }

        [MenuItem("Assets/SVN工具/全部更新", false, 1000)]
        private static void SvnToolAllUpdate()
        {
            // 往上两级，包括数据配置文件     
            string arg = "/command:update /closeonend:0 /path:\"";
            arg += ".\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN工具/全部恢复", false, 1001)]
        private static void SvnToolAllRevert()
        {
            // 往上两级，包括数据配置文件      
            string arg = "/command:revert /closeonend:0 /path:\"";
            arg += ".\"";
            SvnCommandRun(arg);
        }

        public static void Log(List<string> paths)
        {
            if (paths.Count == 0)
            {
                return;
            }
            string arg = "/command:log /closeonend:0 /path:\"";
            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += path;
            }
            arg += "\"";
            SvnCommandRun(arg);
        }

        // SVN更新指定的路径        
        // 例：Assets/XXX.png                   
        public static void UpdatePath(string assetPath)
        {
            List<string> assetPaths = new List<string>();
            assetPaths.Add(assetPath);
            UpdatePaths(assetPaths);
        }

        // SVN更新指定的路径                    
        public static void UpdatePaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }
            string arg = "/command:update /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg))
            {
                arg += " /logmsg:\"" + logmsg + "\"";
            }
            SvnCommandRun(arg);
        }

        // SVN提交指定的路径            
        public static void CommitPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }
            string arg = "/command:commit /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg))
            {
                arg += " /logmsg:\"" + logmsg + "\"";
            }
            SvnCommandRun(arg);
        }

        // SVN恢复指定的路径            
        public static void RevertPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }
            string arg = "/command:revert /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg))
            {
                arg += " /logmsg:\"" + logmsg + "\"";
            }
            SvnCommandRun(arg);
        }

        // SVN命令运行     
        private static void SvnCommandRun(string arg)
        {
            string workDirectory = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { UseShellExecute = false, CreateNoWindow = true, FileName = "TortoiseProc", Arguments = arg, WorkingDirectory = workDirectory });
        }

        // 获取选中路径列表
        static List<string> GetSelectionAssetPaths()
        {
            List<string> assetPaths = new List<string>();

            foreach (var guid in Selection.assetGUIDs)
            {
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    assetPaths.Add(path);
                }
            }
            return assetPaths;
        }
    }
