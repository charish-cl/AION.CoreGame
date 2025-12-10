
using AION.CoreFramework;
using GameConfig.item;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameLogic;
using GameLogic.Player;

namespace UI
{
    public partial class CurrencyItem : UIWidget
    {
        private CurrencyType m_currencyType;
        private BaseCurrencyData m_baseCurrencyData;
        public override void RegisterEvent()
        {
            base.RegisterEvent();
            
            AddUIEvent<CurrencyType,long>(ICommonUI_Event.CurrcyChanged, RefreshNum);
        }

      
        private void OnClick_AddArea()
        {
            // 调用货币的点击处理
            var currencyData = PlayerData.Instance.GetCurrencyData(m_currencyType);
            currencyData?.DoClick();
        }


        public void Init(CurrencyType currencyType)
        {
            m_currencyType = currencyType;
            
            TbCurrency tbCurrencyConfig = ConfigSystem.Instance.Tables.TbCurrency;

            CurrencyConfig currencyConfig = tbCurrencyConfig.Get((int)currencyType);
            
            m_baseCurrencyData= PlayerData.Instance.GetCurrencyData(m_currencyType);
            
            Icon.SetSprite(currencyConfig.Icon);
            
            RefreshNum();
        }
        
        /// <summary>
        /// 更新数量显示
        /// </summary>
        public void UpdateAmount(string amountStr)
        {
            if (Num != null)
            {
                Num.text = amountStr;
            }
        }

        private void RefreshNum()
        {
            if (m_baseCurrencyData != null && Num != null)
            {
                Num.text = m_baseCurrencyData.GetNumStr();
            }
        }
        private void RefreshNum(CurrencyType arg1, long arg2)
        {
            if (m_currencyType!=arg1)
            {
                return;
            }
            RefreshNum();
        }
    }
}