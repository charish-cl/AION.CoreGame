using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameLogic
{
    public partial class GameMainUI : UIWindow
    {
        public Button boardBg { get; set; }
        public Image image { get; set; }

        public override void ScriptGenerator()
        {
            boardBg = transform.Find("BoardBg").GetComponent<Button>();
            boardBg.onClick.AddListener(() => OnClick_boardBg());
            

            image = transform.Find("PlayerArear/UnitPlaceHolder/Warrior/Image").GetComponent<Image>();

        }
    }
}