using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework.SwitchAction
{
    public class LogAction:BaseAction
    {
        public string message;
        public override void Execute()
        {
            Log.Info(message);
        }
    }

    public class MoveAnchoredPositionAction : BaseAction
    {
        public Vector2 offset;
        public RectTransform target;
        
        public override void Execute()
        {
            var rectTransform = (RectTransform)target;
            rectTransform.anchoredPosition += offset;
        }
    }

    public class SetAnchoredPositionAction : BaseAction
    {
        public Vector2 anchorPosition;
        public RectTransform target;
        public override void Execute()
        {
            var rectTransform = (RectTransform)target;
            rectTransform.anchoredPosition = anchorPosition;
        }

        [Button]
        public void Copy()
        {
            var rectTransform = (RectTransform)target;
            anchorPosition = rectTransform.anchoredPosition;
        }
    }
}