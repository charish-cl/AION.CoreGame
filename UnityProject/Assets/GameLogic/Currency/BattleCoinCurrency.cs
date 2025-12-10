using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 局内金币货币
    /// </summary>
    [Currency(CurrencyType.BattleCoin)]
    public class BattleCoinCurrency : BaseCurrencyData
    {
        public BattleCoinCurrency(CurrencyType currencyType) : base(currencyType)
        {
        }

        /// <summary>
        /// 重写获取数量字符串，可以自定义显示格式
        /// </summary>
        public override string GetNumStr()
        {
            if (CurrentAmount >= 1000000)
            {
                return $"{CurrentAmount / 1000000f:F1}M";
            }
            else if (CurrentAmount >= 1000)
            {
                return $"{CurrentAmount / 1000f:F1}K";
            }
            return CurrentAmount.ToString();
        }

        /// <summary>
        /// 重写点击处理
        /// </summary>
        public override void DoClick()
        {
            // TODO: 实现局内金币点击逻辑
        }
    }
}

