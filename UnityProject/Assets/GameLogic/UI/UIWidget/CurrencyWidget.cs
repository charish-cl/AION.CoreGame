using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig.item;
using UI;
using GameLogic.Player;

namespace GameLogic
{
    public partial class CurrencyWidget
    {
        List<CurrencyItem> currencyItems = new List<CurrencyItem>();
        private Dictionary<CurrencyType, CurrencyItem> m_currencyItemDict = new Dictionary<CurrencyType, CurrencyItem>();
        
        public override void OnCreate()
        {
            base.OnCreate();
        }
        
        /// <summary>
        /// 初始化货币
        /// </summary>
        public void InitCurrency(params CurrencyType[]  currencyTypes)
        {
            if (currencyTypes == null)
            {
                Log.Error("CurrencyType is null");
                return;
            }
            
            AdjustIconNum(currencyItems,currencyTypes.Length,m_tfParent,currencyItem);
            
            m_currencyItemDict.Clear();
            
            for (int i = 0; i < currencyItems.Count; i++)
            {
                CurrencyItem currencyItem = currencyItems[i];
                CurrencyType currencyType = currencyTypes[i];
             
                currencyItem.Init(currencyType);
                m_currencyItemDict[currencyType] = currencyItem;
                
                // 初始化显示当前货币数量
                RefreshCurrencyDisplay(currencyType);
            }
        }
        
        /// <summary>
        /// 货币更新事件回调
        /// </summary>
        private void OnCurrencyUpdated(CurrencyType currencyType, long newAmount)
        {
            if (m_currencyItemDict.TryGetValue(currencyType, out var currencyItem))
            {
                RefreshCurrencyDisplay(currencyType);
            }
        }
        
        /// <summary>
        /// 刷新货币显示
        /// </summary>
        private void RefreshCurrencyDisplay(CurrencyType currencyType)
        {
            if (!m_currencyItemDict.TryGetValue(currencyType, out var currencyItem))
            {
                return;
            }
            
            var currencyData = PlayerData.Instance.GetCurrencyData(currencyType);
            if (currencyData != null)
            {
                currencyItem.UpdateAmount(currencyData.GetNumStr());
            }
        }
    }
}