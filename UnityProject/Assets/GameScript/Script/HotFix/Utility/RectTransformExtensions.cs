using UnityEngine;

public static class RectTransformExtensions
{
    public static void SetAnchorPositionX(this RectTransform rectTransform, float x)
    {
        rectTransform.anchoredPosition = new Vector2(x,rectTransform.anchoredPosition.y);
    }
    public static void SetAnchorPositionY(this RectTransform rectTransform, float y)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x,y);
    }
    public static void SetTopPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为上角
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        // 设置位置为父对象的顶部，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetBottomPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为下角
        rectTransform.pivot = new Vector2(0.5f, 0);
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 0);
        // 设置位置为父对象的底部，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetLeftPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为左角
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        // 设置位置为父对象的左边，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetRightPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为右角
        rectTransform.pivot = new Vector2(1, 0.5f);
        rectTransform.anchorMin = new Vector2(1, 0.5f);
        rectTransform.anchorMax = new Vector2(1, 0.5f);
        // 设置位置为父对象的右边，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetTopLeftPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为左上角
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        // 设置位置为父对象的左上角，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetTopRightPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为右上角
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        // 设置位置为父对象的右上角，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetBottomLeftPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为左下角
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        // 设置位置为父对象的左下角，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetBottomRightPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为右下角
        rectTransform.pivot = new Vector2(1, 0);
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        // 设置位置为父对象的右下角，带偏移
        rectTransform.anchoredPosition = offset;
    }

    public static void SetCenterPosition(this RectTransform rectTransform, Vector2 offset = default(Vector2))
    {
        // 设置pivot为中心
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        // 设置位置为父对象的中心，带偏移
        rectTransform.anchoredPosition = offset;
    }
}
