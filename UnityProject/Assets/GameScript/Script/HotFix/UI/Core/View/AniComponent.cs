using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace AION.CoreFramework
{
    /// <summary>
    /// 界面动画组件
    /// </summary>
    public abstract class AniComponent : SerializedMonoBehaviour
    {
        /// <summary>
        /// 动画播放，当动画执行完调用CallWhenFinished.
        /// </summary>
        public abstract void Animate(Transform target, Action callWhenFinished);


        
        protected Camera uiCamera;
        
        private CanvasScaler scaler;
        private Canvas canvas;


        public float duration;
        
        private void Awake()
        {
            // uiCamera = Get
        }

        public float GetCanvasScaleRadio()
        {
            if (scaler == null)
            {
                return 0;
            }

            float radio = 0;

            float screenHeight = Screen.height;
            float screenWidth = Screen.width;

            if (scaler.matchWidthOrHeight == 0)
            {
                radio = scaler.referenceResolution.x * 1f / screenWidth * 1f;
            }
            else if (scaler.matchWidthOrHeight == 1)
            {
                radio = scaler.referenceResolution.y * 1f / screenHeight * 1f;
            }
            // Debug.Log(radio);
            // Debug.Log(
            //     $"screenHeight : {screenHeight} screenWidth:{screenWidth} ");

            return radio;
        }
        public float GetTopDistance(RectTransform rectTransform)
        {
            // 获取UI元素在屏幕上的边界
            Vector3[] corners = new Vector3[4];
            //Get the corners of the calculated rectangle in world space.
            rectTransform.GetWorldCorners(corners);
            // 计算UI元素上面的Y坐标
            float topY = corners[0].y;
            //在这种模式下，UI元素是相对于摄像机进行渲染的，但是其世界坐标并不直接映射到屏幕坐标上。
            //因此，使用rectTransform.GetWorldCorners(corners)获取的坐标是相对于Canvas所在的坐标系的世界坐标，而不是屏幕坐标。
            //如果想要获取UI元素在屏幕上的边界坐标，需要将UI元素的世界坐标转换为屏幕坐标，可以使用Camera的WorldToScreenPoint方法。
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // 转换UI元素的世界坐标为屏幕坐标
                // bottomY = Camera.main.WorldToScreenPoint(corners[1]).y;
                topY = uiCamera.WorldToScreenPoint(corners[0]).y;
            }
            //这种模式，UI元素的世界坐标与屏幕坐标是一致的
            else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
            }
            //在这种模式下，UI元素是直接放置在世界空间中的，其坐标是真实的世界坐标。
            else if (canvas.renderMode == RenderMode.WorldSpace)
            {
            }

            float radio = 0;


            // 计算屏幕底部的Y坐标
            float screenHeight = Screen.height;
            float screenBottomY = 0; // 屏幕底部Y坐标

            // 计算UI到屏幕底部的距离
            float distanceToTop = screenHeight - topY;

            return distanceToTop * GetCanvasScaleRadio();
        }

        /// <summary>
        /// 获取UI上边缘（左上角）顶部到屏幕底部的距离
        /// </summary>
        /// <returns></returns>
        public float GetBottomDistance(RectTransform rectTransform)
        {
            // 获取UI元素在屏幕上的边界
            Vector3[] corners = new Vector3[4];
            //Get the corners of the calculated rectangle in world space.
            rectTransform.GetWorldCorners(corners);


            // 计算UI元素上面的Y坐标
            float bottomY = corners[1].y;
            //在这种模式下，UI元素是相对于摄像机进行渲染的，但是其世界坐标并不直接映射到屏幕坐标上。
            //因此，使用rectTransform.GetWorldCorners(corners)获取的坐标是相对于Canvas所在的坐标系的世界坐标，而不是屏幕坐标。
            //如果想要获取UI元素在屏幕上的边界坐标，需要将UI元素的世界坐标转换为屏幕坐标，可以使用Camera的WorldToScreenPoint方法。
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // 转换UI元素的世界坐标为屏幕坐标
                bottomY = uiCamera.WorldToScreenPoint(corners[1]).y;
            }
            //这种模式，UI元素的世界坐标与屏幕坐标是一致的
            else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
            }
            //在这种模式下，UI元素是直接放置在世界空间中的，其坐标是真实的世界坐标。
            else if (canvas.renderMode == RenderMode.WorldSpace)
            {
            }


            float screenBottomY = 0; // 屏幕底部Y坐标

            // 计算UI到屏幕底部的距离
            float distanceToBottom = bottomY - screenBottomY;


            return GetCanvasScaleRadio() * distanceToBottom;
        }
    }
}