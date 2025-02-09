using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameDevKit
{
    public class UGUITest:GridLayoutGroup ,IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        
        public float sensitivity = 1f; // 拖动灵敏度
        private Vector2 startPosition; // 拖动开始位置
        private Vector2 contentStartPosition; // 内容开始位置
        private RectTransform contentRectTransform; // 内容的 RectTransform
        

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = eventData.position;
            contentStartPosition = contentRectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float dragDelta = (eventData.position.y - startPosition.y) * sensitivity;
            Vector2 newPosition = contentStartPosition + new Vector2(0f, dragDelta);
            contentRectTransform.anchoredPosition = newPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 拖动结束时，可以添加一些额外的逻辑，比如检查内容是否超出边界，然后做出相应的调整
        }
    }
}