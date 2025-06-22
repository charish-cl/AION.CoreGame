
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class CommonGameMainBtnItem 
    {
        
        public Image ItemIcon { get;  set; }
        public Transform RedNode { get;  set; }
        public TextMeshProUGUI Num { get;  set; }
        public Button ItemBtn { get;  set; }

        public override void ScriptGenerator()
        {
            
            ItemIcon = transform.Find("ItemIcon").GetComponent<Image>();
            RedNode = transform.Find("RedNode").GetComponent<Transform>();
            Num = transform.Find("Num").GetComponent<TextMeshProUGUI>();
            ItemBtn = transform.Find("ItemBtn").GetComponent<Button>();
            ItemBtn.onClick.AddListener(() => OnClick_ItemBtn());

        }
    }
}