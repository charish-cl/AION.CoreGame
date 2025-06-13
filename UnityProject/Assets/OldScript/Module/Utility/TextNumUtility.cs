using System;

namespace AION.CoreFramework
{
    /// <summary>
    /// 文本显示数字工具类
    /// </summary>
    public static class TextNumUtility
    {

        private static string ConvertNum(ulong num, ulong standard, string unit)
        {
            decimal result = num / (decimal)standard;
            // Log.Info($"{num} {standard} {unit} {result}");
            if (num / (double)standard >= 100)
            {
                return $"{Math.Floor(result)}{unit}";
            }
            else if (num / (double)standard >= 10)
            {
                return $"{Math.Floor(result*10)/10:0.#}{unit}";
            }
            else if (num / (double)standard >= 1)
            {
                return $"{Math.Floor(result*100)/100:0.##}{unit}";
            }

            throw new Exception($"{num} {standard} {unit}");
        }

        /// <summary>
        /// 显示数字，根据是否在小区域内显示不同格式
        /// </summary>
        /// <param name="num"></param>
        /// <param name="IsInSmallArea"></param>
        /// <returns></returns>
        public static string ShowTextNum(ulong num, bool IsInSmallArea)
        {
            string text = "";
            //如果在小区域内
            if (IsInSmallArea)
            {
                //如果除不尽默认显示两位小数 F2会自动四舍五入，不太行
                //用Math.Floor 方法来向下取整。
                //首先，将 num2 / 10000m 的结果乘以100（这是因为我们想保留两位小数），
                //然后使用 Math.Floor 向下取整，最后再除以100来恢复原来的小数位。
                //这种方法确保了在转换为字符串之前，数字已经被适当地截断，从而避免了在格式化时的四舍五入
                if (num >= 1_000_000_000_000) // T - 万亿
                {
                    text = ConvertNum(num, 1_000_000_000_000, "T");
                }
                else if (num >= 1_000_000_000) // B - 十亿
                {
                    text = ConvertNum(num, 1_000_000_000, "B");
                }
                else if (num >= 1_000_000) // M - 百万
                {
                    text = ConvertNum(num, 1_000_000, "M");
                }
                else if (num >= 1_0000) // K - 千，超过10000才缩写
                {
                    text = ConvertNum(num, 1_000, "K");
                }
                else
                {
                    text = num.ToString();
                }
            }
            else
            {
                //每隔三个数字加一个逗号
                if (num >= 1000)
                {
                    text = num.ToString("N0");
                }
                else
                {
                    text = num.ToString();
                }
            }

            return text;
        }

       
    }
}