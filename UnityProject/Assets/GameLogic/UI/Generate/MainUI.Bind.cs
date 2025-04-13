
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class MainUI 
    {
        
        public TextMeshProUGUI TestTxt { get;  set; }
        public Button TestButton { get;  set; }

    
        public override void ScriptGenerator()
        {
            
            TestTxt = transform.Find("TestButton/TestTxt").GetComponent<TextMeshProUGUI>();
            TestButton = transform.Find("TestButton").GetComponent<Button>();
            TestButton.onClick.AddListener(() => OnClick_TestButton());

        }
    }
}