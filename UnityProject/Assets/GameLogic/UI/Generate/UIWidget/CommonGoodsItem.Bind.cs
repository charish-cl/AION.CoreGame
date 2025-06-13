
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class CommonGoodsItem 
    {
        
        public Image Bg { get;  set; }
        public Image ItemIcon { get;  set; }
        public Image Border { get;  set; }
        public Transform RedNode { get;  set; }
        public TextMeshProUGUI Num { get;  set; }
        public Transform OnSelect { get;  set; }
        public Button ItemBtn { get;  set; }

        public override void ScriptGenerator()
        {
            
            Bg = transform.Find("Bg").GetComponent<Image>();
            ItemIcon = transform.Find("ItemIcon").GetComponent<Image>();
            Border = transform.Find("Border").GetComponent<Image>();
            RedNode = transform.Find("RedNode").GetComponent<Transform>();
            Num = transform.Find("Num").GetComponent<TextMeshProUGUI>();
            OnSelect = transform.Find("OnSelect").GetComponent<Transform>();
            ItemBtn = transform.Find("ItemBtn").GetComponent<Button>();
            ItemBtn.onClick.AddListener(() => OnClick_ItemBtn());

        }
    }
}