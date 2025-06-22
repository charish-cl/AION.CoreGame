using AION.CoreFramework;
using GameBase;
using UI;

namespace GameLogic
{
    public class UIManager:Singleton<UIManager>
    {
        public void ShowConfirmDialog(string content, System.Action<bool> confirmCallbackc)
        {
            GameModule.UI.ShowWindow<CommonSelectTipUI>();
        }
    }
}