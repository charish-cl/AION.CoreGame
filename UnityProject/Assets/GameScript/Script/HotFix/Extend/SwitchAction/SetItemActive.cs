using UnityEngine;

namespace AION.CoreFramework.SwitchAction
{
    public class SetItemActive:BaseAction
    {
        public RectTransform target;
        
        public bool active;
        public override void Execute()
        {
            target.gameObject.SetActive(active);
        }
    }
}