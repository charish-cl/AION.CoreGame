using GameConfig.item;

namespace GameLogic
{
    public partial class BattleMainUI
    {
        public override void OnCreate()
        {
            base.OnCreate();

            var currencyWidget = CreateWidgetByType<CurrencyWidget>(transform);
            
            currencyWidget.InitCurrency(CurrencyType.Coin);
        }

        void RefreshUI()
        {
            
        
        }
        private void OnClick_button_Pause()
        {
        }

        private void OnClick_button_GameSpeed()
        {
        }

        private void OnClick_button_Refresh()
        {
        }

        private void OnClick_button_StartFight()
        {
        }
    }
}