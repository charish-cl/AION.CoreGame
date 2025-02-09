using UnityEngine;
namespace GameDevKit
{
    /// <summary>
    /// 绘图调试工具
    /// </summary>
    public class FDraw
    {
        /// <summary>
        /// 2d画圆，3d把31行改成x，0，z就好
        /// </summary>
        /// <param name="m_Transform"></param>
        /// <param name="m_Color"></param>
        /// <param name="m_Radius"></param>
        /// <param name="m_Theta">值越低圆环越平滑 0.1</param>
        public static void DrawCircle(Transform m_Transform, Color m_Color, float m_Radius, float m_Theta=0.1f)
        {
            if (m_Theta < 0.0001f) m_Theta = 0.0001f;
            // 设置矩阵
            Matrix4x4 defaultMatrix = Gizmos.matrix;
            Gizmos.matrix = m_Transform.localToWorldMatrix;

            // 设置颜色
            Color defaultColor = Gizmos.color;
            Gizmos.color = m_Color;
            // 绘制圆环
            Vector3 beginPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;
            for (float theta = 0; theta < 2 * Mathf.PI; theta += m_Theta)
            {
                float x = m_Radius * Mathf.Cos(theta);
                float z = m_Radius * Mathf.Sin(theta);
                Vector3 endPoint = new Vector3(x, z);
                if (theta == 0)
                {
                    firstPoint = endPoint;
                }
                else
                {
                    Gizmos.DrawLine(beginPoint, endPoint);
                }
                beginPoint = endPoint;
            }
            // 绘制最后一条线段
            Gizmos.DrawLine(firstPoint, beginPoint);
            // 恢复默认颜色
            Gizmos.color = defaultColor;
            // 恢复默认矩阵
            Gizmos.matrix = defaultMatrix;
        }

        /// <summary>
        /// 绘制一个矩形或正方形
        /// </summary>
        /// <param name="color"></param>
        /// <param name="fRect></param>
        public static void GimzoDrawRectangle(Color color, FRect fRect)
        {
        
            var start = fRect.center + new Vector2(-fRect.width / 2, -fRect.height / 2);
            Gizmos.color = Color.green;
            var a = start;
            var b = start + new Vector2(fRect.width, 0);
            var c = start + new Vector2(fRect.width, 0) + new Vector2(0, fRect.height);
            var d = start + new Vector2(0, fRect.height);
            
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
        public static void DebugDrawRectangle(Vector3 minXY, Vector3 maxXY, Color color, float duration) {
            Debug.DrawLine(new Vector3(minXY.x, minXY.y), new Vector3(maxXY.x, minXY.y), color, duration);
            Debug.DrawLine(new Vector3(minXY.x, minXY.y), new Vector3(minXY.x, maxXY.y), color, duration);
            Debug.DrawLine(new Vector3(minXY.x, maxXY.y), new Vector3(maxXY.x, maxXY.y), color, duration);
            Debug.DrawLine(new Vector3(maxXY.x, minXY.y), new Vector3(maxXY.x, maxXY.y), color, duration);
        }
        
       
    }
}