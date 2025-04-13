using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AION.CoreFramework
{
    public class MathUtility
    {
        /// <summary>
        /// 从概率数组中随机一个索引
        /// </summary>
        /// <param name="chance"></param>
        /// <returns></returns>
        public static int ChanceList2RandomIndex(List<int> chance)
        {
            //获取总概率
            int totalChance = 0;
            for (int i = 0; i < chance.Count; i++)
            {
                totalChance += chance[i];
            }

            int random = Random.Range(0, totalChance);
            for (int i = 0; i < chance.Count; i++)
            {
                if (random - chance[i] < 0)
                {
                    return i;
                }
                else
                {
                    random -= chance[i];
                }
            }

            Log.Fatal("从概率数组中随机一个索引发生错误");
            return -1;
         }
        //
        // /// <summary>
        // /// 将秒数转时：分：秒格式
        // /// </summary>
        // /// <param name="second"></param>
        // /// <returns></returns>
        // public static string Second2HourMinuteSecond(float second)
        // {
        //     TimeSpan time = TimeSpan.FromSeconds(second);
        //
        //     return time.ToString(@"hh\:mm\:ss");
        // }
        //
        // /// <summary>
        // /// 将秒数转 {0}小时以前 等的格式
        // /// </summary>
        // /// <param name="timeSpan"></param>
        // /// <returns></returns>
        // public static string Second2DayOrHourOrMin(long timeSpan)
        // {
        //     var second = TimeSyncComponent.ServerUtcNowS() - timeSpan;
        //     TimeSpan time = TimeSpan.FromSeconds(second);
        //     if (time.Days > 0)
        //     {
        //         return string.Format(GameEntry.Data.localization.GetLocalization("SocialRecord.DayTime"), time.Days);
        //     }
        //
        //     if (time.Hours > 0)
        //     {
        //         return string.Format(GameEntry.Data.localization.GetLocalization("SocialRecord.HourTime"), time.Hours);
        //     }
        //
        //     if (time.Minutes > 0)
        //     {
        //         return string.Format(GameEntry.Data.localization.GetLocalization("SocialRecord.MinTime"), time.Minutes);
        //     }
        //
        //     return string.Format(GameEntry.Data.localization.GetLocalization("SocialRecord.WithinOneMin"));
        // }
        //
        // public static string Second2TimeStr(long duration,bool onlyOne=false)
        // {
        //     TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
        //     StringBuilder stringBuilder = new StringBuilder();
        //     if (duration <= 0)
        //     {
        //         // 防止返回负的时间
        //         return stringBuilder.ToString();
        //     }
        //
        //     if (timeSpan.Days > 0)
        //     {
        //         stringBuilder.Append(timeSpan.Days);
        //         stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Day"));
        //         if (timeSpan.Hours > 0&&!onlyOne)
        //         {
        //             stringBuilder.Append(timeSpan.Hours);
        //             stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Hour"));
        //         }
        //     }
        //     else if (timeSpan.Hours > 0)
        //     {
        //         stringBuilder.Append(timeSpan.Hours);
        //         stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Hour"));
        //         if (timeSpan.Minutes > 0&&!onlyOne)
        //         {
        //             stringBuilder.Append(timeSpan.Minutes);
        //             stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Min"));
        //         }
        //     }
        //     else
        //     {
        //         if (timeSpan.Minutes > 0)
        //         {
        //             stringBuilder.Append(timeSpan.Minutes);
        //             stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Min"));
        //             if (timeSpan.Seconds > 0&&!onlyOne)
        //             {
        //                 stringBuilder.Append(timeSpan.Seconds);
        //                 stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Second"));
        //             }
        //         }
        //         else
        //         {
        //             stringBuilder.Append(timeSpan.Seconds);
        //             stringBuilder.Append(GameEntry.Data.localization.GetLocalization("Milestone.Second"));
        //         }
        //     }
        //     return stringBuilder.ToString();
        // }
        //
        // public static string Timestamp2Time(long timestamp)
        // {
        //     return Second2TimeStr(timestamp - TimeSyncComponent.ServerUtcNowS());
        // }
        //
        // public static string Timestamp2TimeOnlyOne(long timestamp)
        // {
        //     return Second2TimeStr(timestamp - TimeSyncComponent.ServerUtcNowS(),true);
        // }
        //
        // public static string LastTime(long timestamp)
        // {
        //     return Second2TimeStr( TimeSyncComponent.ServerUtcNowS()-timestamp);
        // }
        //
        // /// <summary>
        // /// 是否超过一定时间
        // /// </summary>
        // /// <param name="timestamp"></param>
        // /// <param name="timeSpan"></param>
        // /// <returns></returns>
        // public static bool IsOverTime(long timestamp, TimeSpan timeSpan)
        // {
        //     return TimeSyncComponent.ServerUtcNowS() - timestamp > timeSpan.TotalSeconds;
        // }
        // public static bool CheckExpire(long timestamp)
        // {
        //     return TimeSyncComponent.ServerUtcNowS() > timestamp;
        // }
        //
        // public static TimeSpan GetIntervalTime(long timestamp )
        // {
        //     TimeSpan timeSpan = TimeSpan.FromSeconds(timestamp - TimeSyncComponent.ServerUtcNowS());
        //     return timeSpan;
        // }
        public static long DataTimeToTimestamp(DateTime dateTime)
        {
            // 转换为 DateTimeOffset
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);

            // 转换为 Unix 时间戳
            long unixTime = dateTimeOffset.ToUnixTimeSeconds();

            return unixTime;
        }

        //洗牌算法,有范围的洗牌，参数为数组，i，j
        public static void Shuffle<T>(T[] array, int begin = 0, int last = -1)
        {
            if (last == -1)
            {
                last = array.Length;
            }

            for (int i = begin; i < last; i++)
            {
                int r = Random.Range(i, last);
                (array[i], array[r]) = (array[r], array[i]);
            }
        }

        //洗牌算法,有范围的洗牌，参数为List
        public static void Shuffle<T>(List<T> list, int begin = 0, int last = -1)
        {
            if (last == -1)
            {
                last = list.Count;
            }

            if (list.Count<= 1)
            {
                return;
            }
            for (int i = begin; i < last; i++)
            {
                int r = Random.Range(i, last);
                (list[i], list[r]) = (list[r], list[i]);
            }
        }
        /// <summary>
        /// 获取随机的索引列表，相邻元素不能重复
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="minCnt">最小数量</param>
        /// <param name="maxCnt">最大数量</param>
        /// <returns>随机索引列表</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<int> GetRandomIndices<T>(List<T> list, int minCnt, int maxCnt)
        {
            int cnt = list.Count;
            if (cnt <=1 || maxCnt < minCnt || minCnt < 0)
            {
                throw new System.ArgumentException("参数无效：minCnt 或 maxCnt 不合理。");
            }

            // 确定结果列表的长度
            int resultCount = UnityEngine.Random.Range(minCnt, maxCnt + 1);
            List<int> result = new List<int>(resultCount);

            // 初始化第一个索引
            result.Add(UnityEngine.Random.Range(0, cnt));

            for (int i = 1; i < resultCount; i++)
            {
                int index;
                do
                {
                    index = UnityEngine.Random.Range(0, cnt);
                } while (index == result[i - 1]); // 确保与前一个元素不相同

                result.Add(index);
            }

            return result;
        } 
        /// <summary>
        /// 获取原始列表中随机的元素值，相邻元素不能重复
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="minCnt">最小数量</param>
        /// <param name="maxCnt">最大数量</param>
        /// <returns>随机元素值列表</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<T> GetRandomValues<T>(List<T> list, int minCnt, int maxCnt)
        {
            int cnt = list.Count;
            if (cnt <=1|| maxCnt < minCnt || minCnt < 0)
            {
                throw new System.ArgumentException("参数无效：minCnt 或 maxCnt 不合理。");
            }

            // 确定结果列表的长度
            int resultCount = UnityEngine.Random.Range(minCnt, maxCnt + 1);
            List<T> result = new List<T>(resultCount);

            // 初始化第一个元素值
            result.Add(list[UnityEngine.Random.Range(0, cnt)]);

            for (int i = 1; i < resultCount; i++)
            {
                T value;
                do
                {
                    value = list[UnityEngine.Random.Range(0, cnt)];
                } while (EqualityComparer<T>.Default.Equals(value, result[i - 1])); // 确保与前一个元素值不相同

                result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// 分配时间，使得每个时间点的总和等于 totalTime，且越来越长
        /// </summary>
        /// <param name="totalTime"></param>
        /// <param name="count"></param>
        /// <param name="ratio">初始增长因子，可以调整</param>
        /// <returns></returns>
       
        public static List<int> DistributeTime(int totalTime, int count, float ratio = 1.2f)
        {
            List<int> timePoints = new List<int>(count);
   
            // 初始值为1，构建一个几何序列
            float currentValue = 1f;
            float sum = 0f;

            // 首先计算几何序列的所有值，并计算它们的总和
            for (int i = 0; i < count; i++)
            {
                timePoints.Add((int)currentValue);
                sum += currentValue;
                currentValue *= ratio;
            }

            // 计算每个值相对于总时间的比例
            float scale = totalTime / sum;

            // 重新计算每个时间点，使它们的总和等于 totalTime
            for (int i = 0; i < timePoints.Count; i++)
            {
                timePoints[i] = Mathf.RoundToInt(timePoints[i] * scale);
            }

            // 由于四舍五入的误差，我们可能需要做一次微调来确保总和等于 totalTime
            int adjustedSum = timePoints.Sum();
            int difference = totalTime - adjustedSum;
            if (difference != 0)
            {
                // 调整最后一个值以确保总和精确
                timePoints[timePoints.Count - 1] += difference;
            }

            return timePoints;
        }

        
        //从List中随机取出一个元素
        public static T SelectRandomElement<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("The list cannot be null or empty.");
            }

            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// 计算一个数是2的多少次方
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetPowerOfTwo(int value)
        {
            int power = 0;
            while (value != 1)
            {
                value >>= 1;
                power++;
            }

            return power;
        }
        

        public static List<float> NormalizeWeight(List<float> weight)
        {
            float sum = weight.Sum();
            return weight.Select(x => x / sum).ToList();
        }
        
        public static int ClampLoopIndex(int value,int min,int max)
        {
            if (value < min)
            {
                value = max;
            }

            if (value > max)
            {
                value = min;
            }

            return value;
        }
    }
}