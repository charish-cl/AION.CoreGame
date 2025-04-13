
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public partial class MainUI 
    {
        
        public Button LevelEndBtn { get;  set; }

    
        public override void ScriptGenerator()
        {
            
            LevelEndBtn = transform.Find("Mid/LevelEndBtn").GetComponent<Button>();
            LevelEndBtn.onClick.AddListener(() => OnClick_LevelEndBtn());

        }
    }
}