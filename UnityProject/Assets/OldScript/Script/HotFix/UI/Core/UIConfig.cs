// using System.Collections.Generic;
// using System.IO;
// using Sirenix.OdinInspector;
// using UnityEngine;
//
// namespace AION.CoreFramework
// {
//     [CreateAssetMenu]
//     public class UIConfig:SerializedScriptableObject
//     {
//         [ShowInInspector]
//         public const string UIComponentBasePath = "Assets/Game/UIComponent/";
//         [ShowInInspector]
//         public const string UIFormBasePath = "Assets/Game/UIForm/";
//         [ShowInInspector]
//         public const string UIFormJsonPath = "Assets/Script/HotFix/UI/Core/";
//         [ShowInInspector]
//         public const string UIFormJsonFileName = "UIFormJson.json";
//         [ShowInInspector]
//         public const string UIConfigPath = "Assets/Resources/Config/UIConfig.asset";
//
//         public enum UITypes
//         {
//             UIForm,
//             UIItem,
//             UIPopWindow
//         }
//         
//         [Button]
//         public void SetGroupLayer()
//         {
//             foreach (var keyValuePair in OpenConfigs)
//             {
//                keyValuePair.Value.GroupName = UIGroups[0].Name;
//             }
//         }
//         public List<UIGroup> UIGroups = new List<UIGroup>
//         {
//             new UIGroup { Name = "MainUI", Layer = 0 },
//             new UIGroup { Name = "NotReturnMainUI", Layer = 10 },
//             new UIGroup(){Name = "GuideUI",Layer = 20},
//             // new UIGroup { Name = "BuildUI", Layer = 10 },
//             // new UIGroup { Name = "Talent", Layer = 11 },
//             // new UIGroup { Name = "GetCommonRewardUI", Layer = 20 },
//             // new UIGroup { Name = "RandomEvent", Layer = 30 },
//             // new UIGroup { Name = "PopWindow", Layer = 40 },
//             // new UIGroup { Name = "RewardGroup", Layer = 50 },
//             // new UIGroup(){Name = "RewardBuff",Layer = 46},
//             new UIGroup { Name = "DebugUI", Layer = 60 }
//         };
//         
//         [LabelText("UI打开配置")]
//         // [ReadOnly]
//         [Searchable]
//         public Dictionary<string,OpenConfig> OpenConfigs = new Dictionary<string, OpenConfig>();
//         
//         //Add UIBindTool to OpenConfigs
//       
//         public void Add2OpenConfig(UIConfigBindTool bindTool)
//         {
//             // 检查绑定工具是否为空
//             if (bindTool == null)
//             {
//                 Debug.LogError("UIConfigBindTool is null.");
//                 return;
//             }
//
//             // 获取绑定工具的名称和 OpenConfig 对象
//             string uiName = bindTool.UIName;
//             OpenConfig openConfig = new OpenConfig
//             {
//                 UIFormName = bindTool.UIName,
//                 GroupName = bindTool.UIGroupName,
//                 Path = bindTool.Location,
//                 AllowMultiInstance = bindTool.AllowMultiInstance,
//                 IsPreLoad = bindTool.IsPreLoad,
//                 IsPreInstance =bindTool.IsPreInstance,
//                 CloseMainUI = bindTool.CloseMainUI
//             };
//
//             // 将 OpenConfig 添加到 OpenConfigs 字典中
//             OpenConfigs[uiName] = openConfig;
//
//
// #if UNITY_EDITOR
//             UnityEditor.EditorUtility.SetDirty(this);
// #endif
//         }
//     }
//
//     public class OpenConfig
//     {
//         public string UIFormName;
//         
//         public string GroupName;
//         
//         public string Path;
//         
//         public bool AllowMultiInstance = false;
//         
//         public bool IsPreLoad = false;
//
//         public bool IsPreInstance = false;
//
//         public bool CloseMainUI = false;
//     }
//     public class UIGroup
//     {
//         public string Name;
//
//         public int Layer;
//     }
// }