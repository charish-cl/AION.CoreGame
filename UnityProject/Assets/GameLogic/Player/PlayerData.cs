using System.Collections.Generic;
using GameBase;
using AION.CoreFramework;
using GameConfig.item;

namespace GameLogic.Player
{

    public class PlayerData:Singleton<PlayerData>
    {
        
        /// <summary>
        /// 货币数据字典
        /// </summary>
        private Dictionary<CurrencyType, BaseCurrencyData> _currencyDataDict = new Dictionary<CurrencyType, BaseCurrencyData>();


        public PlayerData()
        {
            // 初始化所有货币
            InitializeCurrencies();
        }

        /// <summary>
        /// 初始化所有货币
        /// </summary>
        private void InitializeCurrencies()
        {
            foreach (CurrencyType currencyType in System.Enum.GetValues(typeof(CurrencyType)))
            {
                if (currencyType == 0) continue; // 跳过 None 值
                
                var currencyData = CurrencyFactory.CreateCurrency(currencyType);
                if (currencyData != null)
                {
                    _currencyDataDict[currencyType] = currencyData;
                }
            }
        }

        
        /// <summary>
        /// 获取货币数量
        /// </summary>
        public long GetMoney(CurrencyType currencyType)
        {
            if (!_currencyDataDict.TryGetValue(currencyType, out var currencyData))
            {
                Log.Warning($"PlayerData: 未找到货币类型 {currencyType}");
                return 0;
            }
            
            return currencyData.CurrentAmount;
        }
        
        /// <summary>
        /// 更新货币数量
        /// </summary>
        public void UpdateMoney(CurrencyType currencyType, long amount)
        {
            if (!_currencyDataDict.TryGetValue(currencyType, out var currencyData))
            {
                Log.Warning($"PlayerData: 未找到货币类型 {currencyType}");
                return;
            }
            
            currencyData.SetAmount(amount);
            
            // 触发货币更新事件
            GameEvent.Get<ICommonUI>().CurrcyChanged(currencyType, currencyData.CurrentAmount);
        }
        
        
        /// <summary>
        /// 检查货币是否足够
        /// </summary>
        public bool CheckMoneyEnough(CurrencyType currencyType, long amount)
        {
            return GetMoney(currencyType) >= amount;
        }
        
        /// <summary>
        /// 获取货币数据
        /// </summary>
        public BaseCurrencyData GetCurrencyData(CurrencyType currencyType)
        {
            _currencyDataDict.TryGetValue(currencyType, out var currencyData);
            return currencyData;
        }
    }
}