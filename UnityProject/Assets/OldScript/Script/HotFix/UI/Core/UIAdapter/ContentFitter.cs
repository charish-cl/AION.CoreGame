using UnityEngine;
using Sirenix.OdinInspector;

namespace Feif.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ContentFitter : MonoBehaviour
    {
        [Header("顶部物体")]
        public RectTransform topElement;

        [Header("底部物体")]
        public RectTransform bottomElement;

        [Header("中间自适应物体")]
        public RectTransform middleElement;

        [Header("是否每帧计算")]
        public bool calculateEveryFrame = true;

        [Header("最大高度")]
        public float maxHeight = 2000f;

        private RectTransform rectTransform;

        void OnEnable()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            AdjustLayout();
        }

        void Update()
        {
            if (calculateEveryFrame)
            {
                AdjustLayout();
            }
        }

        [Button("调整布局")]
        public void AdjustLayout()
        {
            if (topElement == null || bottomElement == null || middleElement == null)
            {
                // Debug.LogError("请确保所有组件已正确赋值！");
                return;
            }

            // 获取父容器的高度
            float totalHeight = rectTransform.parent.GetComponent<RectTransform>().rect.height;

            // 获取顶部和底部物体的高度
            float topHeight = topElement.rect.height;
            float bottomHeight = bottomElement.rect.height;

            // 计算中间物体的最大可用高度
            float availableHeight = totalHeight - topHeight - bottomHeight;
            
            middleElement.pivot = new Vector2(0.5f, 1f);
            
            //左右拉伸
            middleElement.anchorMin = new Vector2(0f, 1f);
            middleElement.anchorMax = new Vector2(1f, 1f);
        
            // 更新中间物体的高度
            middleElement.sizeDelta = new Vector2(middleElement.sizeDelta.x, availableHeight);

            middleElement.SetAnchorPositionY(-topHeight);
        }
    }
}