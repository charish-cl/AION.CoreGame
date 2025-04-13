using Sirenix.OdinInspector;
using UnityEngine;

namespace Feif.UI
{
    using UnityEngine;
    using TMPro;

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AdaptiveTextWidth : MonoBehaviour
    {
        public TextMeshProUGUI textMeshPro; // TextMeshProUGUI组件
        public RectTransform rectTransform; // 需要调整宽度的RectTransform
        public float padding = 10f; // 额外的宽度，用于给文本留一些边距
        [Header("是否每一帧都计算")]
        public bool CalculateEveryFrame = true;

        [LabelText("最大宽度")]
        public float MaxWidth = 20000;
        void OnEnable()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            }
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            AdjustWidth();
        }

        void Update()
        {
            if (CalculateEveryFrame)
            {
                // 根据需要选择是否在Update中持续调整宽度，通常只在文本变化时调整一次即可
                AdjustWidth();
            }
        }

        [Button]
        public void AdjustWidth()
        {
            if (textMeshPro == null || rectTransform == null)
            {
                Debug.LogError("TextMeshProUGUI or RectTransform is not assigned.");
                return;
            }

            // var textWidth = textMeshPro.text.Length * textMeshPro.fontSize;
            // 获取文本的预期宽度
            //这个如果不及时更新，会错判宽度的
          textMeshPro.ForceMeshUpdate(); // 强制更新文本的网格
          
          // 获取文本的预期宽度
          float textWidth = textMeshPro.preferredWidth;

          float ajustWidth = textMeshPro.preferredWidth;
          // 如果文本宽度超过最大宽度，缩小字体尺寸
          if (textWidth > MaxWidth)
          {
              // AdjustFontSizeToFitWidth(MaxWidth - padding);
              ajustWidth = MaxWidth;
          }
            // 调整RectTransform的宽度
            rectTransform.sizeDelta = new Vector2(ajustWidth, rectTransform.sizeDelta.y);
        }
        // private void AdjustFontSizeToFitWidth(float targetWidth)
        // {
        //     // 获取当前字体大小
        //     // 手动减小字体大小直到适应目标宽度
        //     while (textMeshPro.preferredWidth > targetWidth)
        //     {
        //         textMeshPro.fontSize -= 0.1f;
        //         textMeshPro.ForceMeshUpdate();
        //     }
        // }
    }

}