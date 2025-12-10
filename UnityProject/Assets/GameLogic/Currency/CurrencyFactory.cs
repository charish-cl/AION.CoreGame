using System.Collections.Generic;
using GameConfig.item;

namespace GameLogic
{
    /// <summary>
    /// 货币工厂类，使用通用反射工厂管理所有货币类
    /// </summary>
    public static class CurrencyFactory
    {
        /// <summary>
        /// 创建货币数据实例
        /// </summary>
        /// <param name="currencyType">货币类型</param>
        /// <returns>创建的货币数据实例，如果找不到对应的类型则返回null</returns>
        public static BaseCurrencyData CreateCurrency(CurrencyType currencyType)
        {
            return ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>.Create(currencyType, currencyType);
        }

        /// <summary>
        /// 获取所有已注册的货币类型
        /// </summary>
        public static IEnumerable<CurrencyType> GetAllCurrencyTypes()
        {
            return ReflectionFactory<BaseCurrencyData, CurrencyType, CurrencyAttribute>.GetAllKeys();
        }
    }
}

