using System;

namespace GameDevKit.Utility
{
    public static class FloatExtend
    {
        /// <summary>
        /// 判断浮点数是否为-1
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsInfinite(this float num)
        {
            return Math.Abs(num - (-1f)) < float.Epsilon;
        }
        
    }
}