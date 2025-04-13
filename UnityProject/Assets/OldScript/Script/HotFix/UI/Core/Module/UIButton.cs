using UnityEngine;
using UnityEngine.UI;

namespace AION.CoreFramework
{
    /// <summary>
    /// 这里用PhysicalColider适配多边形按钮
    /// </summary>
    public class UIButton:Button
    {
        // public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        // {
        //     if ((Object) this.Polygon == (Object) null)
        //         return true;
        //     Vector3 worldPoint;
        //     RectTransformUtility.ScreenPointToWorldPointInRectangle(this.rectTransform, screenPoint, eventCamera, out worldPoint);
        //     return this.Polygon.OverlapPoint((Vector2) worldPoint);
        // }
        
        // /// <summary>
        // /// 重写该方法用于定义可点击区域
        // /// </summary>
        // /// <param name="screenPoint"></param>
        // /// <param name="eventCamera"></param>
        // /// <returns></returns>
        // public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        // {
        //     RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint);
        //     localPoint = localPoint - new Vector2((.5f - rectTransform.pivot.x) * rectTransform.rect.width,
        //         (.5f - rectTransform.pivot.y) * rectTransform.rect.height); //根据pivot变换坐标使圆心为(0,0)，以方便判断是否点击在圆内
        //     return localPoint.magnitude < rectTransform.rect.width / 2 && localPoint.magnitude < rectTransform.rect.height / 2;
        // }
    }
}