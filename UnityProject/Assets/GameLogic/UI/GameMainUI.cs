using System.IO;
using AION.CoreFramework;
using GameLogic;


namespace GameLogic
{
    [Window(UILayer.UI)]
    public partial class GameMainUI : UIWindow
    {
        public override void RegisterEvent()
        {
        }
        public override void OnCreate()
        {
            // var tables = new cfg.Tables(file => return new ByteBuf(File.ReadAllBytes($"{gameConfDir}/{file}.bytes")));

            // CreateWidgetByType<CurrencyItem>(transform);
            //
            AddUIEvent<string>(ICommonUI_Event.ShowTip, ShowTip);
        }

        private void ShowTip(string s)
        {
            Log.Info(s);
        }

        private void OnClick_TestButton()
        {
            Log.Info("OnClick_TestButtotn");
            Close();
            
            
        }

        private void OnClick_LevelEndBtn()
        {
            GameEvent.Get<ICommonUI>().ShowTip("Level End");
        }
    }
}