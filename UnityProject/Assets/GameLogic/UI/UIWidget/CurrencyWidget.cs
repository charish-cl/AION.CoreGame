using System.Collections.Generic;
using AION.CoreFramework;
using GameConfig.item;
using UI;

namespace GameLogic
{
    public partial class CurrencyWidget
    {
        List<CurrencyItem> currencyItems = new List<CurrencyItem>();
        
        public void InitCurrency(params CurrencyType[]  currencyTypes)
        {
            if (currencyTypes == null)
            {
                Log.Error("CurrencyType is null");
                return;
            }
            
            AdjustIconNum(currencyItems,currencyTypes.Length,m_tfParent,currencyItem);
            
            
            for (int i = 0; i < currencyItems.Count; i++)
            {
                CurrencyItem currencyItem = currencyItems[i];
             
                currencyItem.Init(currencyTypes[i]);
            }
        }
    }
}