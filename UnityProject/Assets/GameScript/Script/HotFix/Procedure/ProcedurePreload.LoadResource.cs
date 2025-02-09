// using System;
// using Cysharp.Threading.Tasks;
// using GameFramework.Localization;
// using TMPro;
// using UnityEngine;
// using UnityEngine.TextCore.LowLevel;
// using UnityGameFramework.Runtime;
// using Debug = UnityEngine.Debug;
// using Object = UnityEngine.Object;
//
// #if WX
// using WeChatWASM;
// #endif
//
// namespace AION.CoreFramework
// {
//     public partial class ProcedurePreload
//     {
//
//         private async UniTask LoadMainScene()
//         {
//             GlobalStopWatch.StartProcedure("LoadSceneAsync main");
//             try
//             {
//                 await GameEntry.Scene.LoadSceneAsync(AssetUtility.GetSceneAsset("Main"));
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError("LoadMainScene " + e.Message);
//                 GFBuiltin.PlatformComponent.Platform.RestartGame();
//                 return;
//             }
//             LoadState.LoadMainScene.TrySetResult(true);
//             GlobalStopWatch.EndProcedure("LoadSceneAsync main");
//         }
//         
//         public async UniTask LoadGFExtend()
//         {
//             GlobalStopWatch.StartProcedure("加载GFExtend");
//             // 加载自定义组件。GFExtend
//             GameObject gameObject = await GameEntry.Resource.LoadAssetAsync<GameObject>(AssetUtility.GetGFExtendPath());
//             Object.Instantiate(gameObject);
//             await UniTask.Yield();
//             GlobalStopWatch.EndProcedure($"加载GFExtend");
//             LoadState.LoadGFExtend.TrySetResult(true);
// #if UNITY_EDITOR
//             GameEntry.CustomResource.LoadFullFont(0);
// #endif
//         }
//
//         public async void LoadCommonResource()
//         {
//             await LoadState.LoadGFExtend.Task;
//             GlobalStopWatch.MethodCall("加载配置表", GameEntry.Data.PreLoad().ContinueWith(() =>
//             {
//                 //加载本地化要放在加载配置表完成后
//                 GameEntry.PlayerLogic.InitBySetting();
//                 LoadLocalizationConfig();
//                 LoadState.LoadDataConfig.TrySetResult(true);
//             }));
//             GlobalStopWatch.MethodCall("加载所有主场景需要的EventPrefab",
//                 GameEntry.CustomResource.prefabManager.PreLoadAllPrefab()
//                     .ContinueWith((() => GameEntry.ChessPool.InitPool()))
//                     .ContinueWith(() =>
//                     {
//                         LoadState.LoadAllPrefab.TrySetResult(true);
//                     }));
//         }
//         
//         public async void OpenDebugUI()
//         {
//             await LoadState.LoadGFExtend.Task;
//             // debugui是用来显示toast的，不能注释
//             GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("DebugUI"), "DebugUI");
//         }
//
//         private void  LoadLocalizationConfig()
//         {
//             Language language = GameEntry.Localization.Language;
//             if (GameEntry.Setting.HasSetting(Constant.Setting.Language))
//             {
//                 try
//                 {
//                     string languageString = GameEntry.Setting.GetString(Constant.Setting.Language);
//                     language = (Language)Enum.Parse(typeof(Language), languageString);
//                 }
//                 catch
//                 {
//                 }
//             }
//
//             //TODO 不支持语言的处理
//             if (language != Language.English
//                 && language != Language.ChineseSimplified
//                 && language != Language.ChineseTraditional
//                 && language != Language.Korean)
//             {
//                 // 若是暂不支持的语言，则使用英语
//                 language = Language.English;
//
//                 GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
//                 GameEntry.Setting.Save();
//             }
//
//             GameEntry.Localization.Language = language;
//
//             GameEntry.Localization.ParseData("");
//
//             
//             Log.Info("初始化语言为{0}", language.ToString());
//         }
//         //快速排序
//
//
//         private async UniTask InitFont()
//         {
//             FontAssetCreationSettings fontAssetCreationSettings = TMP_Settings.defaultFontAsset.creationSettings;
// #if UNITY_EDITOR
//             TMP_Settings.fallbackFontAssets.Clear();
// #endif
//             // 如果默认的获取不到就读取 ab包中的 字体创建动态字体
//             Debug.Log("InitFont start");
//             Font font = await GameEntry.PlatformComponent.Platform.GetFont();
//             Debug.Log("fallback 字体 " + font);
//             if (font != null)
//             {
//                 try
//                 {
//                     TMP_FontAsset fontAsset =
//                         TMP_FontAsset.CreateFontAsset(font
//                             , fontAssetCreationSettings.pointSize, fontAssetCreationSettings.padding, 
//                             GlyphRenderMode.SDFAA, 1024, 1024);
//                     fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
//                     fontAsset.isMultiAtlasTexturesEnabled = true;
//                     TMP_Settings.fallbackFontAssets.Add(fontAsset);
//                 }
//                 catch (Exception e)
//                 {
//                     Console.WriteLine(e);
//                 }
//             }
//         }
//   
//     }
// }