// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using GameFramework;
// using LitJson;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityGameFramework.Runtime;
// using UnityWebSocket;
// using WebSocketState = UnityWebSocket.WebSocketState;
//
// #nullable enable
// namespace AION.CoreFramework
// {
//     public class WebsocketRespData
//     {
//         public int NotifyType;
//     }
//
//     public class WebsocketResponse
//     {
//         public WebsocketAction Action { get; set; }
//         public int Status { get; set; }
//         public int ErrorCode { get; set; }
//
//         public string? ErrorMessage { get; set; }
//
//         // 这里 Data 服务端定义的是 object?类型，跟 ApiResponse 类似。客户端 Json 解析库不支持，所以改为 JsonData 类型。
//         // 当接到服务端通知时，Data 对应的是下面定义的 WebsocketNotify 类
//         // jsondata android 端会报错，找不到 JsonData
//         // public WebsocketRespData Data { get; set; } //public object? Data { get; set; }
//         public WebsocketNotify Data { get; set; }
//     }
//
//     public class WebSocketComponent : GameFrameworkComponent
//     {
//         private IWebSocket socket;
//
//         // private readonly string _wsNotifyUrl = "ws://localhost:5036/ws/notify";
//         public string address => "wss://" + GFBuiltin.BuiltinData.Host + "/ws/notify";
//
//         // 通过 HTTP 登录接口获取的 jwt token
//         public string _jwtToken = null;
//         
//         private bool isReconnecting = false;
//
//
//         public DateTime LastHeartBeatTime { get; set; } = DateTime.Now;
//
//
//         public float SendHeartBeatIntervalTime { get; set; } = 2;
//
//         /// <summary>
//         ///  最大未收到心跳包间隔时间，超过这个时间则认为连接断开
//         /// </summary>
//         public float MaxNotReceiveHeartBeatIntervalTime { get; set; } = 5;
//
//         [Button]
//         public  void Connect(string Token)
//         {
//             _jwtToken = Token;
//             socket = new UnityWebSocket.WebSocket(address);
//             socket.OnOpen += Socket_OnOpen;
//             socket.OnMessage += Socket_OnMessage;
//             socket.OnClose += Socket_OnClose;
//             socket.OnError += Socket_OnError;
//             socket.ConnectAsync();
//             AddLog(string.Format("Connecting..."));
//         }
//         
//         private void StartHeartbeat()
//         {
//             StartCoroutine(this.SendHeartbeat());
//         }
//
//         private IEnumerator SendHeartbeat()
//         {
//             while (true)
//             {
//                 var request = new WebsocketRequest { Action = WebsocketAction.Ping, Message = "ping" };
//                 var json = JsonMapper.ToJson(request);
//
//                 try
//                 {
//                     AddLog("Ping 发送心跳包");
//                     socket.SendAsync(json);
//                 }
//                 catch (Exception ex)
//                 {
//                     AddLog($"Ping 发送心跳包失败：{ex.Message}");
//                     Reconnect();
//                     yield break;
//                 }
//                 yield return new WaitForSeconds(SendHeartBeatIntervalTime);
//             }
//
//         }
//
//
//         public void Reconnect()
//         {
//             if (isReconnecting) return;
//             isReconnecting = true;
//             this.StopAllCoroutines();
//             socket.CloseAsync();
//
//             StartCoroutine(ReconnectCoroutine());
//         }
//
//         private IEnumerator ReconnectCoroutine()
//         {
//             while (true)
//             {
//                 AddLog("正在尝试重连...");
//
//                 Connect(_jwtToken);
//
//                 if (socket.ReadyState == WebSocketState.Open)
//                 {
//                     AddLog("重连成功");
//                     isReconnecting = false;
//                     yield break;
//                 }
//
//                 yield return new WaitForSeconds(SendHeartBeatIntervalTime);
//             }
//         }
//
//         private void Socket_OnOpen(object sender, OpenEventArgs e)
//         {
//             AddLog(string.Format("Connected: {0}", address));
//
//             // 发送 JWT token 到服务器
//             var request = new WebsocketRequest() { Message = _jwtToken, Action = WebsocketAction.Login };
//             var json = JsonMapper.ToJson(request);
//             socket.SendAsync(json);
//             AddLog("JWT token sent!");
//           
//             StartHeartbeat();
//             
//         }
//
//         private void Socket_OnMessage(object sender, MessageEventArgs e)
//         {
//             if (e.IsBinary)
//             {
//                 AddLog(string.Format("Receive Bytes ({1}): {0}", e.Data, e.RawData.Length));
//             }
//             else if (e.IsText)
//             {
//                 // 将字符串反序列化为 WebsocketResponse 对象
//                 var responseObject = JsonMapper.ToObject<WebsocketResponse>(e.Data);
//                 if (responseObject.Action == WebsocketAction.Ping)
//                 {
//                     AddLog(string.Format("Receive Ping : {0}", e.Data));
//                     UpdateHeartBeat();
//                 }
//                 if (responseObject.Action != WebsocketAction.Ping)
//                 {
//                     AddLog(string.Format("Receive: {0}", e.Data));
//                 }
//
//                 // 登录返回
//                 if (responseObject.Action == WebsocketAction.Login)
//                 {
//                     if (responseObject.Status == 1)
//                     {
//                         AddLog("登录成功");
//                     }
//                     else
//                     {
//                         AddLog($"登录失败, 错误消息: {responseObject.ErrorMessage}");
//                     }
//                 }
//                 // 通知消息
//                 else if (responseObject.Action == WebsocketAction.Notify)
//                 {
//                     var notify = responseObject.Data;
//                     var notifyType = notify.NotifyType;
//                     switch (notifyType)
//                     {
//                         case NotifyType.None:
//                             AddLog("收到空消息");
//                             break;
//                         case NotifyType.DailyQuest:
//                             AddLog("收到每日任务完成通知");
//                             break;
//                         case NotifyType.DiceRecharge:
//                             AddLog("收到骰子恢复通知");
//                             break;
//                         case NotifyType.GetCard:
//                             AddLog("收到获得卡片通知");
//                             break;
//                         case NotifyType.StartupClaim:
//                             AddLog("收到新手任务完成通知");
//                             break;
//                         case NotifyType.SocialRecord:
//                             AddLog("收到社交日志通知");
//                             break;
//                         case NotifyType.NewActivity:
//                             AddLog("收到新活动配置通知");
//                             break;
//                         default:
//                             break;
//                     }
//
//                     MessageQueue.Enqueue(notify);
//                     GameEntry.Event.Fire(this, WebSocketNotifyEvent.Create(notify));
//                 }
//             }
//         }
//
//         private void UpdateHeartBeat()
//         {
//             // 这里判断是否断开连接
//             if (LastHeartBeatTime.AddSeconds(MaxNotReceiveHeartBeatIntervalTime) < DateTime.Now)
//             {
//                 Reconnect();
//             }
//
//             LastHeartBeatTime = DateTime.Now;
//         }
//
//         public Queue<WebsocketNotify> MessageQueue = new();
//
//         private void Socket_OnClose(object sender, CloseEventArgs e)
//         {
//             AddLog(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
//             Reconnect();
//         }
//
//         private void Socket_OnError(object sender, ErrorEventArgs e)
//         {
//             AddLog(string.Format("Error: {0}", e.Message));
//             Reconnect();
//         }
//
//         private void OnApplicationQuit()
//         {
//             if (socket != null && socket.ReadyState != WebSocketState.Closed)
//             {
//                 socket.CloseAsync();
//                 Debug.Log("[WebSocket] 关闭网络连接------");
//             }
//         }
//
//         private void AddLog(string str)
//         {
//             if (GameEntry.BuiltinData.isDebugOrWhite())
//             {
//                 Log.Info($"[WebSocket] {str}");
//             }
//         }
//     }
// }