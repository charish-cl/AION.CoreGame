using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace AION.CoreFramework
{
    public class CircleImage : Image
    {
        // 圆形由多少块三角面片拼成
        [SerializeField] private int _segments = 100;

        // 控制圆形显示比例
        [SerializeField, Range(0f, 1f)] private float _fillPercent = 1f;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            // 清除顶点数据
            toFill.Clear();
            int realSegments = (int)(_segments * _fillPercent);
            AddVert(toFill, realSegments);
            AddTriangle(toFill, realSegments);
        }

        // 添加顶点
        private void AddVert(VertexHelper toFill, int realSegments)
        {
            // 获取当前图片的外层uv
            var uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : new Vector4(0, 0, 1, 1);

            var width = rectTransform.rect.width;
            var height = rectTransform.rect.height;
            var uvWidth = uv.z - uv.x;
            var uvHeight = uv.w - uv.y;

            // 获取uv中心点
            Vector2 uvCenter = new Vector2(uv.x + uvWidth * 0.5f, uv.y + uvHeight * 0.5f);
            // 计算UI中心点
            Vector2 originPos = new Vector2((0.5f - rectTransform.pivot.x) * width,
                (0.5f - rectTransform.pivot.y) * height);

            // 每个三角形的弧度
            var radian = (2 * Mathf.PI) / _segments;
            // 整个圆形的半径
            var radius = width * 0.5f;

            // 创建圆心顶点
            UIVertex origin = new();
            origin.color = color;
            origin.position = originPos;
            origin.uv0 = uvCenter;
            toFill.AddVert(origin);

            // 顶点总数
            var vertexCount = realSegments + 1;
            // 当前弧度
            var curRadian = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                // 计算每个三角形面片的顶点坐标
                var x = Mathf.Cos(curRadian) * radius;
                var y = Mathf.Sin(curRadian) * radius;
                curRadian += radian;
                // 添加顶点
                UIVertex vertexTemp = new();
                vertexTemp.color = color;
                vertexTemp.position = new Vector2(x, y) + originPos;
                vertexTemp.uv0 = new Vector2((x / width + 0.5f) * uvWidth + uv.x, (y / height + 0.5f) * uvHeight + uv.y);
                toFill.AddVert(vertexTemp);
            }
        }

        // 添加三角形
        private static void AddTriangle(VertexHelper toFill, int realSegments)
        {
            for (int i = 1; i <= realSegments; i++)
            {
                toFill.AddTriangle(i, 0, i + 1);
            }
        }

        /// <summary>
        /// 重写该方法用于定义可点击区域
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="eventCamera"></param>
        /// <returns></returns>
        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint);
            localPoint = localPoint - new Vector2((.5f - rectTransform.pivot.x) * rectTransform.rect.width,
                (.5f - rectTransform.pivot.y) * rectTransform.rect.height); //根据pivot变换坐标使圆心为(0,0)，以方便判断是否点击在圆内
            return localPoint.magnitude < rectTransform.rect.width / 2 && localPoint.magnitude < rectTransform.rect.height / 2;
        }
    }
}
