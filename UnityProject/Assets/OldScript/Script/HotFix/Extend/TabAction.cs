using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework
{
    [Serializable]
       public abstract class BaseAction
       {
           [Button("Execute", ButtonSizes.Large)]
           public abstract void Execute();
       }
       
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
    public class SetItemActive:BaseAction
    {
        public RectTransform target;
        
        public bool active;
        public override void Execute()
        {
            target.gameObject.SetActive(active);
        }
    }
    
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