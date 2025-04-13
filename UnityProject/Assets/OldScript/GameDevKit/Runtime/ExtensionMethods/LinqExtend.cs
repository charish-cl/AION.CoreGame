using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameDevKit.GameLogic
{
    public static class LinqExtend
    {
        public static Vector2 Average(this IEnumerable<Vector2> source)
        {
            var sum = Vector2.zero;
            long count = 0;
            checked
            {
                foreach (Vector2 v in source)
                {
                    sum += v;
                    count++;
                }
            }

            if (count > 0) return sum / count;
            return Vector2.zero;
        }

        /// <summary>
        /// 从0开始,倒数第0个,第1个
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource LastItem<TSource>(this IEnumerable<TSource> source, int index)
        {
            var length = source.Count();
            return source.ElementAt(length - 1 - index);
        }

        public static TSource Closest<TSource>(this IEnumerable<TSource> others,
            Func<TSource, float> selector)
        {
            TSource target = default(TSource);
            float closet = float.MaxValue;
            foreach (var source in others)
            {
                var t = selector(source);
                if (t < closet)
                {
                    closet = t;
                    target = source;
                }
            }

            return target;
        }

        /// <summary>
        /// 用隔开的符号组成一个字符串
        /// </summary>
        /// <param name="others"></param>
        /// <param name="splitChar">隔开的符号</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static string Split<TSource>(this IEnumerable<TSource> others, string splitChar)
        {
            StringBuilder builder = new StringBuilder();
            var cnt = others.Count();
            int i = 0;
            foreach (var tSource in others)
            {
                builder.Append(tSource.ToString());
                if (i != cnt - 1)
                {
                    builder.Append(splitChar);
                }

                i++;
            }

            return builder.ToString();
        }

        #region 调试相关

        /// <summary>
        /// 集合转为多行数据
        /// </summary>
        /// <param name="others"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static string ToMultiline<TSource>(this IEnumerable<TSource> others, Func<TSource, string> func)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var tSource in others)
            {
                builder.AppendLine(func(tSource));
            }

            return builder.ToString();
        }

        public static void Dump<TSource>(this IEnumerable<TSource> others)
        {
            foreach (var source in others)
            {
                Debug.Log(source);
            }
        }

        public static void Dump(this string others)
        {
            Debug.Log(others);
        }

        public static void Dump<TSource>(this IEnumerable<TSource> others, Func<TSource, List<object>> func)
        {
            foreach (var source in others)
            {
                Debug.Log(func(source).Split("---"));
            }
        }

        public static void Dump(this object source)
        {
            Debug.Log(source);
        }

        #endregion
  
    }
}