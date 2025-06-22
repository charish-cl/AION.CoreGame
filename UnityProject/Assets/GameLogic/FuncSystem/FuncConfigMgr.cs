// using System.IO;
// using GameBase;
// using GameConfig;
// using Luban;
// using UnityEngine;
//
// namespace GameLogic
// {
//
//     public class FuncConfigMgr : Singleton<FuncConfigMgr>
//     {
//         
//         public FuncConfigMgr()
//         {
//             
//             // 一行代码可以加载所有配置。 cfg.Tables 包含所有表的一个实例字段。
//             var tables = new Tables(file =>  new ByteBuf(File.ReadAllBytes($"Assets/AssetRaw/Configs/bytes/{file}.bytes")));
//
// // 访问普通的 key-value 表
//             Debug.Log(tables.TbItem.Get(10001).Name);
// // // 支持 operator []用法
//             Debug.Log(tables.TbItem[10002].Name);
//             
//             
//             var list = tables.TbFuncConfig.DataList;
//             
//             foreach (var item in list)
//             {
//                 Debug.Log(item.Name);
//             }
//             
//             // Debug.Log();
//             
//         }
//         
//         
//         
//     }
// }