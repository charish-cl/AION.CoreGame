//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Sirenix.OdinInspector;
// using UnityEngine;
//
//
// namespace AION.CoreFramework
// {
//  
//     // [ExecuteAlways]
//     [RequireComponent(typeof(ComponentAutoBindTool))]
//     public class UIConfigBindTool :SerializedMonoBehaviour
//     {
//
//         [LabelText("UI类型")]
//         public UIConfig.UITypes UIType;
//         
//         [ReadOnly]
//         public string UIName;
//         /// <summary>
//         /// 资源定位地址。
//         /// </summary>
//         [LabelText("资源定位地址")] 
//         [ReadOnly]
//         public string Location;
//         
//         /// <summary>
//         /// UI 组名。
//         /// </summary>
//         [LabelText("UI 组名")]
//         [HideIf("UIType",UIConfig.UITypes.UIItem)]
//         [ValueDropdown("GetUIGroupNames")]
//         public string UIGroupName = "MainUI";
//
//         /// <summary>
//         /// 是否使用默认动画。
//         /// </summary>
//         [BoxGroup("动画")]
//         [LabelText("是否使用默认动画")]
//         [HideIf("UIType",UIConfig.UITypes.UIItem)]
//
//         public bool UseDefaultAnimation=true;
//
//         /// <summary>
//         /// 默认动画是否需要弹跳效果。
//         /// </summary>
//         [BoxGroup("动画")]
//         [LabelText("默认动画是否需要弹跳效果")]
//         [HideIf("UIType",UIConfig.UITypes.UIItem)]
//         [ShowIf("UseDefaultAnimation",true)]
//         public bool IsNeedBounce = false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否允许多个实例")]
//         public bool AllowMultiInstance = false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否预加载")]
//         public bool IsPreLoad = false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否预实例化")]
//         public bool IsPreInstance = false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否锁定(锁定的对象不会被回收)")]
//         public bool IsLock=false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否不需要遮罩")] 
//         public bool NotNeedMask = false;
//         
//         [BoxGroup("加载")]
//         [LabelText("是否自定义加载完成时机")]
//         public bool IsCustomLoadFinish = false;
//  
//         [BoxGroup("加载")]
//         [LabelText("打开本页面是否关闭主页")]
//         public bool CloseMainUI = false;
//         
//         #region 打开子界面
//   
//         List<string> GetAllUIFormNames()
//         {
// #if UNITY_EDITOR
//             //反射所有继承UIFormExtend的类
//             return UnityEditor.TypeCache.GetTypesDerivedFrom<UIFormLogicExtend>().Select(e=>e.Name).ToList();  
// #endif
//             return null;
//         }
//         #endregion
//         
//         // private void OnEnable()
//         // {
//         //     var uiConfig = AssetPathUtility.LoadAssetDataBase<UIConfig>(UIConfig.UIConfigPath);
//         //
//         //     if (uiConfig.OpenConfigs.ContainsKey(UIName))
//         //     {
//         //         var uiConfigOpenConfig = uiConfig.OpenConfigs[UIName];
//         //         UIGroupName = uiConfigOpenConfig.GroupName;
//         //     }
//         // }
//
//         [Button("初始化")]
//         public void Init()
//         {
//             
//             string basePath = UIType switch
//             {
//                 UIConfig.UITypes.UIForm => UIConfig.UIFormBasePath,
//                 UIConfig.UITypes.UIItem => UIConfig.UIComponentBasePath,
//                 _ => UIConfig.UIFormBasePath
//             };
//
//             switch (UIType)
//             {
//                 case UIConfig.UITypes.UIForm:
//                     break;
//                 case UIConfig.UITypes.UIItem:
//                     break;
//                 case UIConfig.UITypes.UIPopWindow:
//                     // UIGroupName = "PopWindow";
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//
//             Location=AssetPathUtility.GetAssetPathRelativeToBase(gameObject,basePath);
//
//             UIName = GetComponent<ComponentAutoBindTool>().className;
//             var uiConfig = AssetPathUtility.LoadAssetDataBase<UIConfig>(UIConfig.UIConfigPath);
//             uiConfig.Add2OpenConfig(this);
//         }
//
//         [Button("分离子Tab")]
//         public void DetachSubGroup()
//         {
//             var uiSetActiveBindTool = GetComponentInChildren<UISetActiveBindTool>();
//
//             if (uiSetActiveBindTool == null)
//             {
//                 return;
//             }
//             
//             var prefabPath = $"Assets/Game/UIForm/{Location}.prefab";
//             uiSetActiveBindTool.DetachSubGroup(prefabPath,transform);
//         }
//         /// <summary>
//         /// 获取 UI 组名列表
//         /// </summary>
//         private static IEnumerable<string> GetUIGroupNames()
//         {
//             var config = ScriptableObject.CreateInstance<UIConfig>();
//             foreach (var group in config.UIGroups)
//             {
//                 yield return group.Name;
//             }
//         }
//     }
// }
