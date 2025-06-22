
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GameLogic
{
    public partial class CommonTipUI 
    {
        
        public TextMeshProUGUI Title { get;  set; }
        public TextMeshProUGUI Info { get;  set; }

    
        public override void ScriptGenerator()
        {
            
            Title = transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();
            Info = transform.Find("Text_Info").GetComponent<TextMeshProUGUI>();

        }
    }
}