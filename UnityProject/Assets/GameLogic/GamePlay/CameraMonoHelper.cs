using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameLogic
{
    public class CameraMonoHelper : SerializedMonoBehaviour
    {
        [Title("Camera Settings")]
        [SerializeField] private Camera targetCamera;
        
        [Title("Target Object Settings")]
        [SerializeField] private Transform targetRootObject;
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        
        [Title("Fit Options")]
        [SerializeField] private bool fitToHeight = true; // 否则以宽度适配
        [SerializeField] private float padding = 0f; // 额外的边距

        private void OnEnable()
        {
            if (targetCamera == null) 
                targetCamera = Camera.main;
        }

        void Start()
        {
            // 可选：启动时自动适配
            // FitCameraToRootBounds();
        }

        #region 转换为2D相机
        
        [Button("转换为2D相机", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 1f)]
        public void ConvertTo2DCamera()
        {
            if (targetCamera == null)
            {
                Debug.LogError("目标相机未设置！");
                return;
            }

            // 设置为正交模式
            targetCamera.orthographic = true;
            
            // 设置合适的正交尺寸
            targetCamera.orthographicSize = 5f;
            
            // 重置旋转为2D视角
            targetCamera.transform.rotation = Quaternion.identity;
            
            // 确保Z轴位置负值（相机在场景后方）
            Vector3 pos = targetCamera.transform.position;
            if (pos.z >= 0)
            {
                pos.z = -10f;
                targetCamera.transform.position = pos;
            }
            
            // 设置背景颜色
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = Color.black;

            Debug.Log($"相机 '{targetCamera.name}' 已转换为2D相机模式");
        }
        
        #endregion

        #region 适配到SpriteRenderer

        [Button("适配到SpriteRenderer", ButtonSizes.Large)]
        [GUIColor(0.4f, 1f, 0.4f)]
        [ShowIf("@targetSpriteRenderer != null")]
        public void FitCameraToSprite()
        {
            if (targetCamera == null || targetSpriteRenderer == null)
            {
                Debug.LogWarning("相机或SpriteRenderer未设置！");
                return;
            }

            if (!targetCamera.orthographic)
            {
                Debug.LogWarning("相机不是正交模式，请先转换为2D相机！");
                return;
            }

            // 获取经过Scale变换后的物体边界大小
            Vector3 worldSize = targetSpriteRenderer.bounds.size;
            float worldHeight = worldSize.y;
            float worldWidth = worldSize.x;

            float targetSize = CalculateOrthographicSize(worldWidth, worldHeight);

            targetCamera.orthographicSize = targetSize;

            // 将相机中心对准物体中心
            CenterCameraOnBounds(targetSpriteRenderer.bounds);
            
            Debug.Log($"相机已适配到SpriteRenderer '{targetSpriteRenderer.name}'");
        }

        #endregion

        #region 适配到根物体Bounds

        [Button("适配到根物体Bounds", ButtonSizes.Large)]
        [GUIColor(1f, 0.7f, 0.3f)]
        [ShowIf("@targetRootObject != null")]
        public void FitCameraToRootBounds()
        {
            if (targetCamera == null || targetRootObject == null)
            {
                Debug.LogWarning("相机或根物体未设置！");
                return;
            }

            if (!targetCamera.orthographic)
            {
                Debug.LogWarning("相机不是正交模式，请先转换为2D相机！");
                return;
            }

            // 计算根物体及其所有子物体的总边界
            Bounds totalBounds = CalculateTotalBounds(targetRootObject);

            if (totalBounds.size == Vector3.zero)
            {
                Debug.LogWarning("根物体没有可用的Renderer边界！");
                return;
            }

            float worldHeight = totalBounds.size.y;
            float worldWidth = totalBounds.size.x;

            float targetSize = CalculateOrthographicSize(worldWidth, worldHeight);

            targetCamera.orthographicSize = targetSize;

            // 将相机中心对准总边界中心
            CenterCameraOnBounds(totalBounds);
            
            Debug.Log($"相机已适配到根物体 '{targetRootObject.name}' 的Bounds (Size: {totalBounds.size})");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算包含所有子物体的总边界
        /// </summary>
        private Bounds CalculateTotalBounds(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
                return new Bounds(root.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        /// <summary>
        /// 根据宽高计算正交相机尺寸
        /// </summary>
        private float CalculateOrthographicSize(float worldWidth, float worldHeight)
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float targetSize;

            if (fitToHeight)
            {
                // 以高度适配
                targetSize = (worldHeight / 2.0f) + padding;
            }
            else
            {
                // 以宽度适配
                targetSize = (worldWidth / (2.0f * screenAspect)) + padding;
            }

            return targetSize;
        }

        /// <summary>
        /// 将相机中心对准指定边界
        /// </summary>
        private void CenterCameraOnBounds(Bounds bounds)
        {
            targetCamera.transform.position = new Vector3(
                bounds.center.x,
                bounds.center.y,
                targetCamera.transform.position.z
            );
        }

        #endregion

        #region 编辑器辅助

        [Button("自动查找组件")]
        [PropertyOrder(-1)]
        private void AutoFindComponents()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            
            if (targetSpriteRenderer == null)
                targetSpriteRenderer = GetComponent<SpriteRenderer>();
            
            if (targetRootObject == null)
                targetRootObject = transform;
            
            Debug.Log("组件自动查找完成");
        }

        #endregion
    }
}