using System;
using Sirenix.OdinInspector;

namespace GameLogic
{
    using UnityEngine;

    public class CameraFitToObject : MonoBehaviour
    {
        public Camera targetCamera;
        public SpriteRenderer targetSpriteRenderer; // 或者使用 public Transform targetTransform; 并手动输入尺寸

        private void OnEnable()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetSpriteRenderer == null) targetSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            FitCameraToObject();
        }
        public bool fitToHeight = true; // 否则以宽度适配

        [Button("Fit to Object")] // 这允许你在Inspector中右键点击脚本组件执行此方法
        public void FitCameraToObject()
        {
            if (targetCamera == null || targetSpriteRenderer == null || !targetCamera.orthographic)
            {
                Debug.LogWarning("Components not set up correctly.");
                return;
            }

            // 关键：获取经过Scale变换后的物体边界大小
            Vector3 worldSize = targetSpriteRenderer.bounds.size;
            float worldHeight = worldSize.y;
            float worldWidth = worldSize.x;

            float screenAspect = (float)Screen.width / Screen.height;
            float targetSize;

            if (fitToHeight)
            {
                // 以高度适配
                targetSize = worldHeight / 2.0f;
            }
            else
            {
                // 以宽度适配
                targetSize = worldWidth / (2.0f * screenAspect);
            }

            targetCamera.orthographicSize = targetSize;

            // 将相机中心对准物体中心
            targetCamera.transform.position = new Vector3(
                targetSpriteRenderer.bounds.center.x,
                targetSpriteRenderer.bounds.center.y,
                targetCamera.transform.position.z
            );
        }
    }
}