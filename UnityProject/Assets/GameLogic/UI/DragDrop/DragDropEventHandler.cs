using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic
{
    /// <summary>
    /// 拖拽事件处理器 - 接收Unity拖拽事件，转换为通用事件
    /// 职责：只负责接收拖拽事件，不关心业务逻辑
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DragDropEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("拖拽设置")]
        [Tooltip("拖拽项数据")]
        public DragItemData dragItemData;
        
        [Tooltip("拖拽时的透明度")]
        [Range(0f, 1f)]
        public float dragAlpha = 0.6f;
        
        [Header("世界坐标转换")]
        [Tooltip("世界相机（用于将屏幕坐标转换为世界坐标）")]
        public Camera worldCamera;
        
        [Tooltip("Z轴深度")]
        public float worldZDepth = 0f;
        
        [Tooltip("世界坐标平面距离")]
        public float worldPlaneDistance = 10f;
        
        // 内部变量
        private RectTransform m_rectTransform;
        private CanvasGroup m_canvasGroup;
        private Canvas m_canvas;
        private Vector2 m_originalPosition;
        private bool m_isDragging = false;
        
        // 事件：只发出通用事件，不关心具体业务
        public event Action<DragItemData> OnBeginDragEvent;
        public event Action<DragItemData, Vector2> OnDragEvent;
        public event Action<DragItemData, Vector2> OnEndDragEvent;
        
        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            m_canvas = GetComponentInParent<Canvas>();
            if (m_canvas == null)
            {
                m_canvas = FindObjectOfType<Canvas>();
            }
            
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
                if (worldCamera == null)
                {
                    worldCamera = FindObjectOfType<Camera>();
                }
            }
            
            if (worldPlaneDistance <= 0 && worldCamera != null)
            {
                worldPlaneDistance = worldCamera.nearClipPlane + 10f;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (dragItemData == null) return;
            
            m_isDragging = true;
            m_originalPosition = m_rectTransform.anchoredPosition;
            m_canvasGroup.alpha = dragAlpha;
            m_canvasGroup.blocksRaycasts = false;
            transform.SetAsLastSibling();
            
            OnBeginDragEvent?.Invoke(dragItemData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!m_isDragging) return;
            
            // 更新UI位置
            if (m_canvas != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_canvas.transform as RectTransform,
                    eventData.position,
                    m_canvas.worldCamera,
                    out localPoint);
                m_rectTransform.position = m_canvas.transform.TransformPoint(localPoint);
            }
            else
            {
                m_rectTransform.anchoredPosition += eventData.delta;
            }
            
            // 获取世界坐标并发出事件
            Vector2 worldPos = GetWorldPosition(eventData.position);
            OnDragEvent?.Invoke(dragItemData, worldPos);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDragging) return;
            
            m_isDragging = false;
            m_canvasGroup.alpha = 1f;
            m_canvasGroup.blocksRaycasts = true;
            m_rectTransform.anchoredPosition = m_originalPosition;
            
            Vector2 worldPos = GetWorldPosition(eventData.position);
            OnEndDragEvent?.Invoke(dragItemData, worldPos);
        }
        
        private Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            if (worldCamera == null) return Vector2.zero;
            
            Vector3 worldPos;
            if (worldCamera.orthographic)
            {
                worldPos = worldCamera.ScreenToWorldPoint(
                    new Vector3(screenPosition.x, screenPosition.y, worldPlaneDistance));
            }
            else
            {
                Ray ray = worldCamera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
                if (Mathf.Abs(ray.direction.z) > 0.0001f)
                {
                    float distance = (worldZDepth - ray.origin.z) / ray.direction.z;
                    worldPos = ray.origin + ray.direction * distance;
                }
                else
                {
                    worldPos = worldCamera.ScreenToWorldPoint(
                        new Vector3(screenPosition.x, screenPosition.y, worldPlaneDistance));
                }
            }
            
            return new Vector2(worldPos.x, worldPos.y);
        }
        
        public void SetDragItemData(DragItemData data)
        {
            dragItemData = data;
        }
        
        public bool IsDragging => m_isDragging;
    }
}

