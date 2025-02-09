// using System;
// using Cysharp.Threading.Tasks;
// using GameFramework.Fsm;
// using GameFramework.Procedure;
// using LitJson;
// #if WX
// using WeChatWASM;
// #endif
//
// namespace AION.CoreFramework
// {
//     public partial class ProcedurePreload : ProcedureBase
//     {
//         protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
//         {
//             base.OnEnter(procedureOwner);
//             await UniTask.Yield();
//             SplashLoadingForm.UpdateProgress(Constant.Loading.Progress.ProcedurePreloadStart,
//                 Constant.Loading.Localization.ProcedurePreloadStart);
//             
//             JsonMapper.RegisterImporter<long, ulong>(Convert.ToUInt64);
//             
//             await LoadGFExtend();
//             UserLogin().ContinueWith(() =>
//             {
//                 GetBaseUserInfo();
//                 GetAllUserInfo();
//             });
//             LoadMainScene();
//             LoadCommonResource();
//             OpenDebugUI();
//             ChangeState<ProcedureMain>(procedureOwner);
//         }
//     }
// }