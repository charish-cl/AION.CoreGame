using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AION.CoreFramework;
using DG.Tweening;

namespace GameLogic
{
    /// <summary>
    /// 世界拖拽系统 - 通用的从UI拖拽到世界的系统
    /// 只负责拖拽逻辑，不关心具体业务，通过事件回调处理
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class WorldDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("拖拽设置")]
        [Tooltip("拖拽项的ID（用于回调时传递）")]
        public int dragItemId;
        
        [Tooltip("拖拽时显示的预览预制体（可选，如果不设置则使用当前GameObject作为预览）")]
        public GameObject previewPrefab;
        
        [Tooltip("拖拽时的透明度")]
        [Range(0f, 1f)]
        public float dragAlpha = 0.6f;
        
        [Header("世界坐标转换")]
        [Tooltip("世界相机（用于将屏幕坐标转换为世界坐标，如果不设置则自动查找Main Camera）")]
        public Camera worldCamera;
        
        [Tooltip("Z轴深度（用于世界坐标转换，通常为0）")]
        public float worldZDepth = 0f;
        
        [Tooltip("世界坐标平面距离（用于ScreenToWorldPoint，通常等于世界相机的nearClipPlane）")]
        public float worldPlaneDistance = 10f;
        
        [Header("弹回动画设置")]
        [Tooltip("是否启用弹回动画（拖到不可放置区域时）")]
        public bool enableBounceBack = true;
        
        [Tooltip("弹回动画时长")]
        public float bounceDuration = 0.3f;
        
        [Tooltip("弹回动画缓动类型")]
        public Ease bounceEase = Ease.OutBack;
        
        // 内部变量
        private RectTransform m_rectTransform;
        private CanvasGroup m_canvasGroup;
        private Canvas m_canvas;
        private GameObject m_previewInstance; // 世界中的预览对象
        private Vector2 m_originalPosition; // UI原始位置
        private bool m_isDragging = false;
        private Tween m_bounceTween; // 弹回动画
        
        // 事件回调
        /// <summary>
        /// 拖拽开始事件 (dragItemId)
        /// </summary>
        public event Action<int> OnDragBegin;
        
        /// <summary>
        /// 拖拽失败事件（用于恢复UI显示）
        /// </summary>
        public event Action<int> OnDragFailed;
        
        /// <summary>
        /// 拖拽中事件 (dragItemId, worldPosition, canPlace)
        /// canPlace: 是否可以放置（由外部通过SetCanPlace设置）
        /// </summary>
        public event Action<int, Vector2, bool> OnDragUpdate;
        
        /// <summary>
        /// 拖拽结束事件 (dragItemId, worldPosition, isSuccess)
        /// isSuccess: 是否成功放置（由外部决定）
        /// </summary>
        public event Action<int, Vector2, bool> OnDragEnd;
        
        // 外部可以设置是否可以放置（用于更新预览颜色）
        private bool m_canPlace = false;
        
        private void Awake()
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 查找Canvas
            m_canvas = GetComponentInParent<Canvas>();
            if (m_canvas == null)
            {
                m_canvas = FindObjectOfType<Canvas>();
            }
            
            // 查找世界相机（排除UI相机）
            if (worldCamera == null)
            {
                // 优先查找Main Camera
                worldCamera = Camera.main;
                
                // 如果Main Camera不存在或者是UI相机，查找其他相机
                if (worldCamera == null || (m_canvas != null && m_canvas.worldCamera == worldCamera))
                {
                    // 查找所有相机，排除UI相机
                    Camera[] cameras = FindObjectsOfType<Camera>();
                    foreach (var cam in cameras)
                    {
                        if (cam != m_canvas?.worldCamera && cam.tag != "UICamera")
                        {
                            worldCamera = cam;
                            break;
                        }
                    }
                }
                
                // 如果还是没找到，使用第一个相机
                if (worldCamera == null)
                {
                    worldCamera = FindObjectOfType<Camera>();
                }
            }
            
            // 设置默认平面距离
            if (worldPlaneDistance <= 0 && worldCamera != null)
            {
                worldPlaneDistance = worldCamera.nearClipPlane + 10f;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Log.Info($"WorldDragDrop.OnBeginDrag: 开始拖拽 - ItemId={dragItemId}, ScreenPos={eventData.position}");
            
            m_isDragging = true;
            
            // 记录原始位置
            m_originalPosition = m_rectTransform.anchoredPosition;
            Log.Info($"WorldDragDrop.OnBeginDrag: 原始位置={m_originalPosition}");
            
            // 设置透明度
            m_canvasGroup.alpha = dragAlpha;
            m_canvasGroup.blocksRaycasts = false;
            
            // 创建世界预览对象
            CreateWorldPreview();
            
            // 设置为最上层
            transform.SetAsLastSibling();
            
            // 触发开始事件（关键：这里触发，Helper会开始显示高亮）
            OnDragBegin?.Invoke(dragItemId);
            
            // 如果是最后一个，隐藏UI（在拖拽时隐藏）
            // 这个逻辑由外部通过DragItemData管理
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!m_isDragging)
            {
                Log.Warning("WorldDragDrop.OnDrag: 未处于拖拽状态");
                return;
            }
            
            // 更新UI位置（跟随鼠标移动）
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
            
            // 更新世界预览位置（关键：跟随鼠标移动）
            Vector2 worldPos = GetWorldPosition(eventData);
            
            // 更新预览位置
            if (m_previewInstance != null)
            {
                m_previewInstance.transform.position = new Vector3(worldPos.x, worldPos.y, worldZDepth);
            }
            
            // 触发更新事件（关键：这里触发，Helper会更新高亮显示，跟随鼠标移动）
            OnDragUpdate?.Invoke(dragItemId, worldPos, m_canPlace);
            
            // 更新预览颜色
            UpdatePreviewColor(m_canPlace);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDragging)
            {
                Log.Warning("WorldDragDrop.OnEndDrag: 未处于拖拽状态");
                return;
            }
            
            Log.Info($"WorldDragDrop.OnEndDrag: 拖拽结束 - ItemId={dragItemId}, ScreenPos={eventData.position}");
            
            m_isDragging = false;
            
            // 恢复透明度
            m_canvasGroup.alpha = 1f;
            m_canvasGroup.blocksRaycasts = true;
            
            // 获取最终世界位置（目标点）
            Vector2 worldPos = GetWorldPosition(eventData);
            
            // 添加日志
            Log.Info($"WorldDragDrop.OnEndDrag: 最终位置 - WorldPos={worldPos}, CanPlace={m_canPlace}");
            
            // 如果不可放置，执行弹回动画
            if (!m_canPlace && enableBounceBack)
            {
                Log.Info("WorldDragDrop.OnEndDrag: 位置不可放置，执行弹回动画");
                BounceBack();
                // 触发失败事件（用于恢复UI显示）
                OnDragFailed?.Invoke(dragItemId);
            }
            else
            {
                // 恢复UI位置
                m_rectTransform.anchoredPosition = m_originalPosition;
            }
            
            // 触发结束事件（关键：这里触发，Helper会处理放置逻辑并清除高亮）
            OnDragEnd?.Invoke(dragItemId, worldPos, m_canPlace);
            
            // 销毁预览
            DestroyWorldPreview();
        }
        
        /// <summary>
        /// 弹回动画（拖到不可放置区域时）
        /// </summary>
        private void BounceBack()
        {
            // 停止之前的动画
            if (m_bounceTween != null && m_bounceTween.IsActive())
            {
                m_bounceTween.Kill();
            }
            
            // 执行弹回动画（使用DoTween的UI扩展方法）
            m_bounceTween = DOTween.To(
                () => m_rectTransform.anchoredPosition,
                pos => m_rectTransform.anchoredPosition = pos,
                m_originalPosition,
                bounceDuration
            )
            .SetEase(bounceEase)
            .SetTarget(m_rectTransform)
            .OnComplete(() => {
                m_bounceTween = null;
            });
        }
        
        /// <summary>
        /// 设置是否可以放置（由外部调用，用于更新预览颜色）
        /// </summary>
        public void SetCanPlace(bool canPlace)
        {
            m_canPlace = canPlace;
            UpdatePreviewColor(canPlace);
        }
        
        /// <summary>
        /// 创建世界预览对象
        /// </summary>
        private void CreateWorldPreview()
        {
            Vector2 worldPos = GetWorldPosition(Input.mousePosition);
            
            if (previewPrefab != null)
            {
                // 使用指定的预览预制体
                m_previewInstance = Instantiate(previewPrefab);
            }
            else
            {
                // 创建一个简单的预览对象（使用当前GameObject的Sprite）
                m_previewInstance = new GameObject("DragPreview");
                var spriteRenderer = GetComponent<Image>();
                if (spriteRenderer != null)
                {
                    var previewSprite = m_previewInstance.AddComponent<SpriteRenderer>();
                    previewSprite.sprite = spriteRenderer.sprite;
                    previewSprite.color = new Color(1f, 1f, 1f, dragAlpha);
                }
            }
            
            if (m_previewInstance != null)
            {
                m_previewInstance.transform.position = new Vector3(worldPos.x, worldPos.y, worldZDepth);
                
                // 设置预览为半透明
                SetPreviewAlpha(dragAlpha);
            }
        }
        
        /// <summary>
        /// 更新预览颜色（绿色=可放置，红色=不可放置）
        /// </summary>
        private void UpdatePreviewColor(bool canPlace)
        {
            if (m_previewInstance == null) return;
            
            Color targetColor = canPlace ? Color.green : Color.red;
            targetColor.a = dragAlpha;
            
            var renderers = m_previewInstance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.color = targetColor;
                }
            }
        }
        
        /// <summary>
        /// 设置预览透明度
        /// </summary>
        private void SetPreviewAlpha(float alpha)
        {
            if (m_previewInstance == null) return;
            
            var renderers = m_previewInstance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }
        
        /// <summary>
        /// 销毁世界预览对象
        /// </summary>
        private void DestroyWorldPreview()
        {
            if (m_previewInstance != null)
            {
                Destroy(m_previewInstance);
                m_previewInstance = null;
            }
        }
        
        /// <summary>
        /// 获取世界坐标（使用当前鼠标位置）
        /// </summary>
        public Vector2 GetWorldPosition()
        {
            return GetWorldPosition(Input.mousePosition);
        }
        
        /// <summary>
        /// 获取世界坐标（使用PointerEventData）
        /// </summary>
        private Vector2 GetWorldPosition(PointerEventData eventData)
        {
            return GetWorldPosition(eventData.position);
        }
        
        /// <summary>
        /// 获取世界坐标（使用屏幕坐标）
        /// </summary>
        public Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            if (worldCamera == null) return Vector2.zero;
            
            Vector3 worldPos;
            
            // 对于正交相机，直接使用ScreenToWorldPoint
            if (worldCamera.orthographic)
            {
                worldPos = worldCamera.ScreenToWorldPoint(
                    new Vector3(screenPosition.x, screenPosition.y, worldPlaneDistance)
                );
            }
            else
            {
                // 对于透视相机，使用Raycast方式
                Ray ray = worldCamera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
                
                // 计算与Z=worldZDepth平面的交点
                if (Mathf.Abs(ray.direction.z) > 0.0001f)
                {
                    float distance = (worldZDepth - ray.origin.z) / ray.direction.z;
                    worldPos = ray.origin + ray.direction * distance;
                }
                else
                {
                    // 如果方向z为0，使用平面距离
                    worldPos = worldCamera.ScreenToWorldPoint(
                        new Vector3(screenPosition.x, screenPosition.y, worldPlaneDistance)
                    );
                }
            }
            
            return new Vector2(worldPos.x, worldPos.y);
        }
        
        /// <summary>
        /// 设置拖拽项ID
        /// </summary>
        public void SetDragItemId(int id)
        {
            dragItemId = id;
        }
        
        /// <summary>
        /// 获取当前是否正在拖拽
        /// </summary>
        public bool IsDragging => m_isDragging;
        
        private void OnDestroy()
        {
            // 清理动画
            if (m_bounceTween != null && m_bounceTween.IsActive())
            {
                m_bounceTween.Kill();
            }
        }
    }
}
