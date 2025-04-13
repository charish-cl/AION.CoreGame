using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework.SwitchAction
{
    public class SetItemSize : BaseAction
    {
        public RectTransform rectTransform;
        
        public Vector2 size;


        public override void Execute()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        [Button("获取当前大小")]
        public void GetSize()
        {
            size = rectTransform.sizeDelta;
            Debug.Log(size);
        }
    }
}