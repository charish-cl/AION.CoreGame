using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 体力货币
    /// </summary>
    [Currency(CurrencyType.Energy)]
    public class EnergyCurrency : BaseCurrencyData
    {
        public EnergyCurrency(CurrencyType currencyType) : base(currencyType)
        {
        }

        /// <summary>
        /// 重写获取数量字符串
        /// </summary>
        public override string GetNumStr()
        {
            // 体力通常显示为 "当前/最大" 格式
            // 这里简化处理，只显示当前值
            return CurrentAmount.ToString();
        }

        /// <summary>
        /// 重写点击处理，例如打开体力购买界面
        /// </summary>
        public override void DoClick()
        {
            // TODO: 实现体力点击逻辑，例如打开购买界面
        }
    }
}

