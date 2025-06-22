
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class CommonSelectTipUI 
    {
        
        public TextMeshProUGUI TextInfo { get;  set; }
        public Button ButtonCancle { get;  set; }
        public Button ButtonConfirm { get;  set; }
        public TextMeshProUGUI CancleText { get;  set; }
        public TextMeshProUGUI ConfirmText { get;  set; }

    
        public override void ScriptGenerator()
        {
            
            TextInfo = transform.Find("Popup/TextInfo").GetComponent<TextMeshProUGUI>();
            ButtonCancle = transform.Find("Popup/ButtonCancle").GetComponent<Button>();
            ButtonCancle.onClick.AddListener(() => OnClick_ButtonCancle());
            ButtonConfirm = transform.Find("Popup/ButtonConfirm").GetComponent<Button>();
            ButtonConfirm.onClick.AddListener(() => OnClick_ButtonConfirm());
            CancleText = transform.Find("Popup/ButtonCancle/CancleText").GetComponent<TextMeshProUGUI>();
            ConfirmText = transform.Find("Popup/ButtonConfirm/ConfirmText").GetComponent<TextMeshProUGUI>();

        }
    }
}