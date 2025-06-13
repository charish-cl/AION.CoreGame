
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class RedNoteItem 
    {
        
        public Button Count { get;  set; }

        public override void ScriptGenerator()
        {
            
            Count = transform.Find("Text_Count").GetComponent<Button>();
            Count.onClick.AddListener(() => OnClick_Count());

        }
    }
}