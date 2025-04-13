using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AION.CoreFramework
{
    public class UITimer
    {
        /// <summary>
        /// 计时器的唯一标识，方便调试
        /// </summary>
        public string Key;

        /// <summary>
        /// 时间戳
        /// </summary>
        public long EndTime;

        /// <summary>
        /// 结束行为
        /// </summary>
        public Action<string> EndAction;

        /// <summary>
        /// 更新显示的委托
        /// </summary>
        private Action<string> UpdateAction;

        private Queue<CancellationTokenSource> cancellationTokenSources = new Queue<CancellationTokenSource>();

        // 构造函数
        public UITimer(string key, Action<string> updateAction, long endTime, Action<string> endAction)
        {
            Key = key;
            UpdateAction = updateAction;
            EndTime = endTime;
            EndAction = endAction;
        }

        public void ResetTimer(string key, Action<string> updateAction, long endTime, Action<string> endAction, string format)
        {
            // 取消并移除队列中的所有旧的CancellationTokenSource
            while (cancellationTokenSources.Count > 0)
            {
                var oldCancellationTokenSource = cancellationTokenSources.Dequeue();
                oldCancellationTokenSource.Cancel();
                oldCancellationTokenSource.Dispose();
            }

            Key = key;
            UpdateAction = updateAction;
            EndTime = endTime;
            EndAction = endAction;
            StartTimer(formatStr: format).Forget();
        }

        public async UniTaskVoid StartTimer(string formatStr = null, object anotherNeedFormat = null)
        {
            // 创建新的CancellationTokenSource，并将其加入队列
            var newCancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSources.Enqueue(newCancellationTokenSource);

            while (true)
            {
                if (newCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                long currentTime = GetServerTime();
                

                void fillTime()
                {
                    string time = ConvertTimestampToTime(EndTime);
                    if (!string.IsNullOrEmpty(formatStr))
                    {
                        time = anotherNeedFormat != null ? string.Format(formatStr, time, anotherNeedFormat) : string.Format(formatStr, time);
                    }
                    UpdateAction?.Invoke(time);
                }

                if (EndTime <= currentTime)
                {
                    fillTime();
                    EndTime = long.MaxValue;
                    // 调用结束行为，传入 key
                    EndAction?.Invoke(Key);
                    Log.Info($"----------------倒计时结束，Key: {Key}-----------------");
                    return;
                }
                else
                {
                    fillTime();
                }
                // TODO: 每隔1分钟刷新下，根据情况
                // 每隔1s刷新下
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: newCancellationTokenSource.Token);
            }
        }

        private long GetServerTime()
        {
          // return  TimeSyncComponent.ServerUtcNowS();
            return 0;
        }

        /// <summary>
        /// 将时间戳转换为时间字符串
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns>时间字符串</returns>
        private string ConvertTimestampToTime(long timestamp)
        {
            // return MathUtility.Timestamp2Time(timestamp);
            return "";
        }

        public void Clear()
        {
            Key = null;
            UpdateAction = null;
            EndAction = null;

            // 取消并移除队列中的所有旧的CancellationTokenSource
            while (cancellationTokenSources.Count > 0)
            {
                var oldCancellationTokenSource = cancellationTokenSources.Dequeue();
                oldCancellationTokenSource.Cancel();
                oldCancellationTokenSource.Dispose();
            }
        }
    }
}