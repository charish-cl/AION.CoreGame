
using AION.CoreFramework;
using GameConfig.item;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class CurrencyItem : UIWidget
    {
        
        private void OnClick_AddArea()
        {
        
        }


        public void Init(CurrencyType currencyType)
        {
            TbCurrencyConfig tbCurrencyConfig = ConfigSystem.Instance.Tables.TbCurrencyConfig;

            CurrencyConfig currencyConfig = tbCurrencyConfig.Get((int)currencyType);
            
            
            Icon.SetSprite(currencyConfig.Icon);
            
        }
    }
}