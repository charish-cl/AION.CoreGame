using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 货币数据基类，使用策略模式实现不同类型的货币
    /// </summary>
    public abstract class BaseCurrencyData
    {
        /// <summary>
        /// 货币类型
        /// </summary>
        public CurrencyType CurrencyType { get; protected set; }
        
        /// <summary>
        /// 当前数量
        /// </summary>
        public long CurrentAmount { get; protected set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected BaseCurrencyData(CurrencyType currencyType)
        {
            CurrencyType = currencyType;
            CurrentAmount = 0;
        }

        /// <summary>
        /// 设置数量
        /// </summary>
        public virtual void SetAmount(long amount)
        {
            CurrentAmount = amount;
        }

        /// <summary>
        /// 增加数量
        /// </summary>
        public virtual void AddAmount(long amount)
        {
            CurrentAmount += amount;
            if (CurrentAmount < 0)
            {
                CurrentAmount = 0;
            }
        }

        /// <summary>
        /// 获取数量字符串（用于显示），子类可以重写以自定义显示格式
        /// </summary>
        public virtual string GetNumStr()
        {
            return CurrentAmount.ToString();
        }

        /// <summary>
        /// 点击货币时的处理，子类可以重写以自定义点击行为
        /// </summary>
        public virtual void DoClick()
        {
            // 默认不做任何处理
        }
    }
}

