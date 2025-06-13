// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using Cysharp.Threading.Tasks;
// using GameFramework;
// using GameFramework.Fsm;
// using GameFramework.Localization;
// using GameFramework.Procedure;
// using GameFramework.Resource;
// using LitJson;
// using TMPro;
// using UnityEngine;
// using UnityEngine.TextCore.LowLevel;
// using UnityGameFramework.Runtime;
// using Debug = UnityEngine.Debug;
//
// #if WX
// using WeChatWASM;
// #endif
//
// using Object = UnityEngine.Object;
//
// namespace AION.CoreFramework
// {
//     public partial class ProcedurePreload
//     {
//         
//         private async UniTask UserLogin()
//         {
//              //初始化微信sdk
// #if WX && !UNITY_EDITOR
//             Log.Info("初始化微信sdk");
//             await GameEntry.WXComponent.InitSdk();
// #endif
//             GlobalStopWatch.AddLog("开始请求网络数据");
//             // GFBuiltin.Setting.RemoveAllSettings();
//             // 这里检查登陆态并验证登陆态.失败的话需要重新登陆
//             if (!string.IsNullOrEmpty(GameEntry.Request.LoadToken()))
//             {
//                await GlobalStopWatch.MethodCall(" 获取用户信息", // get user info 
//                     GameEntry.Request.RequestUserInfoFunc().TryRequest(
//                         ensureSuccess: true,
//                         gameStateErrorAction: (code) =>
//                         {
//                             // clear token when server error
//                             if (code == 401 || (GFBuiltin.BuiltinData.isDebugOrWhite() && code == 500))
//                             {
//                                 Log.Debug("RequestUserInfoFunc 401");
//                                 GameEntry.Request.SaveToken("");
//                                 return true;
//                             }
//
//                             return false;
//                         },
//                         normalErrorAction: (msg) =>
//                         {
//                             Log.Debug("其他异常：" + msg);
//                             GameEntry.Request.SaveToken("");
//                             return true;
//                         },
//                canUsingLoadingUI: canUsingLoadingUI));
//             }
//            
//             // 主要为了用于 上面 401后继续登录。
//             if (string.IsNullOrEmpty(GameEntry.Request.LoadToken()))
//             {
//                 await GlobalStopWatch.MethodCall("登录", EnsureLogin());
//             }
//             GlobalStopWatch.AddLog("登录完成");
//             LoadState.UserLogin.TrySetResult(true);
//         }
//
//         public async UniTask GetBaseUserInfo()
//         {
//             GlobalStopWatch.StartProcedure("GetBaseUserInfo");
//             Func<UniTask<bool>> loadSpriteTask = async () =>
//             {
//                 var result = await GameEntry.PlayerLogic.GetBaseUserInfo();
//                 return result;
//             };
//             await loadSpriteTask.TryRequest(canShowToast: true, canUsingLoadingUI: false, ensureSuccess: true);
//             
//             GlobalStopWatch.EndProcedure("GetBaseUserInfo");
//         }
//
//         public void GetAllUserInfo()
//         {
//             //连接WebSocket
//             ConnectWebSocket();
//             //活动数据，内部已经做了处理,失败会弹窗
//             GameEntry.ActivityCfg.InitActivityCfg();
//             // todo 这个好像不是必须的
//             GameEntry.BuildSystem.InitMineMap();
//             //加载主页的消息
//             GameEntry.PlayerLogic.GetAllGameInfo();
//         }
//         
//         bool canUsingLoadingUI = false;
//         private async UniTask EnsureLogin()
//         {
//             LogCustom.LogLogin("ProcedureMain OnEnter EnsureLogin start");
//             await GameEntry.Request.WXLoginFunc().TryRequest(
//                 gameStateErrorAction: (code) =>
//                 {
//                     // 登录失败就清空微信，让用户使用游客登录
//                     GFBuiltin.WXComponent.ClearWXData();
//                     return false;
//                 },
//                 canUsingLoadingUI: canUsingLoadingUI,
//                 ensureSuccess:true);
//             LogCustom.LogLogin("ProcedureMain OnEnter EnsureLogin finish");
//         }
//       
//         private void ConnectWebSocket()
//         {
//             GameEntry.WebSocket.Connect(GameEntry.Request.Token);
//         }
//     }
// }