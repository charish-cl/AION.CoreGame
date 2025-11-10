using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameLogic
{
    public partial class BattleMainUI : UIWindow
    {
        public Button button_Pause { get; set; }
        public Button button_GameSpeed { get; set; }
        public Button button_Refresh { get; set; }
        public Button button_StartFight { get; set; }
        public TextMeshProUGUI waveProcess { get; set; }
        public TextMeshProUGUI levelText { get; set; }
        public TextMeshProUGUI hpText { get; set; }

        public override void ScriptGenerator()
        {
            button_Pause = transform.Find("BtnParent/Button_Pause").GetComponent<Button>();
            button_Pause.onClick.AddListener(() => OnClick_button_Pause());
            
            button_GameSpeed = transform.Find("BtnParent/Button_GameSpeed").GetComponent<Button>();
            button_GameSpeed.onClick.AddListener(() => OnClick_button_GameSpeed());
            
            button_Refresh = transform.Find("BtnParent/Button_Refresh").GetComponent<Button>();
            button_Refresh.onClick.AddListener(() => OnClick_button_Refresh());
            
            button_StartFight = transform.Find("BtnParent/Button_StartFight").GetComponent<Button>();
            button_StartFight.onClick.AddListener(() => OnClick_button_StartFight());
            

            waveProcess = transform.Find("TopInfo/Wave/WaveProcess").GetComponent<TextMeshProUGUI>();


            levelText = transform.Find("TopInfo/Level/LevelText").GetComponent<TextMeshProUGUI>();


            hpText = transform.Find("").GetComponent<TextMeshProUGUI>();

        }
    }
}