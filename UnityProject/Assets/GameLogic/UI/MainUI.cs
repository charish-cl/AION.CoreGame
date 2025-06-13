using System.IO;
using AION.CoreFramework;
using Luban;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    [Window(UILayer.UI)]
    public partial class MainUI : UIWindow
    {
        public override void RegisterEvent()
        {
        }
        public override void OnCreate()
        {
            // var tables = new cfg.Tables(file => return new ByteBuf(File.ReadAllBytes($"{gameConfDir}/{file}.bytes")));

            CreateWidgetByType<CurrencyItem>(transform);
        }

        private void OnClick_TestButton()
        {
            Log.Info("OnClick_TestButtotn");
            Close();
            
            
        }

        private void OnClick_LevelEndBtn()
        {
            
        }
    }
}