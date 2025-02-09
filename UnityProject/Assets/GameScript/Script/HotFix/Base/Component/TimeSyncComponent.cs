// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using GameFramework;
// using GameFramework.Event;
// using Newtonsoft.Json;
// using UnityEngine;
// using UnityGameFramework.Runtime;
// using Debug = UnityEngine.Debug;
// using Object = System.Object;
//
// namespace AION.CoreFramework
// {
//     public class TimeSyncComponent : GameFrameworkComponent
//     {
//         public Dictionary<int, long> WebRequestStartDic = new Dictionary<int, long>();
//
//         protected override void Awake()
//         {
//             base.Awake();
//
//             GameEntry.Event.Subscribe(WebRequestStartEventArgs.EventId, OnWebRequestStart);
//             GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
//         }
//
//
//         private void OnWebRequestStart(object sender, GameEventArgs e)
//         {
//             var webRequestStartEventArgs = e as WebRequestStartEventArgs;
//
//             WebRequestStartDic[webRequestStartEventArgs.SerialId] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//         }
//
//         private void OnWebRequestSuccess(object sender, GameEventArgs e)
//         {
//             WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
//
//             if (!WebRequestStartDic.ContainsKey(ne.SerialId))
//             {
//                 return;
//             }
//             
//             long respEnd =  DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//             // Debug.Log($"{ne.SerialId} {DateTime.UtcNow.Millisecond} {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
//             if (ne.UserData is AwaitDataWrap<WebResult> webRequestUserdata)
//             {
//               //  Stopwatch stopwatch = new Stopwatch();
//                // stopwatch.Start();
//                 var InfoString = Utility.Converter.GetString(ne.GetWebResponseBytes());
//                 //Litjson要指明类型，只能用JsonUtility了
//                 var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(InfoString);
//                 //以ms为单位
//                 CalculateOffset(WebRequestStartDic[ne.SerialId], respEnd, apiResponse.ServerTime);
//
//                 WebRequestStartDic.Remove(ne.SerialId);
//               //  stopwatch.Stop();
//               //  Debug.Log(stopwatch.ElapsedMilliseconds);
//             }
//         }
//
//         private static long lastDuration = 0;
//         private static long c2sOffsetTime = 0;
//
//         /// <summary>
//         /// 计算客户端与服务器的误差
//         /// </summary>
//         /// <param name="clientStartTime"></param>
//         /// <returns></returns>
//         void CalculateOffset(long clientStartTime, long clientEndTime, long serverStartTime)
//         {
//             var duration = clientEndTime - clientStartTime;
//             if (duration > 3000)
//             {
//                 // 过滤请求时长太大的情况
//                 return;
//             }
//             //收到消息时间
//             var timeOffset = serverStartTime - (clientEndTime + clientStartTime) / 2;
//             // Log.Info($"CalculateOffset {serverStartTime} - {clientStartTime} {clientEndTime} = {timeOffset}");
//             if (c2sOffsetTime == 0 || lastDuration > duration)
//             {
//                 c2sOffsetTime = timeOffset;
//                 lastDuration = duration;
//             }
//             // DebugMessage?.Invoke($"{c2sOffsetTime}ms");
//         }
//
//         public static long ServerUtcNowMS()
//         {
//             return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + c2sOffsetTime;
//         }
//
//         public static long ServerUtcNowS()
//         {
//             return ServerUtcNowMS() / 1000;
//         }
//         
//         public static DateTime ServerUtcNowDateTime()
//         {
//             return new DateTime(ServerUtcNowMS() * 10000l);
//         }
//         
//         public static DateTimeOffset ServerUtcNowDateTimeOffset()
//         {
//             return new DateTimeOffset(ServerUtcNowDateTime());
//         }
//
//         public static long MillisSecond()
//         {
//             long ticks = DateTime.Now.Ticks;
//             
//             long unixTimestampMillis = (long)((ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000);
//             return unixTimestampMillis;
//         }
//
//     }
// }