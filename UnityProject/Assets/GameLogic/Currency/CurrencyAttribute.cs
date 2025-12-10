using System;
using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 货币特性，用于标记货币类对应的CurrencyType
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CurrencyAttribute : Attribute
    {
        public CurrencyType CurrencyType { get; private set; }

        public CurrencyAttribute(CurrencyType currencyType)
        {
            CurrencyType = currencyType;
        }
    }
}

