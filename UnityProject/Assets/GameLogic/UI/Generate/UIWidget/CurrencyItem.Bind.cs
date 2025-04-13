
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class CurrencyItem 
    {
        
        public Button AddArea { get;  set; }
        public Image Icon { get;  set; }
        public TextMeshProUGUI Num { get;  set; }

        public override void ScriptGenerator()
        {
            
            AddArea = transform.Find("Key/Button_AddArea").GetComponent<Button>();
            AddArea.onClick.AddListener(() => OnClick_AddArea());
            Icon = transform.Find("Key/Icon").GetComponent<Image>();
            Num = transform.Find("Key/Num").GetComponent<TextMeshProUGUI>();

        }
    }
}