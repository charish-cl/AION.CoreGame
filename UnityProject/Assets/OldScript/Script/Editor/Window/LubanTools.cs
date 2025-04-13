using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GameConfig;
using GameConfig.item;
using GameDevKitEditor;
using Luban;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameScripts.Editor
{
    [TreeWindow("Luban")]
    public class LubanTools:OdinEditorWindow
    {
    
        [Button("Luban 生成代码",ButtonHeight = 50)]
        public  void BuildLubanExcel()
        {
          string filename = "gen_code_bin_to_project.sh";
          string workPath = Path.Combine(LubanPath,filename);
          Debug.Log(Path.GetFullPath(workPath));
          ShellHelper.Run("sh",workPath);
        }
        
        public string LubanPath =Path.Combine(Application.dataPath, "..", "..", "Configs", "GameConfig");
        
        [Button("打开表格目录",ButtonHeight = 50)]
        public static void OpenConfigFolder()
        {
            string configPath = Path.Combine(Application.dataPath, "..", "..", "Configs", "GameConfig", "Datas");
            Debug.Log(Application.dataPath);
            Debug.Log("Open Config Folder: " + configPath);

            // 检查路径是否存在
            if (Directory.Exists(configPath))
            {
                // 将路径转换为 file:// 协议格式
                string url = "file://" + configPath;
                Application.OpenURL(url);
            }
            else
            {
                Debug.LogError("Config folder does not exist: " + configPath);
            }
        }

        #region 用法



        [Button]
        public void LoadTest()
        {
            // 一行代码可以加载所有配置。 cfg.Tables 包含所有表的一个实例字段。
            var tables = new Tables(file =>  new ByteBuf(File.ReadAllBytes($"Assets/AssetRaw/Configs/bytes/{file}.bytes")));

// 访问普通的 key-value 表
            Debug.Log(tables.TbItem.Get(10001).Name);
// // 支持 operator []用法
           Debug.Log(tables.TbItem[10002].Name);
           
           Debug.Log("表引用表");
           Debug.Log(tables.TbBooty.GetById(1).RewardList_Ref[0].ToString());
           
        }
        

        #endregion
    }
}