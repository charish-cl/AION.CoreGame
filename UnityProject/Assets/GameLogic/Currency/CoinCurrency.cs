using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 金币货币
    /// </summary>
    [Currency(CurrencyType.Coin)]
    public class CoinCurrency : BaseCurrencyData
    {
        public CoinCurrency(CurrencyType currencyType) : base(currencyType)
        {
        }

        /// <summary>
        /// 重写获取数量字符串，可以自定义显示格式（例如：1000 -> 1K）
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
        /// 重写点击处理，例如打开金币商店
        /// </summary>
        public override void DoClick()
        {
            // TODO: 实现金币点击逻辑，例如打开商店
        }
    }
}

