// using Cysharp.Threading.Tasks;
// using GameFramework.Event;
// using GameFramework.Fsm;
// using GameFramework.Procedure;
// using UnityGameFramework.Runtime;
//
// namespace AION.CoreFramework
// {
//     public class ProcedureMain : ProcedureBase
//     {
//         private IFsm<IProcedureManager> ProcedureOwner;
//         private async UniTask InitMainScene()
//         {
//             await LoadState.LoadMainScene.Task;
//             GameEntry.ChessPool.MoveGameObjectToScene();
//             LoadState.InitMainScene.TrySetResult(true);
//         }
//         
//         protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
//         {
//             base.OnEnter(procedureOwner);
//             ProcedureOwner = procedureOwner;
//             await LoadState.LoadDataConfig.Task;
//             SplashLoadingForm.UpdateProgress(Constant.Loading.Progress.MainLoadUserResource,
//                 Constant.Loading.Localization.MainLoadUserResource);
//             await LoadState.LoadBaseUserInfo.Task;
//             await LoadState.LoadMapInfo.Task;
//             await LoadState.LoadActivityConfig.Task;
//             await LoadState.LoadAllUserInfo.Task;
//             
//             SplashLoadingForm.UpdateProgress(Constant.Loading.Progress.MainEnterGame, Constant.Loading.Localization.MainEnterGame);
//             
//             GlobalStopWatch.StartProcedure("打开主UI"); 
//             
//             InitMainScene();
//             await GameEntry.Guide.Init();
//             //Main页面需要玩家的数据  也需要主场景的资源 
//             await (GameEntry.UI.OpenUIFormAsync(AssetUtility.GetUIFormAsset("Main"), "MainUI"));
//             
//             GlobalStopWatch.EndProcedure("打开主UI");
//             
//             //加载当前地图
//             GlobalStopWatch.sw.Stop();
//             GlobalStopWatch.AddLog($"启动耗时：{GlobalStopWatch.sw.ElapsedMilliseconds}ms");
//
// #if WX && !UNITY_EDITOR
//          var userInfoId = GameEntry.PlayerLogic.UserInfo.Id;
//         GameEntry.WXComponent.SetStorageUid(userInfoId);    
// #endif
//             UniTask.Delay(10000).ContinueWith(()=>{
//                 var mapGroup = GameEntry.CustomResource.GetResource<MapGroup>();
//                 mapGroup.PreLoad((GameEntry.BuildSystem.cityId,GameEntry.BuildSystem.LocalUserBuildingStates,false));
//                 });
//             //初始化声音设置
//             SettingDetailForm.InitSoundSetting();
//             //播放背景音乐
//             GameEntry.BgSound.PlayMainBgSound();
//
//             GameEntry.Event.Subscribe(RestartGameEvent.EventId, OnRestartGameEvent);
//         
//             DebugUIForm.ShowDebugInfo();
//             LogCustom.LogLogin("ProcedureMain OnEnter finish");
//         }
//         
//         
//         private void OnRestartGameEvent(object sender, GameEventArgs e)
//         {
//             // ChangeState<ProcedureRestartGame>(ProcedureOwner);
//           
//             GameEntry.UI.Open<ConfirmWindowForm, ConfirmWindowForm.ConfirmWindowParam>(ConfirmWindowForm.GetGameStateError());
//            
//         }
//
//         private void CloseUIFormComplete(object sender, GameEventArgs e)
//         {
//             CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = (CloseUIFormCompleteEventArgs)e;
//             //Log.Info("关闭: " + closeUIFormCompleteEventArgs.UIFormAssetName);
//         }
//
//         protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
//         {
//             base.OnLeave(procedureOwner, isShutdown);
//
//
//           //  GameEntry.Event.Unsubscribe(CloseUIFormCompleteEventArgs.EventId, CloseUIFormComplete);
//             GameEntry.Event.Unsubscribe(RestartGameEvent.EventId, OnRestartGameEvent);
//         }
//     }
// }