using System.Collections.Generic;
using UnityEngine;

namespace AION.CoreFramework
{
    /// <summary>
    /// 椭圆自动布局组件
    /// </summary>
    // [ExecuteInEditMode]
    public class EllipseLayoutGroup : MonoBehaviour
    {
        // 水平半径
        [SerializeField] public float horizontalRadius = 100f;

        // 垂直半径
        [SerializeField] public float verticalRadius = 50f;

        // 每个子物体之间的角度占比数组（例如，[0.2, 0.4, 0.4]）
        [SerializeField] public List<float> angleDeltas = new();

        // 子物体占据的总角度
        [SerializeField] private float totalAngle = 180f;

        // 开始的方向 0-Right 1-Up 2-Left 3-Down
        [SerializeField] private int startDirection = 0;

        // 是否自动刷新布局
        [SerializeField] private bool autoRefresh = true;

        // 是否将子物体y轴对准椭圆中心
        [SerializeField] private bool alignRotationToCenter = true;

        // 是否控制子物体的大小
        [SerializeField] private bool controlChildSize = true;

        // 子物体大小
        [SerializeField] private Vector2 childSize = Vector2.one * 100f;

        // 缓存子物体数量
        private int cacheChildCount;

        private void Start()
        {
            cacheChildCount = transform.childCount;
            RefreshLayout();
        }

        private void Update()
        {
            // 开启自动刷新
            if (autoRefresh)
            {
                // 刷新布局
                RefreshLayout();
                // 再次缓存子物体数量
                cacheChildCount = transform.childCount;
            }
        }

        /// <summary>
        /// 刷新布局
        /// </summary>
        public void RefreshLayout()
        {
            // 获取所有非隐藏状态的子物体
            List<RectTransform> children = new List<RectTransform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    children.Add(child as RectTransform);
                }
            }

            // 计算子物体的起始角度偏移
            float accumulatedAngle = startDirection * 90f - totalAngle / 2f;

            for (int i = 0; i < children.Count; i++)
            {
                // 获取当前子物体的角度占比（超出angleDeltas数组时，重复最后一个值）
                float anglePercent = i < angleDeltas.Count ? angleDeltas[i] : angleDeltas[angleDeltas.Count - 1];
                float currentAngle = totalAngle * anglePercent;

                // 计算子物体的位置
                RectTransform child = children[i];
                float angle = accumulatedAngle + currentAngle * 0.5f; // 中心点为角度偏移
                float x = horizontalRadius * Mathf.Cos(angle * Mathf.PI / 180f);
                float y = verticalRadius * Mathf.Sin(angle * Mathf.PI / 180f);
                child.anchoredPosition = new Vector2(x, y);

                // 控制子物体大小
                if (controlChildSize)
                {
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, childSize.x);
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, childSize.y);
                }

                // 如果启用对齐旋转，将子物体的y轴对准椭圆中心
                if (alignRotationToCenter)
                {
                    Vector2 directionToCenter = (Vector2.zero - child.anchoredPosition).normalized;
                    float rotationAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
                    child.rotation = Quaternion.Euler(0, 0, rotationAngle + 90f); // +90度让Y轴指向中心
                }
                else
                {
                    // 保持初始旋转
                    child.rotation = Quaternion.identity;
                }

                // 累加当前角度以准备下一个子物体
                accumulatedAngle += currentAngle;
            }
        }
    }
}